#version 300 es

precision highp float;

uniform sampler2D tex;

in vec4 v_color;
in vec2 v_uv;
in vec3 v_viewpos;

out vec4 frag_color;

const vec4 fog_color = vec4(0.5, 0.8, 0.9, 1.0);
const float fog_near = (12.0 * 16.0) - 24.0;
const float fog_far = (12.0 * 16.0) - 12.0;

void main() {
    vec4 tex_color = texture(tex, v_uv);
//    frag_color = vec4(tex_color.rgb, 1.0);
    frag_color = tex_color * v_color;
//     frag_color = vec4(1.0);
    float fog = smoothstep(fog_near, fog_far, length(v_viewpos));
    frag_color = mix(frag_color, fog_color, fog);
}