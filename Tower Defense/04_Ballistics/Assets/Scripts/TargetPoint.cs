using UnityEngine;

public class TargetPoint : MonoBehaviour {

	const int enemyLayerMask = 1 << 9;

	static Collider[] buffer = new Collider[100];

	public static int BufferedCount { get; private set; }

	public static TargetPoint RandomBuffered =>
		GetBuffered(Random.Range(0, BufferedCount));

	public static bool FillBuffer (Vector3 position, float range) {
		Vector3 top = position;
		top.y += 3f;
		BufferedCount = Physics.OverlapCapsuleNonAlloc(
			position, top, range, buffer, enemyLayerMask
		);
		return BufferedCount > 0;
	}

	public static TargetPoint GetBuffered (int index) {
		var target = buffer[index].GetComponent<TargetPoint>();
		Debug.Assert(target != null, "Targeted non-enemy!", buffer[0]);
		return target;
	}

	public Enemy Enemy { get; private set; }

	public Vector3 Position => transform.position;

	void Awake () {
		Enemy = transform.root.GetComponent<Enemy>();
		Debug.Assert(Enemy != null, "Target point without Enemy root!", this);
		Debug.Assert(
			GetComponent<SphereCollider>() != null,
			"Target point without sphere collider!", this
		);
		Debug.Assert(gameObject.layer == 9, "Target point on wrong layer!", this);
	}
}