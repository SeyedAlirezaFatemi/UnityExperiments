Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID" , Range(0, 255)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            // For the color of this pixel, take 0% of the color output by the shader and 100% of the color already rendered at the pixel
            Blend Zero One
            // Don't write into depth buffer
            ZWrite Off
            Stencil
            {
                Ref [_StencilID]
                // The stencil test always passes
                Comp Always
                // If both the stencil and depth tests pass, replace the stencil value attached to the pixel with _StencilID
                Pass Replace
                Fail Keep
            }
        }
    }
}