Shader "Custom/Ghosting"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            Name "Ghosting"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _BlendAmount;
            TEXTURE2D_X(_HistoryTex);
            SAMPLER(sampler_HistoryTex);

            half4 Frag (Varyings input) : SV_Target
            {
                // 1. Sample current frame
                half4 currentFrame = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);

                // 2. Multi-tap Box Blur for the History Frame - higher = blurrier.
                float blurSpread = 0.0001;

                // We sample 5 points in a cross shape (+) to create the blur
                half4 historyBlur = SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, input.texcoord); // Center
                historyBlur += SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, input.texcoord + float2(blurSpread, blurSpread));   // Top Right
                historyBlur += SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, input.texcoord + float2(-blurSpread, blurSpread));  // Top Left
                historyBlur += SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, input.texcoord + float2(blurSpread, -blurSpread));  // Bottom Right
                historyBlur += SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, input.texcoord + float2(-blurSpread, -blurSpread)); // Bottom Left
                
                // Average the 5 samples
                historyBlur /= 5.0;

                // 3. Blend the current frame with the blurred history
                return lerp(currentFrame, historyBlur, _BlendAmount);
            }
            ENDHLSL
        }
    }
}
