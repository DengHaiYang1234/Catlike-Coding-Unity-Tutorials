using UnityEngine;

[System.Serializable]
public struct IntRange {

	public int min, max;

	public int RandomValueInRange {
		get {
			return Random.Range(min, max + 1);
		}
	}
}