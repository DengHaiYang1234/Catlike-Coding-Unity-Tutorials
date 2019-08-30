using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory {

	[System.Serializable]
	class EnemyConfig {

		public Enemy prefab = default;

		[FloatRangeSlider(0.5f, 2f)]
		public FloatRange scale = new FloatRange(1f);

		[FloatRangeSlider(0.2f, 5f)]
		public FloatRange speed = new FloatRange(1f);

		[FloatRangeSlider(-0.4f, 0.4f)]
		public FloatRange pathOffset = new FloatRange(0f);

		[FloatRangeSlider(10f, 1000f)]
		public FloatRange health = new FloatRange(100f);
	}

	[SerializeField]
	EnemyConfig small = default, medium = default, large = default;

	EnemyConfig GetConfig (EnemyType type) {
		switch (type) {
			case EnemyType.Small: return small;
			case EnemyType.Medium: return medium;
			case EnemyType.Large: return large;
		}
		Debug.Assert(false, "Unsupported enemy type!");
		return null;
	}

	public Enemy Get (EnemyType type = EnemyType.Medium) {
		EnemyConfig config = GetConfig(type);
		Enemy instance = CreateGameObjectInstance(config.prefab);
		instance.OriginFactory = this;
		instance.Initialize(
			config.scale.RandomValueInRange,
			config.speed.RandomValueInRange,
			config.pathOffset.RandomValueInRange,
			config.health.RandomValueInRange
		);
		return instance;
	}

	public void Reclaim (Enemy enemy) {
		Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(enemy.gameObject);
	}
}