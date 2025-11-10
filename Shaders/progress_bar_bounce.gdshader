shader_type canvas_item;
#include "res://Shaders/random.gdshaderinc"
#include "res://Shaders/utilities.gdshaderinc"

uniform float interval : hint_range(0.0, 1.0) = 0.1;

void fragment() {
    vec2 uv = UV;
    float x = floor(uv.x / TEXTURE_PIXEL_SIZE.x) + floor(TIME / interval) * interval;
	float offset = remap(fract_random(x), 0.0, 1.0, -0.75, 0.75);
    uv.y += TEXTURE_PIXEL_SIZE.y * round(offset);
    COLOR = texture(TEXTURE, uv);
}
