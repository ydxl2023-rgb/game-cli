Shader "Miniwow/RecoveredScene"
{
    Properties
    {
        _MainTex ("Source base texture", 2D) = "white" {}
        _Color ("Source tint", Color) = (1,1,1,1)
        _Cutoff ("Source alpha cutoff", Range(0,1)) = 0.5
        _AlphaClip ("Enable alpha cutoff", Float) = 0
        _SrcBlend ("Source blend", Float) = 1
        _DstBlend ("Destination blend", Float) = 0
        _ZWrite ("Depth writing", Float) = 1
        _Cull ("Source culling", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;
            float _AlphaClip;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 position : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            Output Vert(Input value)
            {
                Output result;
                result.position = UnityObjectToClipPos(value.vertex);
                result.uv = TRANSFORM_TEX(value.uv, _MainTex);
                result.color = value.color;
                return result;
            }
            fixed4 Frag(Output value) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, value.uv) * _Color * value.color;
                if (_AlphaClip > 0.5) clip(color.a - _Cutoff);
                return color;
            }
            ENDCG
        }
    }
}
