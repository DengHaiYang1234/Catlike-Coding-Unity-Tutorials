using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset {

	public enum ShadowMapSize {
		_256 = 256,
		_512 = 512,
		_1024 = 1024,
		_2048 = 2048,
		_4096 = 4096
	}

	public enum ShadowCascades {
		Zero = 0,
		Two = 2,
		Four = 4
	}

	public bool HasLODCrossFading {
		get {
			return supportLODCrossFading;
		}
	}

	public bool HasShadowCascades {
		get {
			return shadowCascades != ShadowCascades.Zero;
		}
	}

	[SerializeField]
	bool dynamicBatching;

	[SerializeField]
	bool instancing;

	[SerializeField]
	MyPostProcessingStack defaultStack = null;

	[SerializeField]
	Texture2D ditherTexture = null;

	[SerializeField, Range(0f, 120f)]
	float ditherAnimationSpeed = 30f;

	[SerializeField]
	bool supportLODCrossFading = true;

	[SerializeField]
	ShadowMapSize shadowMapSize = ShadowMapSize._1024;

	[SerializeField]
	float shadowDistance = 100f;

	[SerializeField, Range(0.01f, 2f)]
	float shadowFadeRange = 1f;

	[SerializeField]
	ShadowCascades shadowCascades = ShadowCascades.Four;

	[SerializeField, HideInInspector]
	float twoCascadesSplit = 0.25f;

	[SerializeField, HideInInspector]
	Vector3 fourCascadesSplit = new Vector3(0.067f, 0.2f, 0.467f);

	protected override IRenderPipeline InternalCreatePipeline () {
		Vector3 shadowCascadeSplit = shadowCascades == ShadowCascades.Four ?
			fourCascadesSplit : new Vector3(twoCascadesSplit, 0f);
		return new MyPipeline(
			dynamicBatching, instancing, defaultStack,
			ditherTexture, ditherAnimationSpeed,
			(int)shadowMapSize, shadowDistance, shadowFadeRange,
			(int)shadowCascades, shadowCascadeSplit
		);
	}
}