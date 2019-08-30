using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour {

	[SerializeField]
	GameTileContentType type = default;

	GameTileContentFactory originFactory;

	public bool BlocksPath =>
		Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

	public GameTileContentType Type => type;

	public GameTileContentFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	public void Recycle () {
		originFactory.Reclaim(this);
	}

	public virtual void GameUpdate () { }
}