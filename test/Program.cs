// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.Utilities;

public static class Program
{
    static Dictionary<int, Dictionary<int, string>> Dics = [];

    public static unsafe void Main()
    {
        var v = Vector3.Zero;
        v = Vector3.Normalize(v);

        var aa = new Aa();
        var a = aa.GetA(1, 2);
        a.Value = 10;
        var b = aa.GetA(1, 2);

        var ss = new List<S>() { new(), new() };
        ss[0] = ss[0].SetValue(10);
        var s0 = ss[0];
        ss[0].SetFx(100);

        var dic = GetDic(5);
        dic[0] = "hello";

        dic = GetDic(5);
        dic[5] = "world";

        var ints = new int[50];

        var p1 = new UnsafeList<I2>() { new(1, 2), new(3, 4), new(5, 6) };
        var p2 = new UnsafeList<I2>(p1.Count);

        Utils.Memcpy(&p1.Data[1], &p2.Data[2], 1 * sizeof(I2));

        var ia = p2[0];
        var ib = p2[1];
        var pIc = &p2.Data[2];
        *pIc = new(7, 7);
        pIc->Value1 = 9;
        var ic = *pIc;

        var s = new S();
        s.SetFx(11);
        var pS = &s;

        var pSs = new UnsafeList<IntPtr>(5);
        var p = Utils.AllocT<S>(1);
        *p = new S().SetValue(10);
        pSs[2] = (IntPtr)p;
        var ts = (S*)(pSs[2]);
        var zero = (S*)(pSs[0]);
        if (zero == null) { }

        var iS = new UnsafeList<int>() { 2, 1, 5, 6 };
        iS.AsSpan().Sort();

        Console.ReadLine();
    }

    private static Dictionary<int, string> GetDic(int index)
    {
        if (Dics.TryGetValue(index, out var dic))
            return dic;
        Dics[index] = [];
        return Dics[index];
    }

    private static unsafe void GetPointer(void* p) { }
}

class A
{
    public int Value { get; set; } = -1;
}

class Aa
{
    A?[,] Values { get; } = new A?[3, 3];

    public A GetA(int x, int y)
    {
        var a = Values[x, y];
        if (a is null)
            a = Values[x, y] = new();
        return a;
    }
}

struct S
{
    public int Value { get; private set; }

    public S()
    {
        Value = 7;
    }

    public S SetValue(int value)
    {
        Value = value;
        return this;
    }

    public void SetFx(int value)
    {
        Value = value;
    }
}

struct I2
{
    public int Value1 { get; set; } = -1;
    public int Value2 { get; set; } = -2;

    public I2(int value1, int value2)
    {
        Value1 = value1;
        Value2 = value2;
    }
}
