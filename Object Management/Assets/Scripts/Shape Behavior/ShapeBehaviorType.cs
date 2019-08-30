public enum ShapeBehaviorType {
	Movement,
	Rotation,
	Oscillation,
	Satellite,
	Growing,
	Dying,
	Lifecycle
}

public static class ShapeBehaviorTypeMethods {

	public static ShapeBehavior GetInstance (this ShapeBehaviorType type) {
		switch (type) {
			case ShapeBehaviorType.Movement:
				return ShapeBehaviorPool<MovementShapeBehavior>.Get();
			case ShapeBehaviorType.Rotation:
				return ShapeBehaviorPool<RotationShapeBehavior>.Get();
			case ShapeBehaviorType.Oscillation:
				return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
			case ShapeBehaviorType.Satellite:
				return ShapeBehaviorPool<SatelliteShapeBehavior>.Get();
			case ShapeBehaviorType.Growing:
				return ShapeBehaviorPool<GrowingShapeBehavior>.Get();
			case ShapeBehaviorType.Dying:
				return ShapeBehaviorPool<DyingShapeBehavior>.Get();
			case ShapeBehaviorType.Lifecycle:
				return ShapeBehaviorPool<LifecycleShapeBehavior>.Get();
		}
		UnityEngine.Debug.Log("Forgot to support " + type);
		return null;
	}
}