using System.IO;
using UnityEngine;

public class GameDataWriter {

	BinaryWriter writer;

	public GameDataWriter (BinaryWriter writer) {
		this.writer = writer;
	}

	public void Write (float value) {
		writer.Write(value);
	}

	public void Write (int value) {
		writer.Write(value);
	}

	public void Write (Color value) {
		writer.Write(value.r);
		writer.Write(value.g);
		writer.Write(value.b);
		writer.Write(value.a);
	}

	public void Write (Quaternion value) {
		writer.Write(value.x);
		writer.Write(value.y);
		writer.Write(value.z);
		writer.Write(value.w);
	}

	public void Write (Random.State value) {
		writer.Write(JsonUtility.ToJson(value));
	}

	public void Write (ShapeInstance value) {
		writer.Write(value.IsValid ? value.Shape.SaveIndex : -1);
	}

	public void Write (Vector3 value) {
		writer.Write(value.x);
		writer.Write(value.y);
		writer.Write(value.z);
	}
}