using UnityEngine;
public class GameManager : MonoBehaviour {
	public static GameManager Instance;

	public Vector3 directShotVelocity;
	public Vector3 indirectShotVelocity;

	[SerializeField]
	private Transform hoop;
	[SerializeField]
	private Transform backboard;
	[SerializeField]
	private Detector detector;
	[SerializeField]
	private UIManager uiManager;

	private Transform _player;
	private Transform _npc;

	private int playerPoints;

	private int PlayerPoints {
		set {
			playerPoints = value;
			uiManager.SetPlayerPoints(value);
		}
		get => playerPoints;
	}
	private int npcPoints;
	private int NPCPoints {
		set {
			npcPoints = value;
			uiManager.SetNPCPoints(value);
		}
		get => npcPoints;
	}

	[Header("Shot tuning")]
	[SerializeField]
	private float maxHeight = 4.5f;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Debug.LogWarning("Multiple game managers in the scene!");
			Destroy(gameObject);
		}
	}

	private void Start() {
		_player = FindObjectOfType<PlayerController>().transform;
		RecalculateShots();

		detector.onBallEnter.AddListener(AddPoints);
	}
	private void RecalculateShots() {
		if (!CalculateDirectShot(_player, out directShotVelocity)) {
			Debug.LogWarning("Unable to find direct shot");
		}

		if (!CalculateBankShot(_player, out indirectShotVelocity)) {
			Debug.LogWarning("Unable to find indirect shot");
		}
		
		//TODO calculate npc shots and indirect shots
	}

	private bool CalculateDirectShot(Transform startPoint, out Vector3 launchVelocity) {
		var toTarget = hoop.position - startPoint.position;
		var toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
		var distance = toTargetXZ.magnitude;

		var gravity = Mathf.Abs(Physics.gravity.y);
		var startY = startPoint.position.y;

		if (maxHeight <= Mathf.Max(startY, hoop.position.y)) {
			Debug.LogWarning("maxHeight is too low to reach the hoop.");
			launchVelocity = Vector3.zero;
			return false;
		}

		var vy = Mathf.Sqrt(2f * gravity * (maxHeight - startY));

		var timeUp = vy / gravity;
		var fallDistance = maxHeight - hoop.position.y;

		if (fallDistance < 0) {
			launchVelocity = Vector3.zero;
			return false;
		}

		var timeDown = Mathf.Sqrt(2f * fallDistance / gravity);
		var totalTime = timeUp + timeDown;

		var vxz = distance / totalTime;
		var dirXZ = toTargetXZ.normalized;

		launchVelocity = dirXZ * vxz + Vector3.up * vy;

		return true;
	}


	private bool CalculateBankShot(Transform startPoint, out Vector3 launchVelocity) {
		launchVelocity = Vector3.zero;

		if (backboard == null || hoop == null) {
			Debug.LogWarning("Backboard o Hoop non assegnati!");
			return false;
		}

		var boardNormal = backboard.forward;
		var hoopToBoard = hoop.position - backboard.position;
		var distToPlane = Vector3.Dot(hoopToBoard, boardNormal);
		
		var reflectedHoop = hoop.position - 2f * distToPlane * boardNormal;
		
		var toTarget = reflectedHoop - startPoint.position;
		var toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
		var distance = toTargetXZ.magnitude;
		var startY = startPoint.position.y;

		var gravity = Mathf.Abs(Physics.gravity.y);

		
		if (maxHeight <= Mathf.Max(startY, reflectedHoop.y)) {
			Debug.LogWarning("maxHeight too low to reach the reflected hoop.");
			return false;
		}
		
		var vy = Mathf.Sqrt(2f * gravity * (maxHeight - startY));
		
		var timeUp = vy / gravity;
		var fallDistance = maxHeight - reflectedHoop.y;
		if (fallDistance < 0f)
			return false;

		var timeDown = Mathf.Sqrt(2f * fallDistance / gravity);
		var totalTime = timeUp + timeDown;
		
		var vxz = distance / totalTime;
		var dirXZ = toTargetXZ.normalized;
		
		launchVelocity = dirXZ * vxz + Vector3.up * vy;

		return true;
	}


	private void AddPoints(int playerId) {
		Debug.Log($"Adding points to id {playerId}");
		switch (playerId) {
			case 0:
				PlayerPoints++;
				break;
			case 1:
				NPCPoints++;
				break;
		}
	}
}
