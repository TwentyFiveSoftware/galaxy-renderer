#include "UnityCG.cginc"
#include "TextureConsts.cginc"

struct Star
{
    float angular_position;
    float angular_velocity;
    float distanceToCenter;
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


float calculateEccentricity(float distanceToCenter)
{
    if (distanceToCenter < bulge_radius)
        return 1.0f + (distanceToCenter / bulge_radius) * (ellipse_a - 1.0f);

    if (distanceToCenter < galaxy_radius)
        return ellipse_a + (distanceToCenter - bulge_radius) / (galaxy_radius - bulge_radius) * (ellipse_b - ellipse_a);

    if (distanceToCenter < galaxy_radius * far_field_factor)
        return ellipse_b + (distanceToCenter - galaxy_radius) / (galaxy_radius * far_field_factor - galaxy_radius) * (1.0f - ellipse_b);

    return 1.0f;
}

float2 calculateStarPosition(Star star)
{
    const float a = star.distanceToCenter;
    const float b = star.distanceToCenter * calculateEccentricity(star.distanceToCenter);
    const float tilt_angle = star.distanceToCenter * ellipse_tilt;

    const float t = star.angular_position + star.angular_velocity * time;
    const float2 f1 = a * float2(cos(tilt_angle), sin(tilt_angle));
    const float2 f2 = b * float2(-sin(tilt_angle), cos(tilt_angle));
    return f1 * cos(t) + f2 * sin(t);
}

v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
{
    const Star star = star_buffer[instance_id];
    const float2 starPosition = calculateStarPosition(star);
    
    const float4 position = float4(starPosition.x, 0.0f, starPosition.y, 1.0f) + position_offset;
    const float4 screenPos = UnityObjectToClipPos(position);

    v2f o;
    o.position = float4(BILLBOARD[vertex_id] * star_size * 0.001f, 0.0f) + screenPos;
    o.uv = BILLBOARD_UVS[vertex_id];
    o.color = float4(1.0f, 1.0f, 1.0f, 0.5f);
    return o;
}
