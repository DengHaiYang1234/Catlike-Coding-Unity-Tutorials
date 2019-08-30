using UnityEngine;

[ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class MyPipelineCamera : MonoBehaviour {

	[SerializeField]
	MyPostProcessingStack postProcessingStack = null;

	public MyPostProcessingStack PostProcessingStack {
		get {
			return postProcessingStack;
		}
	}
}