Shader "Custom/DepthOnly"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite On     // Escribe en el buffer de profundidad
            ColorMask 0   // No renderiza color (oculto)
        }
    }
}
