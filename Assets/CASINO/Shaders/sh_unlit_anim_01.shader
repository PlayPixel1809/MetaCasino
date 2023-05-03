Shader "Casino/unlit_anim_01" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _numU ("numU", Float ) = 4
        _numV ("numV", Float ) = 8
        _Speed ("Speed", Float ) = 0.3
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _numU;
            uniform float _numV;
            float Function_node_1253( float numCol , float Time , float numRow ){
            float oneCol = 1/numCol;
            float allSteps = 1/(numCol*numRow);
            int curentCol = floor(Time/allSteps);
            
            
            if(curentCol >= numCol)
            {
            curentCol = curentCol - ((floor(curentCol / numCol)) * numCol);
            }
            
            return (oneCol * curentCol);
            }
            
            float Function_node_4725( float numRow , float Time ){
            float oneRow = 1/numRow;
            int curentRow = (Time / oneRow);
            return (1 - (oneRow*curentRow)) - numRow;
            }
            
            uniform float _Speed;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
/////// Vectors:
////// Lighting:
////// Emissive:
                float node_3376 = 1.0;
                float4 node_3380 = _Time + _TimeEditor;
                float node_191 = frac((node_3380.g*_Speed));
                float2 node_9926 = ((float2((node_3376/_numU),(node_3376/_numV))*i.uv0)+float2(Function_node_1253( _numU , node_191 , _numV ),Function_node_4725( _numV , node_191 )));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_9926, _MainTex));
                float3 emissive = _MainTex_var.rgb;
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
