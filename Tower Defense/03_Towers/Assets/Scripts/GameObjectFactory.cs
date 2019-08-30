using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameObjectFactory : ScriptableObject {

	Scene scene;

	protected T CreateGameObjectInstance<T> (T prefab) where T : MonoBehaviour {
		if (!scene.isLoaded) {
			if (Application.isEditor) {
				scene = SceneManager.GetSceneByName(name);
				if (!scene.isLoaded) {
					scene = SceneManager.CreateScene(name);
				}
			}
			else {
				scene = SceneManager.CreateScene(name);
			}
		}
		T instance = Instantiate(prefab);
		SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
		return instance;
	}
}