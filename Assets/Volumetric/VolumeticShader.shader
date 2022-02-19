Shader "VolumetricShader"
{
    Properties {}

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

            sampler3D volumetricTexture;

            uniform float3 boxBoundsMin;
            uniform float3 boxBoundsMax;

            struct Point
            {
                float3 position;
                float radius;
                float4 color;
            };

            uniform StructuredBuffer<Point> pointBuffer;


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

            float4 blendColors(float4 dst, float4 src)
            {
                return dst * (1.0f - src.a) + src * src.a;
            }

            float4 sampleColorAt(const float3 position)
            {
                const float3 normalizedPos = 1 - (boxBoundsMax - position) / (boxBoundsMax - boxBoundsMin);

                float4 color = float4(0, 0, 0, 0);

                const uint points = 100;
                for (uint i = 0; i < points; i++)
                {
                    const Point p = pointBuffer[i];
                    const float3 vectorToPoint = p.position - normalizedPos;

                    if (length(vectorToPoint) < p.radius)
                    {
                        const float4 pointColor = float4(p.color.rgb, 1.0f - length(vectorToPoint) / p.radius);
                        color = blendColors(color, pointColor);
                    }
                }

                return color;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDirection = normalize(i.viewVector);

                const float2 rayBoxInfo = calculateRayDistanceToBox(boxBoundsMin, boxBoundsMax, rayOrigin, rayDirection);
                const float distanceToBox = rayBoxInfo.x;
                const float distanceInsideBox = rayBoxInfo.y;

                const int steps = 100;
                const float epsilon = 1.0f / steps;
                const float stepSize = (distanceInsideBox - epsilon) / steps;

                float4 color = float4(0, 0, 0, 1);
                for (int step = 0; step < steps; step++)
                {
                    const float3 rayPosition = rayOrigin + rayDirection * (distanceToBox + stepSize * (steps - step));
                    const float4 colorAtPosition = sampleColorAt(rayPosition);
                    color = blendColors(color, colorAtPosition);
                }

                return color;
            }
            ENDCG
        }
    }
}