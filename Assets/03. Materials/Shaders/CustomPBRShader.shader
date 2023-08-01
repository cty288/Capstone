/*
* References:
* https://github.com/Unity-Technologies/Graphics/blob/v10.5.0/com.unity.render-pipelines.universal/Shaders/Lit.shader
* https://www.cyanilux.com/tutorials/urp-shader-code/
*/


// Shader Implementation uses URP v.12+ libraries and deferred shading which is only available on Unity 2021.2+
Shader "URP/Custom/CustomPBRShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
        LOD 300
        
        // DO NOT USE UsePass, only use Pass
        // DO NOT USE multi-pass
        
        //Forward Pass
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //TODO: If we want to support older versions of OpenGL make another subshader that excludes all other renderers
            /*
             #pragma only_renderers gles gles3 glcore d3d11
             #pragma target 2.0
            */
            // What is Deferred Shading: https://docs.unity3d.com/Manual/RenderTech-DeferredShading.html

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            
            ENDHLSL
        }
        
        //Shadow Pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            ENDHLSL
        }
        
        //REMOVE IF YOU MAKE A SUBSHADER FOR AN OLDER VERSION OF OPENGL
        //Deferred Lighting Pass (Enable in URP Asset)
        Pass
        {
            Name "GBuffer"
            Tags{"LightMode" = "UniversalGBuffer"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            ENDHLSL
        }
        
        //Depth Buffer
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            ENDHLSL
        }
        
        //Depth and Normals Buffer
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            ENDHLSL
        }
        
        //Lightmap Baking
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            ENDHLSL
        }
        
        //2D Lighting
        Pass
        {
            Name "Universal2D"
            Tags{"LightMode" = "Universal2D"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            ENDHLSL
        }
    }
}
