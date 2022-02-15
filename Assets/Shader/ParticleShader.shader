Shader "ParticleShader"
{
    Properties {}

    SubShader
    {
        Pass
        {
            Blend SrcAlpha one

            CGPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Particle
            {
                float3 position;
                float3 velocity;
            };

            struct PixelShaderInput
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
            };

            StructuredBuffer<Particle> particleBuffer;

            PixelShaderInput vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
            {
                PixelShaderInput i;
                i.position = UnityObjectToClipPos(float4(particleBuffer[instance_id].position, 1.0f));
                i.color = float4(abs(particleBuffer[instance_id].velocity) * 5, 1.0f);

                return i;
            }

            float4 frag(PixelShaderInput i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }
    }

    Fallback Off
}