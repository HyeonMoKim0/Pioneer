Shader "Custom/GrayOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrayColor ("Gray Color", Color) = (0.5, 0.5, 0.5, 0.85) // ȸ��, ���� 0.85
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "GrayOverlayPass"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // ���� ���� ����

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _GrayColor;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // ���� �ؽ�ó ���ø�
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // ȸ�� �������̿� ����
                return lerp(col, _GrayColor, _GrayColor.a);
            }
            ENDHLSL
        }
    }
}