using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory {

	[SerializeField]
	Enemy prefab = default;

	[SerializeField, FloatRangeSlider(0.5f, 2f)]
	FloatRange scale = new FloatRange(1f);

	[SerializeField, FloatRangeSlider(0.2f, 5f)]
	FloatRange speed = new FloatRange(1f);

	[SerializeField, FloatRangeSlider(-0.4f, 0.4f)]
	FloatRange pathOffset = new FloatRange(0f);

	public Enemy Get () {
		Enemy instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		instance.Initialize(
			scale.RandomValueInRange,
			speed.RandomValueInRange,
			pathOffset.RandomValueInRange
		);
		return instance;
	}

	public void Reclaim (Enemy enemy) {
		Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(enemy.gameObject);
	}
}