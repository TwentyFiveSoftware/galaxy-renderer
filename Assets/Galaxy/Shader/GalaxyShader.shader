Shader "GalaxyShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "GalaxyVertexShader.cginc"
            #include "GalaxyFragmentShader.cginc"
            ENDCG
        }
    }

    Fallback Off
}