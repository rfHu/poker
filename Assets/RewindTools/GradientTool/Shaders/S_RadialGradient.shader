// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:33535,y:32812,varname:node_3138,prsc:2|emission-340-OUT,alpha-3685-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32233,y:32407,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Vector4Property,id:1221,x:31522,y:33059,ptovrint:False,ptlb:Anchor,ptin:_Anchor,varname:node_1221,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2,v2:0.2,v3:3,v4:0;n:type:ShaderForge.SFN_Code,id:1509,x:32017,y:33084,varname:node_1509,prsc:2,code:dQB2ACAALQA9ACAAZgBsAG8AYQB0ADIAKAAwAC4ANQAsADAALgA1ACkAOwAKAHUAdgAuAHgAIAAqAD0AIABhAHMAcABlAGMAdAA7AAoAUwBjAGEAbABlACAAPQAgAGEAYgBzACgAUwBjAGEAbABlACkAOwAKAGYAbABvAGEAdAAgAGYAaQBlAGwAZAAgAD0AIAAoAGQAaQBzAHQAYQBuAGMAZQAoAHUAdgAsACAAQQBuAGMAaABvAHIAKQAgACoAIAAyACkAIAAqACAAKAAxAC8AUwBjAGEAbABlACkAOwAKAGYAaQBlAGwAZAAgAD0AIABjAGwAYQBtAHAAKAAwACwAMQAsAGYAaQBlAGwAZAApADsACgByAGUAdAB1AHIAbgAgADEALQBwAG8AdwAoADEALQBmAGkAZQBsAGQALAAyACkAOwA=,output:0,fname:RadialGrad,width:490,height:264,input:1,input:1,input:0,input:0,input_1_label:Anchor,input_2_label:uv,input_3_label:aspect,input_4_label:Scale|A-8728-OUT,B-118-UVOUT,C-1221-W,D-1221-Z;n:type:ShaderForge.SFN_Color,id:8733,x:32136,y:32661,ptovrint:False,ptlb:Inner,ptin:_Inner,varname:node_8733,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4705882,c2:0.02768166,c3:0.4583702,c4:1;n:type:ShaderForge.SFN_Color,id:1410,x:32136,y:32825,ptovrint:False,ptlb:Outer,ptin:_Outer,varname:node_1410,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Append,id:8728,x:31684,y:33078,varname:node_8728,prsc:2|A-1221-X,B-1221-Y;n:type:ShaderForge.SFN_TexCoord,id:118,x:31684,y:33223,varname:node_118,prsc:2,uv:0;n:type:ShaderForge.SFN_RgbToHsv,id:2658,x:32460,y:32664,varname:node_2658,prsc:2|IN-8733-RGB;n:type:ShaderForge.SFN_RgbToHsv,id:8678,x:32460,y:32827,varname:node_8678,prsc:2|IN-1410-RGB;n:type:ShaderForge.SFN_HsvToRgb,id:7438,x:33060,y:32779,varname:node_7438,prsc:2|H-7404-OUT,S-9341-OUT,V-8412-OUT;n:type:ShaderForge.SFN_Lerp,id:7404,x:32827,y:32655,varname:node_7404,prsc:2|A-2658-HOUT,B-8678-HOUT,T-1509-OUT;n:type:ShaderForge.SFN_Lerp,id:9341,x:32827,y:32779,varname:node_9341,prsc:2|A-2658-SOUT,B-8678-SOUT,T-1509-OUT;n:type:ShaderForge.SFN_Lerp,id:8412,x:32827,y:32901,varname:node_8412,prsc:2|A-2658-VOUT,B-8678-VOUT,T-1509-OUT;n:type:ShaderForge.SFN_Lerp,id:5454,x:32827,y:33023,varname:node_5454,prsc:2|A-8733-RGB,B-1410-RGB,T-1509-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:340,x:33289,y:32911,ptovrint:False,ptlb:UseHSV,ptin:_UseHSV,varname:node_340,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-5454-OUT,B-7438-OUT;n:type:ShaderForge.SFN_Lerp,id:3685,x:32827,y:33152,varname:node_3685,prsc:2|A-8733-A,B-1410-A,T-1509-OUT;proporder:7241-1221-8733-1410-340;pass:END;sub:END;*/

Shader "GradientTool/S_RadialGradient" {
    Properties {
        _Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _Anchor ("Anchor", Vector) = (0.2,0.2,3,0)
        _Inner ("Inner", Color) = (0.4705882,0.02768166,0.4583702,1)
        _Outer ("Outer", Color) = (1,1,1,1)
        [MaterialToggle] _UseHSV ("UseHSV", Float ) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _Anchor;
            float RadialGrad( float2 Anchor , float2 uv , float aspect , float Scale ){
            uv -= float2(0.5,0.5);
            uv.x *= aspect;
            Scale = abs(Scale);
            float field = (distance(uv, Anchor) * 2) * (1/Scale);
            field = clamp(0,1,field);
            return 1-pow(1-field,2);
            }
            
            uniform float4 _Inner;
            uniform float4 _Outer;
            uniform fixed _UseHSV;
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
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float node_1509 = RadialGrad( float2(_Anchor.r,_Anchor.g) , i.uv0 , _Anchor.a , _Anchor.b );
                float4 node_2658_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_2658_p = lerp(float4(float4(_Inner.rgb,0.0).zy, node_2658_k.wz), float4(float4(_Inner.rgb,0.0).yz, node_2658_k.xy), step(float4(_Inner.rgb,0.0).z, float4(_Inner.rgb,0.0).y));
                float4 node_2658_q = lerp(float4(node_2658_p.xyw, float4(_Inner.rgb,0.0).x), float4(float4(_Inner.rgb,0.0).x, node_2658_p.yzx), step(node_2658_p.x, float4(_Inner.rgb,0.0).x));
                float node_2658_d = node_2658_q.x - min(node_2658_q.w, node_2658_q.y);
                float node_2658_e = 1.0e-10;
                float3 node_2658 = float3(abs(node_2658_q.z + (node_2658_q.w - node_2658_q.y) / (6.0 * node_2658_d + node_2658_e)), node_2658_d / (node_2658_q.x + node_2658_e), node_2658_q.x);;
                float4 node_8678_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_8678_p = lerp(float4(float4(_Outer.rgb,0.0).zy, node_8678_k.wz), float4(float4(_Outer.rgb,0.0).yz, node_8678_k.xy), step(float4(_Outer.rgb,0.0).z, float4(_Outer.rgb,0.0).y));
                float4 node_8678_q = lerp(float4(node_8678_p.xyw, float4(_Outer.rgb,0.0).x), float4(float4(_Outer.rgb,0.0).x, node_8678_p.yzx), step(node_8678_p.x, float4(_Outer.rgb,0.0).x));
                float node_8678_d = node_8678_q.x - min(node_8678_q.w, node_8678_q.y);
                float node_8678_e = 1.0e-10;
                float3 node_8678 = float3(abs(node_8678_q.z + (node_8678_q.w - node_8678_q.y) / (6.0 * node_8678_d + node_8678_e)), node_8678_d / (node_8678_q.x + node_8678_e), node_8678_q.x);;
                float3 emissive = lerp( lerp(_Inner.rgb,_Outer.rgb,node_1509), (lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(lerp(node_2658.r,node_8678.r,node_1509)+float3(0.0,-1.0/3.0,1.0/3.0)))-1),lerp(node_2658.g,node_8678.g,node_1509))*lerp(node_2658.b,node_8678.b,node_1509)), _UseHSV );
                float3 finalColor = emissive;
                return fixed4(finalColor,lerp(_Inner.a,_Outer.a,node_1509));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
