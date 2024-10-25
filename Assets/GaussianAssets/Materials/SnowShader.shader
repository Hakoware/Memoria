Shader "Custom/SnowflakeShader"
{
    Properties
    {
        _MainTex ("Snowflake Texture", 2D) = "white" {}
        _Color ("Snowflake Color", Color) = (1, 1, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 5.0)) = 2.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        float _FresnelPower;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir; // Direction from camera
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Apply Fresnel Effect
            float fresnel = pow(1.0 - dot(normalize(IN.viewDir), float3(0, 0, 1)), _FresnelPower);
            c.rgb += fresnel * 0.5; // Add a light blue-ish tint for a snowy feel

            o.Albedo = c.rgb;
            o.Alpha = c.a * fresnel; // Control transparency using fresnel effect for a softer edge
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
