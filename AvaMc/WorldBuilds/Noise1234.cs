// noise1234
//
// Author: Stefan Gustavson, 2003-2005
// Contact: stefan.gustavson@liu.se
//
// This code was GPL licensed until February 2011.
// As the original author of this code, I hereby
// release it into the public domain.
// Please feel free to use it for whatever you want.
// Credit is appreciated where appropriate, and I also
// appreciate being told where this code finds any use,
// but you may do as you like.

/*
 * This implementation is "Improved Noise" as presented by
 * Ken Perlin at Siggraph 2002. The 3D function is a direct port
 * of his Java reference code which was once publicly available
 * on www.noisemachine.com (although I cleaned it up, made it
 * faster and made the code more readable), but the 1D, 2D and
 * 4D functions were implemented from scratch by me.
 *
 * This is a backport to C of my improved noise class in C++
 * which was included in the Aqsis renderer project.
 * It is highly reusable without source code modifications.
 *
 */

namespace AvaMc.WorldBuilds;

public sealed class Noise1234
{
// Static data

/*
 * Permutation table. This is just a random jumble of all numbers 0-255,
 * repeated twice to avoid wrapping the index at 255 for each lookup.
 * This needs to be exactly the same for all instances on all platforms,
 * so it's easiest to just keep it as static explicit data.
 * This also removes the need for any initialisation of this class.
 *
 * Note that making this an int[] instead of a char[] might make the
 * code run faster on platforms with a high penalty for unaligned single
 * byte addressing. Intel x86 is generally single-byte-friendly, but
 * some other CPUs are faster with 4-aligned reads.
 * However, a char[] is smaller, which avoids cache trashing, and that
 * is probably the most important aspect on most architectures.
 * This array is accessed a *lot* by the noise functions.
 * A vector-valued noise over 3D accesses it 96 times, and a
 * float-valued 4D noise 64 times. We want this to fit in the cache!
 */
    static byte[] Perm { get; } =
    [
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    ];

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static int FastFloor(float x)
    {
        return ((int)(x) < (x)) ? ((int)x) : ((int)x - 1);
    }

    private static float Lerp(float t, float a, float b)
    {
        return ((a) + (t) * ((b) - (a)));
    }

    //---------------------------------------------------------------------

    /*
     * Helper functions to compute Gradients-dot-residualvectors (1D to 4D)
     * Note that these generate Gradients of more than unit length. To make
     * a close match with the value range of classic Perlin noise, the final
     * noise values need to be rescaled. To match the RenderMan noise in a
     * statistical sense, the approximate scaling values (empirically
     * determined from test renderings) are:
     * 1D noise needs rescaling with 0.188
     * 2D noise needs rescaling with 0.507
     * 3D noise needs rescaling with 0.936
     * 4D noise needs rescaling with 0.87
     * Note that these noise functions are the most practical and useful
     * signed version of Perlin noise. To return values according to the
     * RenderMan specification from the SL noise() and pnoise() functions,
     * the noise values need to be scaled and offset to [0,1], like this:
     * float SLnoise = (noise3(x,y,z) + 1.0) * 0.5;
     */

    private static float Grad1(int hash, float x)
    {
        int h = hash & 15;
        float Grad = 1.0f + (h & 7); // Gradient value 1.0, 2.0, ..., 8.0
        if ((h & 8) != 0)
            Grad = -Grad; // and a random sign for the Gradient
        return (Grad * x); // Multiply the Gradient with the distance
    }

    private static float Grad2(int hash, float x, float y)
    {
        int h = hash & 7; // Convert low 3 bits of hash code
        float u = h < 4 ? x : y; // into 8 simple Gradient directions,
        float v = h < 4 ? y : x; // and compute the dot product with (x,y).
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }

    private static float Grad3(int hash, float x, float y, float z)
    {
        int h = hash & 15; // Convert low 4 bits of hash code into 12 simple
        float u = h < 8 ? x : y; // Gradient directions, and compute dot product.
        float v =
            h < 4 ? y
            : h == 12 || h == 14 ? x
            : z; // Fix repeats at h = 12 to 15
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
    }

    private static float Grad4(int hash, float x, float y, float z, float t)
    {
        int h = hash & 31; // Convert low 5 bits of hash code into 32 simple
        float u = h < 24 ? x : y; // Gradient directions, and compute dot product.
        float v = h < 16 ? y : z;
        float w = h < 8 ? z : t;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
    }

