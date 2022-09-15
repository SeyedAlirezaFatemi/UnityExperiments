Shader "Portal/PortalShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL
        
        Cull Off
        
        Pass
        {
            Name "Mask"
            // https://docs.unity3d.com/Manual/SL-Stencil.html
            // The portal surface and the portal outline meshes intersect with each other.
            // We use stencil to cut out the portal surface from the outline. (If there is an outline)
            Stencil
            {
                Ref 1
                Pass replace
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            // The vertex-to-fragment struct
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos: TEXCOORD0;
            };

            // Vertex shader
            v2f vert(appdata v)
            {
                v2f o;
                // Object-space to clip-space
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            uniform sampler2D _MainTex;

            float4 frag(v2f i): SV_Target
            {
                // Perspective divide to find UV coordinates in screen-space
                const float2 uv = i.screenPos.xy / i.screenPos.w;
                // The _MainTex is a shot of what the camera sees.
                // We only take the parts of that shot where the screen is visible.
                /*
                 *  ----------------------
                 *  |                    |
                 *  |      ---------     |
                 *  |     |        |     |
                 *  |     | Portal |     |
                 *  |     |        |     |
                 *  |     ----------     |
                 *  |                    |
                 *  ----------------------
                 */
                float4 color = tex2D(_MainTex, uv);
                return color;
            }
            ENDHLSL
        }
    }
}