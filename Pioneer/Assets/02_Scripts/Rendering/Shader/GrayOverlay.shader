Shader "Custom/GrayOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // ȸ��
        _GrayColor ("Gray Color", Color) = (0.5, 0.5, 0.5, 0.15) // ȸ��, ���� 0.15
        
        // ������ ���� ����
        _GlassColor ("Glass Tint Color", Color) = (0.8, 0.9, 1.0, 0.1) // ���� ������ Ǫ���� ƾƮ, ���� 0.1
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
            float4 _GlassColor;

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
                //half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // ȸ�� �������̿� ����
                //return lerp(col, _GrayColor, _GrayColor.a);

                // === ������ ���� ===
                // �ؽ�ó ���ø� (������: ���� ƾƮ�� ����� ��� ���� ����)
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // ���� ���� ƾƮ�� ����, ���Ĵ� ���� ����
                half4 finalColor = _GrayColor;
                finalColor.a = _GrayColor.a; // ���İ��� �ſ� ���� ���� (0.0~0.1)
                return finalColor;
            }
            ENDHLSL
        }
    }
}