    //---------------------------------------------------------------------
/** 1D float Perlin noise, SL "noise()"
 */
public static float Noise1( float x )
{
    int ix0, ix1;
    float fx0, fx1;
    float s, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    fx0 = x - ix0;       // Fractional part of x
    fx1 = fx0 - 1.0f;
    ix1 = ( ix0+1 ) & 0xff;
    ix0 = ix0 & 0xff;    // Wrap to 0..255

    s = Fade( fx0 );

    n0 = Grad1( Perm[ ix0 ], fx0 );
    n1 = Grad1( Perm[ ix1 ], fx1 );
    return 0.188f * ( Lerp( s, n0, n1 ) );
}

//---------------------------------------------------------------------
/** 1D float Perlin periodic noise, SL "pnoise()"
 */
public static float Pnoise1( float x, int px )
{
    int ix0, ix1;
    float fx0, fx1;
    float s, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    fx0 = x - ix0;       // Fractional part of x
    fx1 = fx0 - 1.0f;
    ix1 = (( ix0 + 1 ) % px) & 0xff; // Wrap to 0..px-1 *and* wrap to 0..255
    ix0 = ( ix0 % px ) & 0xff;      // (because px might be greater than 256)

    s = Fade( fx0 );

    n0 = Grad1( Perm[ ix0 ], fx0 );
    n1 = Grad1( Perm[ ix1 ], fx1 );
    return 0.188f * ( Lerp( s, n0, n1 ) );
}


//---------------------------------------------------------------------
/** 2D float Perlin noise.
 */
public static float Noise2( float x, float y )
{
    int ix0, iy0, ix1, iy1;
    float fx0, fy0, fx1, fy1;
    float s, t, nx0, nx1, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    iy0 = FastFloor( y ); // Integer part of y
    fx0 = x - ix0;        // Fractional part of x
    fy0 = y - iy0;        // Fractional part of y
    fx1 = fx0 - 1.0f;
    fy1 = fy0 - 1.0f;
    ix1 = (ix0 + 1) & 0xff;  // Wrap to 0..255
    iy1 = (iy0 + 1) & 0xff;
    ix0 = ix0 & 0xff;
    iy0 = iy0 & 0xff;
    
    t = Fade( fy0 );
    s = Fade( fx0 );

    nx0 = Grad2(Perm[ix0 + Perm[iy0]], fx0, fy0);
    nx1 = Grad2(Perm[ix0 + Perm[iy1]], fx0, fy1);
    n0 = Lerp( t, nx0, nx1 );

    nx0 = Grad2(Perm[ix1 + Perm[iy0]], fx1, fy0);
    nx1 = Grad2(Perm[ix1 + Perm[iy1]], fx1, fy1);
    n1 = Lerp(t, nx0, nx1);

    return 0.507f * ( Lerp( s, n0, n1 ) );
}

//---------------------------------------------------------------------
/** 2D float Perlin periodic noise.
 */
public static float Pnoise2( float x, float y, int px, int py )
{
    int ix0, iy0, ix1, iy1;
    float fx0, fy0, fx1, fy1;
    float s, t, nx0, nx1, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    iy0 = FastFloor( y ); // Integer part of y
    fx0 = x - ix0;        // Fractional part of x
    fy0 = y - iy0;        // Fractional part of y
    fx1 = fx0 - 1.0f;
    fy1 = fy0 - 1.0f;
    ix1 = (( ix0 + 1 ) % px) & 0xff;  // Wrap to 0..px-1 and wrap to 0..255
    iy1 = (( iy0 + 1 ) % py) & 0xff;  // Wrap to 0..py-1 and wrap to 0..255
    ix0 = ( ix0 % px ) & 0xff;
    iy0 = ( iy0 % py ) & 0xff;
    
    t = Fade( fy0 );
    s = Fade( fx0 );

    nx0 = Grad2(Perm[ix0 + Perm[iy0]], fx0, fy0);
    nx1 = Grad2(Perm[ix0 + Perm[iy1]], fx0, fy1);
    n0 = Lerp( t, nx0, nx1 );

    nx0 = Grad2(Perm[ix1 + Perm[iy0]], fx1, fy0);
    nx1 = Grad2(Perm[ix1 + Perm[iy1]], fx1, fy1);
    n1 = Lerp(t, nx0, nx1);

    return 0.507f * ( Lerp( s, n0, n1 ) );
}


//---------------------------------------------------------------------
/** 3D float Perlin noise.
 */
public static float Noise3( float x, float y, float z )
{
    int ix0, iy0, ix1, iy1, iz0, iz1;
    float fx0, fy0, fz0, fx1, fy1, fz1;
    float s, t, r;
    float nxy0, nxy1, nx0, nx1, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    iy0 = FastFloor( y ); // Integer part of y
    iz0 = FastFloor( z ); // Integer part of z
    fx0 = x - ix0;        // Fractional part of x
    fy0 = y - iy0;        // Fractional part of y
    fz0 = z - iz0;        // Fractional part of z
    fx1 = fx0 - 1.0f;
    fy1 = fy0 - 1.0f;
    fz1 = fz0 - 1.0f;
    ix1 = ( ix0 + 1 ) & 0xff; // Wrap to 0..255
    iy1 = ( iy0 + 1 ) & 0xff;
    iz1 = ( iz0 + 1 ) & 0xff;
    ix0 = ix0 & 0xff;
    iy0 = iy0 & 0xff;
    iz0 = iz0 & 0xff;
    
    r = Fade( fz0 );
    t = Fade( fy0 );
    s = Fade( fx0 );

    nxy0 = Grad3(Perm[ix0 + Perm[iy0 + Perm[iz0]]], fx0, fy0, fz0);
    nxy1 = Grad3(Perm[ix0 + Perm[iy0 + Perm[iz1]]], fx0, fy0, fz1);
    nx0 = Lerp( r, nxy0, nxy1 );

    nxy0 = Grad3(Perm[ix0 + Perm[iy1 + Perm[iz0]]], fx0, fy1, fz0);
    nxy1 = Grad3(Perm[ix0 + Perm[iy1 + Perm[iz1]]], fx0, fy1, fz1);
    nx1 = Lerp( r, nxy0, nxy1 );

    n0 = Lerp( t, nx0, nx1 );

    nxy0 = Grad3(Perm[ix1 + Perm[iy0 + Perm[iz0]]], fx1, fy0, fz0);
    nxy1 = Grad3(Perm[ix1 + Perm[iy0 + Perm[iz1]]], fx1, fy0, fz1);
    nx0 = Lerp( r, nxy0, nxy1 );

    nxy0 = Grad3(Perm[ix1 + Perm[iy1 + Perm[iz0]]], fx1, fy1, fz0);
    nxy1 = Grad3(Perm[ix1 + Perm[iy1 + Perm[iz1]]], fx1, fy1, fz1);
    nx1 = Lerp( r, nxy0, nxy1 );

    n1 = Lerp( t, nx0, nx1 );
    
    return 0.936f * ( Lerp( s, n0, n1 ) );
}

//---------------------------------------------------------------------
/** 3D float Perlin periodic noise.
 */
public static float Pnoise3( float x, float y, float z, int px, int py, int pz )
{
    int ix0, iy0, ix1, iy1, iz0, iz1;
    float fx0, fy0, fz0, fx1, fy1, fz1;
    float s, t, r;
    float nxy0, nxy1, nx0, nx1, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    iy0 = FastFloor( y ); // Integer part of y
    iz0 = FastFloor( z ); // Integer part of z
    fx0 = x - ix0;        // Fractional part of x
    fy0 = y - iy0;        // Fractional part of y
    fz0 = z - iz0;        // Fractional part of z
    fx1 = fx0 - 1.0f;
    fy1 = fy0 - 1.0f;
    fz1 = fz0 - 1.0f;
    ix1 = (( ix0 + 1 ) % px ) & 0xff; // Wrap to 0..px-1 and wrap to 0..255
    iy1 = (( iy0 + 1 ) % py ) & 0xff; // Wrap to 0..py-1 and wrap to 0..255
    iz1 = (( iz0 + 1 ) % pz ) & 0xff; // Wrap to 0..pz-1 and wrap to 0..255
    ix0 = ( ix0 % px ) & 0xff;
    iy0 = ( iy0 % py ) & 0xff;
    iz0 = ( iz0 % pz ) & 0xff;
    
    r = Fade( fz0 );
    t = Fade( fy0 );
    s = Fade( fx0 );

    nxy0 = Grad3(Perm[ix0 + Perm[iy0 + Perm[iz0]]], fx0, fy0, fz0);
    nxy1 = Grad3(Perm[ix0 + Perm[iy0 + Perm[iz1]]], fx0, fy0, fz1);
    nx0 = Lerp( r, nxy0, nxy1 );

    nxy0 = Grad3(Perm[ix0 + Perm[iy1 + Perm[iz0]]], fx0, fy1, fz0);
    nxy1 = Grad3(Perm[ix0 + Perm[iy1 + Perm[iz1]]], fx0, fy1, fz1);
    nx1 = Lerp( r, nxy0, nxy1 );

    n0 = Lerp( t, nx0, nx1 );

    nxy0 = Grad3(Perm[ix1 + Perm[iy0 + Perm[iz0]]], fx1, fy0, fz0);
    nxy1 = Grad3(Perm[ix1 + Perm[iy0 + Perm[iz1]]], fx1, fy0, fz1);
    nx0 = Lerp( r, nxy0, nxy1 );

    nxy0 = Grad3(Perm[ix1 + Perm[iy1 + Perm[iz0]]], fx1, fy1, fz0);
    nxy1 = Grad3(Perm[ix1 + Perm[iy1 + Perm[iz1]]], fx1, fy1, fz1);
    nx1 = Lerp( r, nxy0, nxy1 );

    n1 = Lerp( t, nx0, nx1 );
    
    return 0.936f * ( Lerp( s, n0, n1 ) );
}


//---------------------------------------------------------------------
/** 4D float Perlin noise.
 */

public static float Noise4( float x, float y, float z, float w )
{
    int ix0, iy0, iz0, iw0, ix1, iy1, iz1, iw1;
    float fx0, fy0, fz0, fw0, fx1, fy1, fz1, fw1;
    float s, t, r, q;
    float nxyz0, nxyz1, nxy0, nxy1, nx0, nx1, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    iy0 = FastFloor( y ); // Integer part of y
    iz0 = FastFloor( z ); // Integer part of y
    iw0 = FastFloor( w ); // Integer part of w
    fx0 = x - ix0;        // Fractional part of x
    fy0 = y - iy0;        // Fractional part of y
    fz0 = z - iz0;        // Fractional part of z
    fw0 = w - iw0;        // Fractional part of w
    fx1 = fx0 - 1.0f;
    fy1 = fy0 - 1.0f;
    fz1 = fz0 - 1.0f;
    fw1 = fw0 - 1.0f;
    ix1 = ( ix0 + 1 ) & 0xff;  // Wrap to 0..255
    iy1 = ( iy0 + 1 ) & 0xff;
    iz1 = ( iz0 + 1 ) & 0xff;
    iw1 = ( iw0 + 1 ) & 0xff;
    ix0 = ix0 & 0xff;
    iy0 = iy0 & 0xff;
    iz0 = iz0 & 0xff;
    iw0 = iw0 & 0xff;

    q = Fade( fw0 );
    r = Fade( fz0 );
    t = Fade( fy0 );
    s = Fade( fx0 );

    nxyz0 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz0 + Perm[iw0]]]], fx0, fy0, fz0, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz0 + Perm[iw1]]]], fx0, fy0, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz1 + Perm[iw0]]]], fx0, fy0, fz1, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz1 + Perm[iw1]]]], fx0, fy0, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );
        
    nx0 = Lerp ( r, nxy0, nxy1 );

    nxyz0 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz0 + Perm[iw0]]]], fx0, fy1, fz0, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz0 + Perm[iw1]]]], fx0, fy1, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz1 + Perm[iw0]]]], fx0, fy1, fz1, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz1 + Perm[iw1]]]], fx0, fy1, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );

    nx1 = Lerp ( r, nxy0, nxy1 );

    n0 = Lerp( t, nx0, nx1 );

    nxyz0 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz0 + Perm[iw0]]]], fx1, fy0, fz0, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz0 + Perm[iw1]]]], fx1, fy0, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz1 + Perm[iw0]]]], fx1, fy0, fz1, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz1 + Perm[iw1]]]], fx1, fy0, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );

    nx0 = Lerp ( r, nxy0, nxy1 );

    nxyz0 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz0 + Perm[iw0]]]], fx1, fy1, fz0, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz0 + Perm[iw1]]]], fx1, fy1, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz1 + Perm[iw0]]]], fx1, fy1, fz1, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz1 + Perm[iw1]]]], fx1, fy1, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );

    nx1 = Lerp ( r, nxy0, nxy1 );

    n1 = Lerp( t, nx0, nx1 );

    return 0.87f * ( Lerp( s, n0, n1 ) );
}

