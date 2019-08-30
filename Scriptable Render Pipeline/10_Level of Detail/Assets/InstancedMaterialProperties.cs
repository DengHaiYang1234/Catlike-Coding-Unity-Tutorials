using UnityEngine;

public class InstancedMaterialProperties : MonoBehaviour {

	static MaterialPropertyBlock propertyBlock;

	static int colorID = Shader.PropertyToID("_Color");
	static int metallicId = Shader.PropertyToID("_Metallic");
	static int smoothnessId = Shader.PropertyToID("_Smoothness");
	static int emissionColorId = Shader.PropertyToID("_EmissionColor");

	[SerializeField]
	Color color = Color.white;

	[SerializeField, Range(0f, 1f)]
	float metallic;

	[SerializeField, Range(0f, 1f)]
	float smoothness = 0.5f;

	[SerializeField, ColorUsage(false, true)]
	Color emissionColor = Color.black;

	[SerializeField]
	float pulseEmissionFreqency;

	void Awake () {
		OnValidate();
		if (pulseEmissionFreqency <= 0f) {
			enabled = false;
		}
	}

	void Update () {
		Color originalEmissionColor = emissionColor;
		emissionColor *= 0.5f +
			0.5f * Mathf.Cos(2f * Mathf.PI * pulseEmissionFreqency * Time.time);
		OnValidate();
		DynamicGI.SetEmissive(GetComponent<MeshRenderer>(), emissionColor);
		emissionColor = originalEmissionColor;
	}

	void OnValidate () {
		if (propertyBlock == null) {
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetColor(colorID, color);
		propertyBlock.SetFloat(metallicId, metallic);
		propertyBlock.SetFloat(smoothnessId, smoothness);
		propertyBlock.SetColor(emissionColorId, emissionColor);
		GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
	}
}