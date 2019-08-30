using UnityEngine;

public sealed class RotationShapeBehavior : ShapeBehavior {

	public override ShapeBehaviorType BehaviorType {
		get {
			return ShapeBehaviorType.Rotation;
		}
	}

	public Vector3 AngularVelocity { get; set; }

	public override bool GameUpdate (Shape shape) {
		shape.transform.Rotate(AngularVelocity * Time.deltaTime);
		return true;
	}

	public override void Save (GameDataWriter writer) {
		writer.Write(AngularVelocity);
	}

	public override void Load (GameDataReader reader) {
		AngularVelocity = reader.ReadVector3();
	}

	public override void Recycle () {
		ShapeBehaviorPool<RotationShapeBehavior>.Reclaim(this);
	}
}