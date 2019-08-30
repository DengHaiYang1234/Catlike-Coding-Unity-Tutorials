#ifndef MYRP_LIGHTING_INCLUDED
#define MYRP_LIGHTING_INCLUDED

struct LitSurface {
	float3 normal, position, viewDir;
	float3 diffuse, specular;
	float perceptualRoughness, roughness, fresnelStrength, reflectivity;
	bool perfectDiffuser;
};

LitSurface GetLitSurface (
	float3 normal, float3 position, float3 viewDir,
	float3 color, float metallic, float smoothness, bool perfectDiffuser = false
) {
	LitSurface s;
	s.normal = normal;
	s.position = position;
	s.viewDir = viewDir;
	s.diffuse = color;
	if (perfectDiffuser) {
		s.reflectivity = 0.0;
		smoothness = 0.0;
		s.specular = 0.0;
	}
	else {
		s.specular = lerp(0.04, color, metallic);
		s.reflectivity = lerp(0.04, 1.0, metallic);
		s.diffuse *= 1.0 - s.reflectivity;
	}
	s.perfectDiffuser = perfectDiffuser;
	s.perceptualRoughness = 1.0 - smoothness;
	s.roughness = s.perceptualRoughness * s.perceptualRoughness;
	s.fresnelStrength = saturate(smoothness + s.reflectivity);
	return s;
}

LitSurface GetLitSurfaceVertex (float3 normal, float3 position) {
	return GetLitSurface(normal, position, 0, 1, 0, 0, true);
}

LitSurface GetLitSurfaceMeta (float3 color, float metallic, float smoothness) {
	return GetLitSurface(0, 0, 0, color, metallic, smoothness);
}

void PremultiplyAlpha (inout LitSurface s, inout float alpha) {
	s.diffuse *= alpha;
	alpha = lerp(alpha, 1, s.reflectivity);
}

float3 LightSurface (LitSurface s, float3 lightDir) {
	float3 color = s.diffuse;
	
	if (!s.perfectDiffuser) {
		float3 halfDir = SafeNormalize(lightDir + s.viewDir);
		float nh = saturate(dot(s.normal, halfDir));
		float lh = saturate(dot(lightDir, halfDir));
		float d = nh * nh * (s.roughness * s.roughness - 1.0) + 1.00001;
		float normalizationTerm = s.roughness * 4.0 + 2.0;
		float specularTerm = s.roughness * s.roughness;
		specularTerm /= (d * d) * max(0.1, lh * lh) * normalizationTerm;
		color += specularTerm * s.specular;
	}
	
	return color * saturate(dot(s.normal, lightDir));
}

float3 ReflectEnvironment (LitSurface s, float3 environment) {
	if (s.perfectDiffuser) {
		return 0;
	}
	
	float fresnel = Pow4(1.0 - saturate(dot(s.normal, s.viewDir)));
	environment *= lerp(s.specular, s.fresnelStrength, fresnel);
	environment /= s.roughness * s.roughness + 1.0;
	return environment;
}

#endif // MYRP_LIGHTING_INCLUDED