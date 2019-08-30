using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Post-Processing Stack")]
public class MyPostProcessingStack : ScriptableObject {

	enum Pass { Copy, Blur, DepthStripes };

	static int mainTexId = Shader.PropertyToID("_MainTex");
	static int depthTexId = Shader.PropertyToID("_DepthTex");
	static int tempTexId = Shader.PropertyToID("_MyPostProcessingStackTempTex");

	static Mesh fullScreenTriangle;

	static Material material;

	[SerializeField, Range(0, 10)]
	int blurStrength;

	[SerializeField]
	bool depthStripes;

	static void InitializeStatic () {
		if (fullScreenTriangle) {
			return;
		}
		fullScreenTriangle = new Mesh {
			name = "My Post-Processing Stack Full-Screen Triangle",
			vertices = new Vector3[] {
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f,  3f, 0f),
				new Vector3( 3f, -1f, 0f)
			},
			triangles = new int[] { 0, 1, 2 },
		};
		fullScreenTriangle.UploadMeshData(true);

		material =
			new Material(Shader.Find("Hidden/My Pipeline/PostEffectStack")) {
				name = "My Post-Processing Stack Material",
				hideFlags = HideFlags.HideAndDontSave
			};
	}

	public void RenderAfterOpaque (
		CommandBuffer cb, int cameraColorId, int cameraDepthId,
		int width, int height
	) {
		InitializeStatic();
		if (depthStripes) {
			DepthStripes(cb, cameraColorId, cameraDepthId, width, height);
		}
	}

	public void RenderAfterTransparent (
		CommandBuffer cb, int cameraColorId, int cameraDepthId,
		int width, int height
	) {
		if (blurStrength > 0) {
			Blur(cb, cameraColorId, width, height);
		}
		else {
			Blit(cb, cameraColorId, BuiltinRenderTextureType.CameraTarget);
		}
	}

	void Blit (
		CommandBuffer cb,
		RenderTargetIdentifier sourceId, RenderTargetIdentifier destinationId,
		Pass pass = Pass.Copy
	) {
		cb.SetGlobalTexture(mainTexId, sourceId);
		cb.SetRenderTarget(
			destinationId,
			RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
		);
		cb.DrawMesh(
			fullScreenTriangle, Matrix4x4.identity, material, 0, (int)pass
		);
	}

	void Blur (CommandBuffer cb, int cameraColorId, int width, int height) {
		cb.BeginSample("Blur");
		if (blurStrength == 1) {
			Blit(
				cb, cameraColorId, BuiltinRenderTextureType.CameraTarget,Pass.Blur
			);
			cb.EndSample("Blur");
			return;
		}
		cb.GetTemporaryRT(tempTexId, width, height, 0, FilterMode.Bilinear);
		int passesLeft;
		for (passesLeft = blurStrength; passesLeft > 2; passesLeft -= 2) {
			Blit(cb, cameraColorId, tempTexId, Pass.Blur);
			Blit(cb, tempTexId, cameraColorId, Pass.Blur);
		}
		if (passesLeft > 1) {
			Blit(cb, cameraColorId, tempTexId, Pass.Blur);
			Blit(cb, tempTexId, BuiltinRenderTextureType.CameraTarget, Pass.Blur);
		}
		else {
			Blit(
				cb, cameraColorId, BuiltinRenderTextureType.CameraTarget,Pass.Blur
			);
		}
		cb.ReleaseTemporaryRT(tempTexId);
		cb.EndSample("Blur");
	}

	void DepthStripes (
		CommandBuffer cb, int cameraColorId, int cameraDepthId,
		int width, int height
	) {
		cb.BeginSample("Depth Stripes");
		cb.GetTemporaryRT(tempTexId, width, height);
		cb.SetGlobalTexture(depthTexId, cameraDepthId);
		Blit(cb, cameraColorId, tempTexId, Pass.DepthStripes);
		Blit(cb, tempTexId, cameraColorId);
		cb.ReleaseTemporaryRT(tempTexId);
		cb.EndSample("Depth Stripes");
	}
}