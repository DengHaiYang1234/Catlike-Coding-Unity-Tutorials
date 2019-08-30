#ifndef MYRP_POST_EFFECT_STACK_INCLUDED
#define MYRP_POST_EFFECT_STACK_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

float4 _ProjectionParams;
float4 _ZBufferParams;

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_DepthTex);
SAMPLER(sampler_DepthTex);

struct VertexInput {
	float4 pos : POSITION;
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	float2 uv : TEXCOORD0;
};

VertexOutput DefaultPassVertex (VertexInput input) {
	VertexOutput output;
	output.clipPos = float4(input.pos.xy, 0.0, 1.0);
	output.uv = input.pos.xy * 0.5 + 0.5;
	if (_ProjectionParams.x < 0.0) {
		output.uv.y = 1.0 - output.uv.y;
	}
	return output;
}

float4 CopyPassFragment (VertexOutput input) : SV_TARGET {
	return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
}

float4 BlurSample (float2 uv, float uOffset = 0.0, float vOffset = 0.0) {
	uv += float2(uOffset * ddx(uv.x), vOffset * ddy(uv.y));
	return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
}

float4 BlurPassFragment (VertexOutput input) : SV_TARGET {
	float4 color =
		BlurSample(input.uv, 0.5, 0.5) +
		BlurSample(input.uv, -0.5, 0.5) +
		BlurSample(input.uv, 0.5, -0.5) +
		BlurSample(input.uv, -0.5, -0.5);
	return float4(color.rgb * 0.25, 1);
}

float4 DepthStripesPassFragment (VertexOutput input) : SV_TARGET {
	float rawDepth = SAMPLE_DEPTH_TEXTURE(_DepthTex, sampler_DepthTex, input.uv);
	float depth = LinearEyeDepth(rawDepth, _ZBufferParams);
	float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
	#if UNITY_REVERSED_Z
		bool hasDepth = rawDepth != 0;
	#else
		bool hasDepth = rawDepth != 1;
	#endif
	if (hasDepth) {
		color *= pow(sin(3.14 * depth), 2.0);
	}
	return color;
}

#endif // MYRP_POST_EFFECT_STACK_INCLUDED