//---------------------------------------------------------------------
/** 4D float Perlin periodic noise.
 */

public static float Pnoise4( float x, float y, float z, float w,
                            int px, int py, int pz, int pw )
{
    int ix0, iy0, iz0, iw0, ix1, iy1, iz1, iw1;
    float fx0, fy0, fz0, fw0, fx1, fy1, fz1, fw1;
    float s, t, r, q;
    float nxyz0, nxyz1, nxy0, nxy1, nx0, nx1, n0, n1;

    ix0 = FastFloor( x ); // Integer part of x
    iy0 = FastFloor( y ); // Integer part of y
    iz0 = FastFloor( z ); // Integer part of y
    iw0 = FastFloor( w ); // Integer part of w
    fx0 = x - ix0;        // Fractional part of x
    fy0 = y - iy0;        // Fractional part of y
    fz0 = z - iz0;        // Fractional part of z
    fw0 = w - iw0;        // Fractional part of w
    fx1 = fx0 - 1.0f;
    fy1 = fy0 - 1.0f;
    fz1 = fz0 - 1.0f;
    fw1 = fw0 - 1.0f;
    ix1 = (( ix0 + 1 ) % px ) & 0xff;  // Wrap to 0..px-1 and wrap to 0..255
    iy1 = (( iy0 + 1 ) % py ) & 0xff;  // Wrap to 0..py-1 and wrap to 0..255
    iz1 = (( iz0 + 1 ) % pz ) & 0xff;  // Wrap to 0..pz-1 and wrap to 0..255
    iw1 = (( iw0 + 1 ) % pw ) & 0xff;  // Wrap to 0..pw-1 and wrap to 0..255
    ix0 = ( ix0 % px ) & 0xff;
    iy0 = ( iy0 % py ) & 0xff;
    iz0 = ( iz0 % pz ) & 0xff;
    iw0 = ( iw0 % pw ) & 0xff;

    q = Fade( fw0 );
    r = Fade( fz0 );
    t = Fade( fy0 );
    s = Fade( fx0 );

    nxyz0 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz0 + Perm[iw0]]]], fx0, fy0, fz0, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz0 + Perm[iw1]]]], fx0, fy0, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz1 + Perm[iw0]]]], fx0, fy0, fz1, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy0 + Perm[iz1 + Perm[iw1]]]], fx0, fy0, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );
        
    nx0 = Lerp ( r, nxy0, nxy1 );

    nxyz0 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz0 + Perm[iw0]]]], fx0, fy1, fz0, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz0 + Perm[iw1]]]], fx0, fy1, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz1 + Perm[iw0]]]], fx0, fy1, fz1, fw0);
    nxyz1 = Grad4(Perm[ix0 + Perm[iy1 + Perm[iz1 + Perm[iw1]]]], fx0, fy1, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );

    nx1 = Lerp ( r, nxy0, nxy1 );

    n0 = Lerp( t, nx0, nx1 );

    nxyz0 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz0 + Perm[iw0]]]], fx1, fy0, fz0, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz0 + Perm[iw1]]]], fx1, fy0, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz1 + Perm[iw0]]]], fx1, fy0, fz1, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy0 + Perm[iz1 + Perm[iw1]]]], fx1, fy0, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );

    nx0 = Lerp ( r, nxy0, nxy1 );

    nxyz0 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz0 + Perm[iw0]]]], fx1, fy1, fz0, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz0 + Perm[iw1]]]], fx1, fy1, fz0, fw1);
    nxy0 = Lerp( q, nxyz0, nxyz1 );
        
    nxyz0 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz1 + Perm[iw0]]]], fx1, fy1, fz1, fw0);
    nxyz1 = Grad4(Perm[ix1 + Perm[iy1 + Perm[iz1 + Perm[iw1]]]], fx1, fy1, fz1, fw1);
    nxy1 = Lerp( q, nxyz0, nxyz1 );

    nx1 = Lerp ( r, nxy0, nxy1 );

    n1 = Lerp( t, nx0, nx1 );

    return 0.87f * ( Lerp( s, n0, n1 ) );
}

//---------------------------------------------------------------------
}
