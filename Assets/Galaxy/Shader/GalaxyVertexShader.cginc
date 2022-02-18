﻿#include "UnityCG.cginc"
#include "GalaxyParticle.cginc"

struct v2f
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

uniform float time;
uniform float4 position_offset;
StructuredBuffer<GalaxyParticle> galaxy_buffer;

uniform float bulge_radius;
uniform float galaxy_radius;
uniform float far_field_factor;
uniform float ellipse_a;
uniform float ellipse_b;
uniform float ellipse_tilt;
uniform float particle_size_factor;
uniform float star_size_factor;
uniform float dust_size_factor;
uniform float dust_filament_size_factor;
uniform float dust_transparency;
uniform float dust_filament_transparency;
uniform float dust_transparency_offset;
uniform float dust_bulge_transparency;
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

float calculate_orbital_velocity(const float distance_to_center)
{
    if (distance_to_center <= 0.0f)
        return 0.0f;

    if (use_constant_velocity == 1)
        return velocity_factor;

    const float velocity = 0.1f * sqrt(160.0f * distance_to_center);
    return velocity * velocity_factor / distance_to_center;
}

float2 calculate_star_position(const GalaxyParticle particle)
{
    const float a = particle.distance_to_center;
    const float b = particle.distance_to_center * calculate_eccentricity(particle.distance_to_center);
    const float tilt_angle = particle.distance_to_center * ellipse_tilt;

    const float t = particle.angular_position + calculate_orbital_velocity(particle.distance_to_center) * time;
    const float2 f1 = a * float2(cos(tilt_angle), sin(tilt_angle));
    const float2 f2 = b * float2(-sin(tilt_angle), cos(tilt_angle));
    return f1 * cos(t) + f2 * sin(t);
}

v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
{
    const GalaxyParticle particle = galaxy_buffer[instance_id];
    const float2 particle_position = calculate_star_position(particle);

    const float size = particle.size * particle_size_factor *
        (particle.type == 0 ? star_size_factor : particle.type == 1 ? dust_size_factor : particle.type == 2 ? dust_filament_size_factor : 1.0f);

    const float4 position = float4(particle_position.x, 0.0f, particle_position.y, 1.0f) + position_offset;
    const float4 screen_position = UnityObjectToClipPos(position);
    const float4 screen_position_offset = float4(BILLBOARD[vertex_id] * (0.003f * size), 0.0f);


    float alpha = 1.0f;

    if (particle.type == 1 || particle.type == 2)
    {
        if (particle.distance_to_center < bulge_radius)
            alpha = dust_bulge_transparency;
        else
            alpha = (particle.type == 1 ? dust_transparency : dust_filament_transparency) *
                sin(UNITY_PI / (galaxy_radius * 0.9f - bulge_radius) * (particle.distance_to_center - bulge_radius + dust_transparency_offset));

        alpha = max(alpha, 0.0f);
    }

    v2f o;
    o.position = screen_position_offset + screen_position;
    o.uv = BILLBOARD_UVS[vertex_id];
    o.color = float4((particle.color * particle.size).xyz, alpha);
    return o;
}