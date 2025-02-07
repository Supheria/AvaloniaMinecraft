#version 300 es

precision highp float;

uniform sampler2D tex;
uniform bool use_tex;

uniform vec4 color;
uniform vec4 fog_color;
uniform float fog_near;
uniform float fog_far;

in vec2 v_uv;
in vec3 v_viewpos;

out vec4 frag_color;

void main() {
    frag_color = color * (use_tex ? texture(tex, v_uv) : vec4(1.0));
    
    float fog = smoothstep(fog_near, fog_far, length(v_viewpos));
    frag_color = mix(frag_color, fog_color, fog);
}