using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline {

	const int maxVisibleLights = 16;

	const string shadowsHardKeyword = "_SHADOWS_HARD";
	const string shadowsSoftKeyword = "_SHADOWS_SOFT";

	static int visibleLightColorsId =
		Shader.PropertyToID("_VisibleLightColors");
	static int visibleLightDirectionsOrPositionsId =
		Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
	static int visibleLightAttenuationsId =
		Shader.PropertyToID("_VisibleLightAttenuations");
	static int visibleLightSpotDirectionsId =
		Shader.PropertyToID("_VisibleLightSpotDirections");
	static int lightIndicesOffsetAndCountID =
		Shader.PropertyToID("unity_LightIndicesOffsetAndCount");
	static int shadowMapId = Shader.PropertyToID("_ShadowMap");
	static int worldToShadowMatricesId =
		Shader.PropertyToID("_WorldToShadowMatrices");
	static int shadowBiasId = Shader.PropertyToID("_ShadowBias");
	static int shadowDataId = Shader.PropertyToID("_ShadowData");
	static int shadowMapSizeId = Shader.PropertyToID("_ShadowMapSize");

	Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
	Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];
	Vector4[] visibleLightAttenuations = new Vector4[maxVisibleLights];
	Vector4[] visibleLightSpotDirections = new Vector4[maxVisibleLights];

	CullResults cull;

	RenderTexture shadowMap;
	Vector4[] shadowData = new Vector4[maxVisibleLights];
	Matrix4x4[] worldToShadowMatrices = new Matrix4x4[maxVisibleLights];

	Material errorMaterial;

	CommandBuffer cameraBuffer = new CommandBuffer {
		name = "Render Camera"
	};

	CommandBuffer shadowBuffer = new CommandBuffer {
		name = "Render Shadows"
	};

	DrawRendererFlags drawFlags;

	int shadowMapSize;
	int shadowTileCount;

	public MyPipeline (
		bool dynamicBatching, bool instancing, int shadowMapSize
	) {
		GraphicsSettings.lightsUseLinearIntensity = true;
		if (dynamicBatching) {
			drawFlags = DrawRendererFlags.EnableDynamicBatching;
		}
		if (instancing) {
			drawFlags |= DrawRendererFlags.EnableInstancing;
		}
		this.shadowMapSize = shadowMapSize;
	}

	public override void Render (
		ScriptableRenderContext renderContext, Camera[] cameras
	) {
		base.Render(renderContext, cameras);

		foreach (var camera in cameras) {
			Render(renderContext, camera);
		}
	}

	void Render (ScriptableRenderContext context, Camera camera) {
		ScriptableCullingParameters cullingParameters;
		if (!CullResults.GetCullingParameters(camera, out cullingParameters)) {
			return;
		}

#if UNITY_EDITOR
		if (camera.cameraType == CameraType.SceneView) {
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
#endif

		CullResults.Cull(ref cullingParameters, context, ref cull);
		if (cull.visibleLights.Count > 0) {
			ConfigureLights();
			if (shadowTileCount > 0) {
				RenderShadows(context);
			}
			else {
				cameraBuffer.DisableShaderKeyword(shadowsHardKeyword);
				cameraBuffer.DisableShaderKeyword(shadowsSoftKeyword);
			}
		}
		else {
			cameraBuffer.SetGlobalVector(
				lightIndicesOffsetAndCountID, Vector4.zero
			);
			cameraBuffer.DisableShaderKeyword(shadowsHardKeyword);
			cameraBuffer.DisableShaderKeyword(shadowsSoftKeyword);
		}

		context.SetupCameraProperties(camera);

		CameraClearFlags clearFlags = camera.clearFlags;
		cameraBuffer.ClearRenderTarget(
			(clearFlags & CameraClearFlags.Depth) != 0,
			(clearFlags & CameraClearFlags.Color) != 0,
			camera.backgroundColor
		);

		cameraBuffer.BeginSample("Render Camera");
		cameraBuffer.SetGlobalVectorArray(
			visibleLightColorsId, visibleLightColors
		);
		cameraBuffer.SetGlobalVectorArray(
			visibleLightDirectionsOrPositionsId, visibleLightDirectionsOrPositions
		);
		cameraBuffer.SetGlobalVectorArray(
			visibleLightAttenuationsId, visibleLightAttenuations
		);
		cameraBuffer.SetGlobalVectorArray(
			visibleLightSpotDirectionsId, visibleLightSpotDirections
		);
		context.ExecuteCommandBuffer(cameraBuffer);
		cameraBuffer.Clear();

		var drawSettings = new DrawRendererSettings(
			camera, new ShaderPassName("SRPDefaultUnlit")
		) {
			flags = drawFlags
		};
		if (cull.visibleLights.Count > 0) {
			drawSettings.rendererConfiguration =
				RendererConfiguration.PerObjectLightIndices8;
		}
		drawSettings.sorting.flags = SortFlags.CommonOpaque;

		var filterSettings = new FilterRenderersSettings(true) {
			renderQueueRange = RenderQueueRange.opaque
		};

		context.DrawRenderers(
			cull.visibleRenderers, ref drawSettings, filterSettings
		);

		context.DrawSkybox(camera);

		drawSettings.sorting.flags = SortFlags.CommonTransparent;
		filterSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(
			cull.visibleRenderers, ref drawSettings, filterSettings
		);

		DrawDefaultPipeline(context, camera);

		cameraBuffer.EndSample("Render Camera");
		context.ExecuteCommandBuffer(cameraBuffer);
		cameraBuffer.Clear();

		context.Submit();

		if (shadowMap) {
			RenderTexture.ReleaseTemporary(shadowMap);
			shadowMap = null;
		}
	}

	void ConfigureLights () {
		shadowTileCount = 0;
		for (int i = 0; i < cull.visibleLights.Count; i++) {
			if (i == maxVisibleLights) {
				break;
			}
			VisibleLight light = cull.visibleLights[i];
			visibleLightColors[i] = light.finalColor;
			Vector4 attenuation = Vector4.zero;
			attenuation.w = 1f;
			Vector4 shadow = Vector4.zero;

			if (light.lightType == LightType.Directional) {
				Vector4 v = light.localToWorld.GetColumn(2);
				v.x = -v.x;
				v.y = -v.y;
				v.z = -v.z;
				visibleLightDirectionsOrPositions[i] = v;
			}
			else {
				visibleLightDirectionsOrPositions[i] =
					light.localToWorld.GetColumn(3);
				attenuation.x = 1f /
					Mathf.Max(light.range * light.range, 0.00001f);

				if (light.lightType == LightType.Spot) {
					Vector4 v = light.localToWorld.GetColumn(2);
					v.x = -v.x;
					v.y = -v.y;
					v.z = -v.z;
					visibleLightSpotDirections[i] = v;

					float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
					float outerCos = Mathf.Cos(outerRad);
					float outerTan = Mathf.Tan(outerRad);
					float innerCos =
						Mathf.Cos(Mathf.Atan((46f / 64f) * outerTan));
					float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
					attenuation.z = 1f / angleRange;
					attenuation.w = -outerCos * attenuation.z;

					Light shadowLight = light.light;
					Bounds shadowBounds;
					if (
						shadowLight.shadows != LightShadows.None &&
						cull.GetShadowCasterBounds(i, out shadowBounds)
					) {
						shadowTileCount += 1;
						shadow.x = shadowLight.shadowStrength;
						shadow.y =
							shadowLight.shadows == LightShadows.Soft ? 1f : 0f;
					}
				}
			}

			visibleLightAttenuations[i] = attenuation;
			shadowData[i] = shadow;
		}

		if (cull.visibleLights.Count > maxVisibleLights) {
			int[] lightIndices = cull.GetLightIndexMap();
			for (int i = maxVisibleLights; i < cull.visibleLights.Count; i++) {
				lightIndices[i] = -1;
			}
			cull.SetLightIndexMap(lightIndices);
		}
	}

	void RenderShadows (ScriptableRenderContext context) {
		int split;
		if (shadowTileCount <= 1) {
			split = 1;
		}
		else if (shadowTileCount <= 4) {
			split = 2;
		}
		else if (shadowTileCount <= 9) {
			split = 3;
		}
		else {
			split = 4;
		}

		float tileSize = shadowMapSize / split;
		float tileScale = 1f / split;
		Rect tileViewport = new Rect(0f, 0f, tileSize, tileSize);

		shadowMap = RenderTexture.GetTemporary(
			shadowMapSize, shadowMapSize, 16, RenderTextureFormat.Shadowmap
		);
		shadowMap.filterMode = FilterMode.Bilinear;
		shadowMap.wrapMode = TextureWrapMode.Clamp;

		CoreUtils.SetRenderTarget(
			shadowBuffer, shadowMap,
			RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
			ClearFlag.Depth
		);
		shadowBuffer.BeginSample("Render Shadows");
		context.ExecuteCommandBuffer(shadowBuffer);
		shadowBuffer.Clear();

		int tileIndex = 0;
		bool hardShadows = false;
		bool softShadows = false;
		for (int i = 0; i < cull.visibleLights.Count; i++) {
			if (i == maxVisibleLights) {
				break;
			}
			if (shadowData[i].x <= 0f) {
				continue;
			}

			Matrix4x4 viewMatrix, projectionMatrix;
			ShadowSplitData splitData;
			if (!cull.ComputeSpotShadowMatricesAndCullingPrimitives(
				i, out viewMatrix, out projectionMatrix, out splitData
			)) {
				shadowData[i].x = 0f;
				continue;
			}

			float tileOffsetX = tileIndex % split;
			float tileOffsetY = tileIndex / split;
			tileViewport.x = tileOffsetX * tileSize;
			tileViewport.y = tileOffsetY * tileSize;
			if (split > 1) {
				shadowBuffer.SetViewport(tileViewport);
				shadowBuffer.EnableScissorRect(new Rect(
					tileViewport.x + 4f, tileViewport.y + 4f,
					tileSize - 8f, tileSize - 8f
				));
			}
			shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
			shadowBuffer.SetGlobalFloat(
				shadowBiasId, cull.visibleLights[i].light.shadowBias
			);
			context.ExecuteCommandBuffer(shadowBuffer);
			shadowBuffer.Clear();

			var shadowSettings = new DrawShadowsSettings(cull, i);
			context.DrawShadows(ref shadowSettings);

			if (SystemInfo.usesReversedZBuffer) {
				projectionMatrix.m20 = -projectionMatrix.m20;
				projectionMatrix.m21 = -projectionMatrix.m21;
				projectionMatrix.m22 = -projectionMatrix.m22;
				projectionMatrix.m23 = -projectionMatrix.m23;
			}
			var scaleOffset = Matrix4x4.identity;
			scaleOffset.m00 = scaleOffset.m11 = scaleOffset.m22 = 0.5f;
			scaleOffset.m03 = scaleOffset.m13 = scaleOffset.m23 = 0.5f;
			worldToShadowMatrices[i] =
				scaleOffset * (projectionMatrix * viewMatrix);

			if (split > 1) {
				var tileMatrix = Matrix4x4.identity;
				tileMatrix.m00 = tileMatrix.m11 = tileScale;
				tileMatrix.m03 = tileOffsetX * tileScale;
				tileMatrix.m13 = tileOffsetY * tileScale;
				worldToShadowMatrices[i] = tileMatrix * worldToShadowMatrices[i];
			}
			tileIndex += 1;
			if (shadowData[i].y <= 0f) {
				hardShadows = true;
			}
			else {
				softShadows = true;
			}
		}

		if (split > 1) {
			shadowBuffer.DisableScissorRect();
		}
		shadowBuffer.SetGlobalTexture(shadowMapId, shadowMap);
		shadowBuffer.SetGlobalMatrixArray(
			worldToShadowMatricesId, worldToShadowMatrices
		);
		shadowBuffer.SetGlobalVectorArray(shadowDataId, shadowData);
		float invShadowMapSize = 1f / shadowMapSize;
		shadowBuffer.SetGlobalVector(
			shadowMapSizeId, new Vector4(
				invShadowMapSize, invShadowMapSize, shadowMapSize, shadowMapSize
			)
		);
		CoreUtils.SetKeyword(shadowBuffer, shadowsHardKeyword, hardShadows);
		CoreUtils.SetKeyword(shadowBuffer, shadowsSoftKeyword, softShadows);
		shadowBuffer.EndSample("Render Shadows");
		context.ExecuteCommandBuffer(shadowBuffer);
		shadowBuffer.Clear();
	}

	[Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
	void DrawDefaultPipeline (ScriptableRenderContext context, Camera camera) {
		if (errorMaterial == null) {
			Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
			errorMaterial = new Material(errorShader) {
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		var drawSettings = new DrawRendererSettings(
			camera, new ShaderPassName("ForwardBase")
		);
		drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
		drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
		drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
		drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
		drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
		drawSettings.SetOverrideMaterial(errorMaterial, 0);

		var filterSettings = new FilterRenderersSettings(true);

		context.DrawRenderers(
			cull.visibleRenderers, ref drawSettings, filterSettings
		);
	}
}