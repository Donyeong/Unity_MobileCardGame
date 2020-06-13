Shader "Shadow/PostProcessShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Dist("Dist", Range(0,1)) = 0
        _Inf("Inf", Range(0,50)) = 0
        _Size("Size", Range(0,0.1)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Dist;
            float _Inf;
            float _Size;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 cuv = i.uv;
                
                float dis = distance(cuv, float2(0.5, 0.5));

                if (dis > _Dist - _Size && dis  < _Dist + _Size)
                {
                    cuv -= 0.5 ;
                    float nor = (_Size - abs(_Dist - dis)) * _Inf;

                    nor = clamp(nor,0, 0.05);

                    float2 n = normalize(i.uv - float2(0.5, 0.5));
                    
                    
                    cuv = cuv + float2(-n.y, n.x)*nor;

                    

                    cuv += 0.5;
                }

                fixed4 col = tex2D(_MainTex, cuv);
                
                
                return col;
            }
            ENDCG
        }
    }
}
