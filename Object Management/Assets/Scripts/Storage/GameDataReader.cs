using System.IO;
using UnityEngine;

public class GameDataReader {

	public int Version { get; }

	BinaryReader reader;

	public GameDataReader (BinaryReader reader, int version) {
		this.reader = reader;
		this.Version = version;
	}

	public float ReadFloat () {
		return reader.ReadSingle();
	}

	public int ReadInt () {
		return reader.ReadInt32();
	}

	public Color ReadColor () {
		Color value;
		value.r = reader.ReadSingle();
		value.g = reader.ReadSingle();
		value.b = reader.ReadSingle();
		value.a = reader.ReadSingle();
		return value;
	}

	public Quaternion ReadQuaternion () {
		Quaternion value;
		value.x = reader.ReadSingle();
		value.y = reader.ReadSingle();
		value.z = reader.ReadSingle();
		value.w = reader.ReadSingle();
		return value;
	}

	public Random.State ReadRandomState () {
		return JsonUtility.FromJson<Random.State>(reader.ReadString());
	}

	public ShapeInstance ReadShapeInstance () {
		return new ShapeInstance(reader.ReadInt32());
	}

	public Vector3 ReadVector3 () {
		Vector3 value;
		value.x = reader.ReadSingle();
		value.y = reader.ReadSingle();
		value.z = reader.ReadSingle();
		return value;
	}
}