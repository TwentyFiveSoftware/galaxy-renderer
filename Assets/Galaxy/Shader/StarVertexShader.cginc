#include "UnityCG.cginc"
#include "TextureConsts.cginc"

struct Star
{
    float angular_position;
    float distance_to_center;
    float4 color;
};

struct v2f
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

uniform float time;
uniform float4 position_offset;
StructuredBuffer<Star> star_buffer;

uniform float bulge_radius;
uniform float galaxy_radius;
uniform float far_field_factor;
uniform float ellipse_a;
uniform float ellipse_b;
uniform float ellipse_tilt;
uniform float star_size;
uniform float velocity_factor;
uniform int use_constant_velocity;

float calculate_eccentricity(const float distance_to_center)
{
    if (distance_to_center < bulge_radius)
        return 1.0f + (distance_to_center / bulge_radius) * (ellipse_a - 1.0f);

    if (distance_to_center < galaxy_radius)
        return ellipse_a + (distance_to_center - bulge_radius) / (galaxy_radius - bulge_radius) * (ellipse_b - ellipse_a);

    if (distance_to_center < galaxy_radius * far_field_factor)
        return ellipse_b + (distance_to_center - galaxy_radius) / (galaxy_radius * far_field_factor - galaxy_radius) * (1.0f - ellipse_b);

    return 1.0f;
}

float calculate_orbital_velocity(float distance_to_center)
{
    if (distance_to_center <= 0.0f)
        return 0.0f;

    if (use_constant_velocity == 1)
        return velocity_factor;

    const float velocity = 0.1f * sqrt(160.0f * distance_to_center);
    return velocity * velocity_factor / distance_to_center;
}

float2 calculate_star_position(const Star star)
{
    const float a = star.distance_to_center;
    const float b = star.distance_to_center * calculate_eccentricity(star.distance_to_center);
    const float tilt_angle = star.distance_to_center * ellipse_tilt;

    const float t = star.angular_position + calculate_orbital_velocity(star.distance_to_center) * time;
    const float2 f1 = a * float2(cos(tilt_angle), sin(tilt_angle));
    const float2 f2 = b * float2(-sin(tilt_angle), cos(tilt_angle));
    return f1 * cos(t) + f2 * sin(t);
}

v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
{
    const Star star = star_buffer[instance_id];
    const float2 star_position = calculate_star_position(star);

    const float4 position = float4(star_position.x, 0.0f, star_position.y, 1.0f) + position_offset;
    const float4 screen_pos = UnityObjectToClipPos(position);

    v2f o;
    o.position = float4(BILLBOARD[vertex_id] * star_size * 0.001f, 0.0f) + screen_pos;
    o.uv = BILLBOARD_UVS[vertex_id];
    o.color = star.color;
    return o;
}
