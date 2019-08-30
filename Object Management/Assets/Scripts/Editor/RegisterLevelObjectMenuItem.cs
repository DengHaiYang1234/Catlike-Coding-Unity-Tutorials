using UnityEditor;
using UnityEngine;

static class RegisterLevelObjectMenuItem {

	const string menuItem = "GameObject/Register Level Object";

	[MenuItem(menuItem, true)]
	static bool ValidateRegisterLevelObject () {
		if (Selection.objects.Length == 0) {
			return false;
		}
		foreach (Object o in Selection.objects) {
			if (!(o is GameObject)) {
				return false;
			}
		}
		return true;
	}

	[MenuItem(menuItem)]
	static void RegisterLevelObject () {
		if (Selection.objects.Length == 0) {
			Debug.LogWarning("No level object selected.");
			return;
		}

		foreach (Object o in Selection.objects) {
			Register(o as GameObject);
		}
	}

	static void Register (GameObject o) {
		if (PrefabUtility.GetPrefabType(o) == PrefabType.Prefab) {
			Debug.LogWarning(o.name + " is a prefab asset.", o);
			return;
		}

		var levelObject = o.GetComponent<GameLevelObject>();
		if (levelObject == null) {
			Debug.LogWarning(o.name + " isn't a game level object.", o);
			return;
		}

		foreach (GameObject rootObject in o.scene.GetRootGameObjects()) {
			var gameLevel = rootObject.GetComponent<GameLevel>();
			if (gameLevel != null) {
				if (gameLevel.HasLevelObject(levelObject)) {
					Debug.LogWarning(o.name + " is already registered.", o);
					return;
				}

				Undo.RecordObject(gameLevel, "Register Level Object.");
				gameLevel.RegisterLevelObject(levelObject);
				Debug.Log(
					o.name + " registered to game level " +
					gameLevel.name + " in scene " + o.scene.name + ".", o
				);
				return;
			}
		}
		Debug.LogWarning(o.name + " isn't part of a game level.", o);
	}
}