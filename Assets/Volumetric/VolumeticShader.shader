Shader "VolumetricShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            uniform float3 boxBoundsMin;
            uniform float3 boxBoundsMax;


            float2 calculateRayDistanceToBox(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 rayDirection)
            {
                const float3 t0 = (boundsMin - rayOrigin) / rayDirection;
                const float3 t1 = (boundsMax - rayOrigin) / rayDirection;
                const float3 tmin = min(t0, t1);
                const float3 tmax = max(t0, t1);

                const float distanceToNearIntersection = max(max(tmin.x, tmin.y), tmin.z);
                const float distanceToFarIntersection = min(tmax.x, min(tmax.y, tmax.z));

                const float distanceToBox = max(0, distanceToNearIntersection);
                const float distanceInsideBox = max(0, distanceToFarIntersection - distanceToNearIntersection);
                return float2(distanceToBox, distanceInsideBox);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.viewVector = mul(unity_CameraToWorld, float4(mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1)).xyz, 0));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDirection = normalize(i.viewVector);

                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)) * length(i.viewVector);

                const float2 rayBoxInfo = calculateRayDistanceToBox(boxBoundsMin, boxBoundsMax, rayOrigin, rayDirection);
                const float distanceToBox = rayBoxInfo.x;
                const float distanceInsideBox = rayBoxInfo.y;

                const bool rayHitsBox = distanceInsideBox > 0 && distanceToBox < depth;
                if (rayHitsBox)
                {
                    color = 0.0f;
                }

                return color;
            }
            ENDCG
        }
    }
}