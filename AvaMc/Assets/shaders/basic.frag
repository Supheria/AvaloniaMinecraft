#version 300 es

precision highp float;

uniform sampler2D tex;

in vec4 v_color;
in vec2 v_uv;

out vec4 frag_color;

void main() {
    vec4 tex_color = texture(tex, v_uv);
//    frag_color = vec4(tex_color.rgb, 1.0);
    frag_color = tex_color;
//     frag_color = vec4(1.0);
}