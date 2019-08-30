#ifndef MYRP_LIT_META_INCLUDED
#define MYRP_LIT_META_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Lighting.hlsl"

CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4 unity_LightmapST, unity_DynamicLightmapST;
CBUFFER_END

CBUFFER_START(UnityPerMaterial)
	float4 _MainTex_ST;
	float4 _Color, _EmissionColor;
	float _Metallic;
	float _Smoothness;
CBUFFER_END

CBUFFER_START(UnityMetaPass)
	float unity_OneOverOutputBoost;
	float unity_MaxOutputValue;
	bool4 unity_MetaFragmentControl, unity_MetaVertexControl;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct VertexInput {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
	float2 lightmapUV : TEXCOORD1;
	float2 dynamicLightmapUV : TEXCOORD2;
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	float2 uv : TEXCOORD0;
};

VertexOutput MetaPassVertex (VertexInput input) {
	VertexOutput output;
	if (unity_MetaVertexControl.x) {
		input.pos.xy =
			input.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
	}
	if (unity_MetaVertexControl.y) {
		input.pos.xy =
			input.dynamicLightmapUV * unity_DynamicLightmapST.xy +
			unity_DynamicLightmapST.zw;
	}
	input.pos.z = input.pos.z > 0 ? FLT_MIN : 0.0;
	output.clipPos = mul(unity_MatrixVP, float4(input.pos.xyz, 1.0));
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);
	return output;
}

float4 MetaPassFragment (VertexOutput input) : SV_TARGET {
	float4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
	albedoAlpha *= _Color;
	albedoAlpha.rgb *= albedoAlpha.a;
	LitSurface surface = GetLitSurfaceMeta(
		albedoAlpha.rgb, _Metallic, _Smoothness
	);
	
	float4 meta = 0;
	if (unity_MetaFragmentControl.x) {
		meta = float4(surface.diffuse, 1);
		meta.rgb += surface.specular * surface.roughness * 0.5;
		meta.rgb = clamp(
			PositivePow(meta.rgb, unity_OneOverOutputBoost),
			0,unity_MaxOutputValue
		);
	}
	if (unity_MetaFragmentControl.y) {
		meta = float4(_EmissionColor.rgb * albedoAlpha.a, 1);
	}
	return meta;
}

#endif // MYRP_LIT_META_INCLUDED