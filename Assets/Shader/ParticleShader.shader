Shader "ParticleShader"
{
    Properties
    {
        _ParticleSize ("Particle Size", Float) = 1
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Particle
            {
                float3 position;
                float3 velocity;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            StructuredBuffer<Particle> particleBuffer;

            uniform float _ParticleSize;
            sampler2D _MainTex;

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

            v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
            {
                const float4 screenPos = UnityObjectToClipPos(float4(particleBuffer[instance_id].position, 1.0f));

                v2f o;
                o.position = float4(BILLBOARD[vertex_id] * _ParticleSize * 0.001, 0) + screenPos;
                o.uv = BILLBOARD_UVS[vertex_id];
                o.color = float4(1.0f, 0.45f, 0.0f, 0.1f);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }

    Fallback Off
}