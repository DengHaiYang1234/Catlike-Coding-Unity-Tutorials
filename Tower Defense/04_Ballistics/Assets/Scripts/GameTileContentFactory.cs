using UnityEngine;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory {

	[SerializeField]
	GameTileContent destinationPrefab = default;

	[SerializeField]
	GameTileContent emptyPrefab = default;

	[SerializeField]
	GameTileContent wallPrefab = default;

	[SerializeField]
	GameTileContent spawnPointPrefab = default;

	[SerializeField]
	Tower[] towerPrefabs = default;

	public GameTileContent Get (GameTileContentType type) {
		switch (type) {
			case GameTileContentType.Destination: return Get(destinationPrefab);
			case GameTileContentType.Empty: return Get(emptyPrefab);
			case GameTileContentType.Wall: return Get(wallPrefab);
			case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
			//case GameTileContentType.Tower: return Get(towerPrefab);
		}
		Debug.Assert(false, "Unsupported non-tower type: " + type);
		return null;
	}

	public Tower Get (TowerType type) {
		Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
		Tower prefab = towerPrefabs[(int)type];
		Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
		return Get(prefab);
	}

	public void Reclaim (GameTileContent content) {
		Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(content.gameObject);
	}

	T Get<T> (T prefab) where T : GameTileContent {
		T instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}
}