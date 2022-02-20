static const float3 BILLBOARD[] = {
    float3(-1, -1, 0),
    float3(1, -1, 0),
    float3(-1, 1, 0),
    float3(-1, 1, 0),
    float3(1, -1, 0),
    float3(1, 1, 0),
};

static const float2 BILLBOARD_UVS[] = {
    float2(0, 0),
    float2(1, 0),
    float2(0, 1),
    float2(0, 1),
    float2(1, 0),
    float2(1, 1),
};

struct GalaxyParticle
{
    float angular_position;
    float distance_to_center;
    float size;
    float yOffset;
    float4 color;
    int type;
};
