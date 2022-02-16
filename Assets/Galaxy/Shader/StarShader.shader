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
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "StarVertexShader.cginc"
            #include "StarFragmentShader.cginc"
            ENDCG
        }
    }

    Fallback Off
}