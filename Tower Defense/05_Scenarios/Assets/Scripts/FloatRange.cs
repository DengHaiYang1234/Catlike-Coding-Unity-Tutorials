using UnityEngine;

[System.Serializable]
public struct FloatRange {

	[SerializeField]
	float min, max;

	public float Min => min;

	public float Max => max;

	public float RandomValueInRange {
		get {
			return Random.Range(min, max);
		}
	}

	public FloatRange (float value) {
		min = max = value;
	}

	public FloatRange (float min, float max) {
		this.min = min;
		this.max = max < min ? min : max;
	}
}