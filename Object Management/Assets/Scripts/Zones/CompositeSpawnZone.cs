using UnityEngine;

public class CompositeSpawnZone : SpawnZone {

	[SerializeField]
	bool overrideConfig;

	[SerializeField]
	bool sequential;

	[SerializeField]
	SpawnZone[] spawnZones;

	int nextSequentialIndex;

	public override Vector3 SpawnPoint {
		get {
			int index;
			if (sequential) {
				index = nextSequentialIndex++;
				if (nextSequentialIndex >= spawnZones.Length) {
					nextSequentialIndex = 0;
				}
			}
			else {
				index = Random.Range(0, spawnZones.Length);
			}
			return spawnZones[index].SpawnPoint;
		}
	}

	public override void SpawnShapes () {
		if (overrideConfig) {
			base.SpawnShapes();
		}
		else {
			int index;
			if (sequential) {
				index = nextSequentialIndex++;
				if (nextSequentialIndex >= spawnZones.Length) {
					nextSequentialIndex = 0;
				}
			}
			else {
				index = Random.Range(0, spawnZones.Length);
			}
			spawnZones[index].SpawnShapes();
		}
	}

	public override void Save (GameDataWriter writer) {
		base.Save(writer);
		writer.Write(nextSequentialIndex);
	}

	public override void Load (GameDataReader reader) {
		if (reader.Version >= 7) {
			base.Load(reader);
		}
		nextSequentialIndex = reader.ReadInt();
	}
}