using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour {
	public static GameManager Instance;

	[Header("Debug Shot Velocity")]
	public Vector3 directShotVelocity;
	public Vector3 bankShotVelocity;

	[Header("Components")]
	[SerializeField] private Transform hoop;
	[SerializeField] private Transform backboard;
	[SerializeField] private Detector detector;
	[SerializeField] private UIManager uiManager;
	[SerializeField] private TextMeshProUGUI backboardText;
	
	[Header("Shoot Race Settings")]
	[SerializeField] private Transform[] shootingPoints;
	[SerializeField] private int normalPoints = 2;
	[SerializeField] private int perfectPoints = 3;
	
	[Header("Shot tuning")] [SerializeField]
	private float maxHeight = 4.5f;

	private Transform _player;
	private Transform _npc;
	private int backboardBonus = 0;
	
	private int _playerPoints;
	private int PlayerPoints
	{
		set
		{
			_playerPoints = value;
			uiManager.SetPlayerPoints(value);
		}
		get => _playerPoints;
	}

	private int _npcPoints; 
	private int NPCPoints
	{
		set
		{
			_npcPoints = value;
			uiManager.SetNPCPoints(value);
		}
		get => _npcPoints;
	}
	
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

		if (!CalculateBankShot(_player, out bankShotVelocity)) {
			Debug.LogWarning("Unable to find indirect shot");
		}

		//TODO calculate npc shots shots
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

	private void SetBackboardPoints(int bonus) {
		backboardBonus = bonus;
		backboardText.text = $"+{bonus}";
		//TODO make Backboard Blink
	}

	private void AddPoints(int playerId, bool perfect, bool backShot) {
		var points = perfect ? perfectPoints : normalPoints;
		points += backShot ? backboardBonus : 0;
		Debug.Log($"Adding points to id {playerId}");
		switch (playerId) {
			case 0:
				PlayerPoints += points;
				break;
			case 1:
				NPCPoints  += points;
				break;
		}
	}
}
