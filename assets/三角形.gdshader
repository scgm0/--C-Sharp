shader_type canvas_item;

uniform float side_length;

void fragment()
{
    // 等边三角形的高度
    float height = sqrt(3.0) / 2.0 * side_length;

    // 等边三角形的顶点坐标
    vec2 v0 = vec2(0.0, height / 2.0);
    vec2 v1 = vec2(-side_length / 2.0, -height / 2.0);
    vec2 v2 = vec2(side_length / 2.0, -height / 2.0);

    // 将片段位置转换为等边三角形局部坐标系
    vec2 localPos = UV - vec2(0.5, 0.5);

    // 检查片段是否在等边三角形内部
    if (dot(localPos - v0, normalize(v1 - v0)) >= 0.0 &&
        dot(localPos - v1, normalize(v2 - v1)) >= 0.0 &&
        dot(localPos - v2, normalize(v0 - v2)) >= 0.0)
    {
        COLOR = vec4(1.0, 0.0, 0.0, 1.0);  // 内部为红色
    }
    else
    {
        COLOR = vec4(0.0, 0.0, 0.0, 1.0);  // 外部为黑色
    }
}
