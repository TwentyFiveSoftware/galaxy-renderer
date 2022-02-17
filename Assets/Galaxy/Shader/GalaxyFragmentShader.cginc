sampler2D _MainTex;

float4 frag(v2f i) : SV_Target
{
    return tex2D(_MainTex, i.uv) * i.color;
}
