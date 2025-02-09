#version 300 es

precision highp float;

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in uint color;
//layout (location = 2) in vec4 light_rgbi;
//layout (location = 3) in uint face_direction;
//layout (location = 4) in float sun_intensity;

uniform mat4 m, v, p;

uniform vec4 sunlight_color;

// should match enum Direction in direction.h
const uint NORTH = 0U;
const uint SOUTH = 1U;
const uint EAST = 2U;
const uint WEST = 3U;
const uint UP = 4U;
const uint DOWN = 5U;

out vec4 v_color;
out vec2 v_uv;
out vec3 v_viewpos;

void main() {
    gl_Position = p * v * m * vec4(position, 1.0);

    // 'color' is packed:
    // - (3) face direction, NORTH, SOUTH, EAST, WEST, UP, DOWN
    // - (4) sunlight intensity
    // - (4) R
    // - (4) G
    // - (4) B
    // - (4) intensity
    vec3 light = vec3(
    (float((color & 0x0F000U) >> 12U)) / 16.0,
    (float((color & 0x00F00U) >>  8U)) / 16.0,
    (float((color & 0x000F0U) >>  4U)) / 16.0
    ) * (float((color & 0x0000FU)) / 16.0);
    
    // vec3 light = (light_rgbi.xyz / 16.0) * (light_rgbi.w / 16.0);
    light += vec3(sunlight_color.rgb) * (float(((color & 0xF0000U) >> 16U)) / 16.0);
    // adjust light range to prevent entirely black lighting
    const float min_light = 0.005;
    light *= vec3(1.0 - min_light);
    light += vec3(min_light);

    // adjust lighting for direction
    uint direction = (color & 0x700000U) >> 20U;
    if (direction == UP) {
        light *= 1.0;
    } else if (direction == EAST || direction == WEST) {
        light *= 0.85;
    } else if (direction == NORTH || direction == SOUTH) {
        light *= 0.7;
    } else {
        light *= 0.6;
    }

    // set and gamma correct
    v_color = vec4(light, 1.0);
    v_color = vec4(pow(v_color.rgb, vec3(1.0 / 2.2)), v_color.a);

    v_uv = uv;
    v_viewpos = ((v * m) * vec4(position, 1.0)).xyz;
}