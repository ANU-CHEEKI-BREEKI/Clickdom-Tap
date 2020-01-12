Shader "Custom/SpriteSheetShaderWithDisolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

		_CracksMaskMaskTex ("CracksMaskMain", 2D) = "white" {}
		_CracksMaskAdditionalTex ("CracksMaskAdditional", 2D) = "white" {}
		_CracksTex ("CracksTexture", 2D) = "white" {}
		_CracksColor ("CracksColor", Color) = (0,0,0,1)

		_CutoffGrayshade("CutoffGrayshade", Range(0, 1)) = 0.85

		_BorderCracksAlphaCutoff("BorderCracksAlphaCutoff", Range(0, 1)) = 0.5
		_BorderCracksDistance("BorderCracksDistance", Range(0, 0.01)) = 0.0035
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off
		Cull off 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID 
				float2 uv: TEXCOORD;
			};

			struct v2f
			{
				float2 uv: TEXCOORD;
				float2 meshUv: TEXCOORD;
				float4 vertex: SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID 
			};

			sampler2D _MainTex;

			sampler2D _CracksMaskMaskTex;
			float4 _CracksMaskMaskTex_ST;

			sampler2D _CracksMaskAdditionalTex;
			float4 _CracksMaskAdditionalTex_ST;

			sampler2D _CracksTex;
			float4 _CracksTex_ST;

			float4 _CracksColor;

			float _CutoffGrayshade;

			float _BorderCracksAlphaCutoff;
			float _BorderCracksDistance;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(fixed4,  _Color)
				UNITY_DEFINE_INSTANCED_PROP(fixed4,  _MainTex_UV)
				UNITY_DEFINE_INSTANCED_PROP(float,  _CracksDisolve)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = (v.uv * UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).xy) + UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).zw;

				o.meshUv = v.uv;

				return o;
			}

			float Remap(float value, float low1, float high1, float low2, float high2)
			{
				return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
			}

			float Grayshade(float4 color)
			{
				return (color.r + color.g + color.b) / 3;
			}

			bool HasNearTransparentPixel(float2 texUv)
			{
				float dist = _BorderCracksDistance;
				return 
					tex2D(_MainTex, float2(texUv.x + dist, texUv.y)).a < _BorderCracksAlphaCutoff ||
					tex2D(_MainTex, float2(texUv.x, texUv.y + dist)).a < _BorderCracksAlphaCutoff ||
					tex2D(_MainTex, float2(texUv.x - dist, texUv.y)).a < _BorderCracksAlphaCutoff ||
					tex2D(_MainTex, float2(texUv.x, texUv.y - dist)).a < _BorderCracksAlphaCutoff;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				//calc cracks mask pixel color
				float2 cracksUv = TRANSFORM_TEX(i.meshUv, _CracksMaskMaskTex);
				fixed4 crackMaskColor = tex2D(_CracksMaskMaskTex, cracksUv);

				float2 addCracksUv = TRANSFORM_TEX(i.meshUv, _CracksMaskAdditionalTex);
				fixed4 additionalCrackMaskColor = tex2D(_CracksMaskAdditionalTex, addCracksUv);

				fixed4 cracksMaskColor = crackMaskColor * additionalCrackMaskColor;
				
				fixed4 crackTexColor = 0;

				if(Grayshade(cracksMaskColor) > _CutoffGrayshade)
				{
					//cut off white peaces
					cracksMaskColor = 1;
				}				
				else
				{
					//calc cracks amount
					float disolve = UNITY_ACCESS_INSTANCED_PROP(Props, _CracksDisolve);
					disolve = Remap(disolve, 0, 1, -0.65, 0.65);
					cracksMaskColor += disolve;
					cracksMaskColor = float4(
						Remap(cracksMaskColor.r, 0, 1, -4, 4),
						Remap(cracksMaskColor.g, 0, 1, -4, 4),
						Remap(cracksMaskColor.b, 0, 1, -4, 4),
						1
					);

					//cracksMaskColor = smoothstep(0, 0.9, cracksMaskColor);
					cracksMaskColor = clamp(cracksMaskColor, 0, 1);

					if(Grayshade(cracksMaskColor) <= 0.001)
					{
						//if this pixel in chack
						//then we need to apply cracl tectute to it
						float2 crackTexUv = TRANSFORM_TEX(i.meshUv, _CracksTex);
						crackTexColor = tex2D(_CracksTex, crackTexUv);
						//tint it
						crackTexColor *= _CracksColor;

						//if this pixel in chack and it near original sprite alpha border
						//then we need return zero alpha
						if(HasNearTransparentPixel(i.uv))
							return 0;
					}
				}
				cracksMaskColor.a = 1;

				//calc real pixel color
				float2 uv = i.uv;
				fixed4 pixColor = tex2D(_MainTex, uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				
				//calc cracked pixel color
				return pixColor * cracksMaskColor + crackTexColor * pixColor.a;
			}

			ENDCG
		}
    }
}
