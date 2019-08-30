using UnityEngine;

public sealed class GrowingShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Growing;
		}
	}

	Vector3 originalScale;
	float duration;

	public void Initialize (Shape shape, float duration) {
		originalScale = shape.transform.localScale;
		this.duration = duration;
		shape.transform.localScale = Vector3.zero;
	}

	public override bool GameUpdate (Shape shape) {
		if (shape.Age < duration) {
			float s = shape.Age / duration;
			s = (3f - 2f * s) * s * s;
			shape.transform.localScale = s * originalScale;
			return true;
		}
		shape.transform.localScale = originalScale;
		return false;
	}

	public override void Save (GameDataWriter writer) {
		writer.Write(originalScale);
		writer.Write(duration);
	}

	public override void Load (GameDataReader reader) {
		originalScale = reader.ReadVector3();
		duration = reader.ReadFloat();
	}

	public override void Recycle () {
		ShapeBehaviorPool<GrowingShapeBehavior>.Reclaim(this);
	}
}