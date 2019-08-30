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

	[SerializeField]
	ShadowMapSize shadowMapSize = ShadowMapSize._1024;

	[SerializeField]
	bool dynamicBatching;

	[SerializeField]
	bool instancing;

	protected override IRenderPipeline InternalCreatePipeline () {
		return new MyPipeline(
			dynamicBatching, instancing, (int)shadowMapSize
		);
	}
}