using UnityEngine;

public class FloatRangeSliderAttribute : PropertyAttribute {

	public float Min { get; private set; }

	public float Max { get; private set; }

	public FloatRangeSliderAttribute (float min, float max) {
		Min = min;
		Max = max < min ? min : max;
	}
}