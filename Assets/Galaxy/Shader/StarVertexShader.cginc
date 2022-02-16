#include "UnityCG.cginc"
#include "TextureConsts.cginc"

struct Star
{
    float angular_position;
    float angular_velocity;
    float ellipse_tilt_angle;
    float ellipse_a;
    float ellipse_b;
    float size;
    float4 color;
};

struct v2f
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

uniform float time;
uniform float4 positionOffset;
StructuredBuffer<Star> starBuffer;

// https://en.wikipedia.org/wiki/Ellipse
float2 calculateStarPosition(Star star)
{
    const float t = star.angular_position + star.angular_velocity * time;
    const float2 f1 = star.ellipse_a * float2(cos(star.ellipse_tilt_angle), sin(star.ellipse_tilt_angle));
    const float2 f2 = star.ellipse_b * float2(-sin(star.ellipse_tilt_angle), cos(star.ellipse_tilt_angle));
    return f1 * cos(t) + f2 * sin(t);
}

v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
{
    const Star star = starBuffer[instance_id];
    const float2 starPosition = calculateStarPosition(star);
    const float4 position = float4(starPosition.x, 0.0f, starPosition.y, 1.0f) + positionOffset;
    const float4 screenPos = UnityObjectToClipPos(position);

    v2f o;
    o.position = float4(BILLBOARD[vertex_id] * star.size * 0.001, 0) + screenPos;
    o.uv = BILLBOARD_UVS[vertex_id];
    o.color = star.color;
    return o;
}
