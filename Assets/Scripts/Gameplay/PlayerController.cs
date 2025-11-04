using Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gameplay {
	public class PlayerController : MonoBehaviour, IShotController {
		[Header("References")]
		[SerializeField] private Slider slider;
		[SerializeField] private GameEvents gameEvents;
		
		[Header("Input")]
		[SerializeField] private float mouseSensitivity = 0.1f;
		[SerializeField] private float touchSensitivity = 0.1f;
		[SerializeField] private float maxTime = 1f;

		[Header("Gameplay")]
		[SerializeField] private GameObject ballPrefab;
		[SerializeField, Range(0f, 0.3f)] private float perfectThreshold = 0.1f;
		[SerializeField] private float maxShotPowerMultiplier = 1.2f;
		[SerializeField] private float minPowerFraction = 0.3f;
		[SerializeField] private float shotStartHeight = 1.8f;

		private bool _isDragging;
		private float _startY;
		private float _endY;
		private float _timer;
		private bool _cooldown;


		private GameManager _gm;
		private Vector3 _directShotVelocity;
		private Vector3 _bankShotVelocity;
		private Vector3 _shotStartPoint;

		private bool _fireballActive;
		private float _currentSensitivity;

		private void Start() {
			_gm = GameManager.Instance;
			gameEvents.OnFireStateChanged.AddListener(TrackFireballState);
			gameEvents.OnScoreAdded.AddListener(OnScoreAdded);
			gameEvents.OnShotMiss.AddListener(OnShotMiss);

		#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			_currentSensitivity = touchSensitivity;
			Debug.Log("Using touch sensitivity");
		#else
		    _currentSensitivity = mouseSensitivity;
			Debug.Log("Using mouse sensitivity");
		#endif
		}

		private void TrackFireballState(int playerId, bool state) {
			if (playerId != 0) return;
			_fireballActive = state;
		}
		private void OnShotMiss(int playerId) {
			if (playerId != 0) return;
			ResetSlider();
		}
		private void OnScoreAdded(int playerId, bool __, bool ___) {
			if (playerId != 0) return;
			ResetSlider();
		}

		private void ResetSlider() {
			slider.value = 0;
			_cooldown = false;
		}

		private void Update() {
			if (_cooldown) return;
			HandleInput();

			if (_isDragging)
				slider.value = Mathf.Clamp01((_endY - _startY) * _currentSensitivity);
		}

		private void HandleInput() {
			if (Input.GetMouseButtonDown(0))
				StartDrag(Input.mousePosition.y);
			else if (Input.GetMouseButton(0))
				UpdateDrag(Input.mousePosition.y);
			else if (Input.GetMouseButtonUp(0))
				EndDrag();

			if (_isDragging && _timer >= maxTime)
				EndDrag();
		}

		private void StartDrag(float y) {
			_isDragging = true;
			_startY = _endY = y;
			_timer = 0;
		}

		private void UpdateDrag(float y) {
			if (!_isDragging) return;
			_endY = Mathf.Max(y, _endY);
			_timer += Time.deltaTime;
		}

		private void EndDrag() {
			if (!_isDragging) return;
			_isDragging = false;
			_cooldown = true;
			Shoot(slider.value);
		}

		private void Shoot(float charge) {
			var direct = _directShotVelocity;
			var bank = _bankShotVelocity;

			var directMag = direct.magnitude;
			var bankMag = bank.magnitude;


			var chargePower = Mathf.Lerp(minPowerFraction, maxShotPowerMultiplier, charge);

			var currentPower = chargePower * directMag;

			var diffDirect = Mathf.Abs(currentPower - directMag) / directMag;
			var diffBank = Mathf.Abs(currentPower - bankMag) / bankMag;

			var perfectDirect = diffDirect <= perfectThreshold;
			var perfectBank = diffBank <= perfectThreshold;

			Vector3 chosenVelocity;
			var isBank = false;

			if (perfectDirect || (!perfectBank && diffDirect < diffBank)) {
				chosenVelocity = perfectDirect ? direct : direct.normalized * currentPower;
			} else {
				chosenVelocity = perfectBank ? bank : bank.normalized * currentPower;
				isBank = true;
			}


			var ball = Instantiate(ballPrefab, _shotStartPoint, Quaternion.identity);
			var rb = ball.GetComponent<Rigidbody>();
			ball.transform.GetChild(0).tag = "PlayerBall";
			var ballController = ball.GetComponent<BallController>();
			ballController.bankShot = isBank;
			ballController.perfectShot = perfectDirect;
			ballController.fireballActive = _fireballActive;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.AddForce(chosenVelocity * rb.mass, ForceMode.Impulse);
			gameEvents.OnBallThrown.Invoke(0, ball);

			var type = perfectDirect ? "PERFETTO Diretto" :
			perfectBank ? "PERFETTO Tabellone" :
			isBank ? "Bank approssimato" : "Diretto approssimato";

			Debug.Log($"Tiro: {type} (slider={charge:F2}, power={chargePower:F2})");
		}

		public void RecalculateTrajectories() {
			_gm = GameManager.Instance;
			_shotStartPoint = transform.position + new Vector3(0, shotStartHeight, 0);

			if (!ShotCalculator.CalculateDirectShot(_shotStartPoint, _gm.hoop.position, _gm.maxHeight, out _directShotVelocity))
				Debug.LogWarning("Couldn't calculate direct shot!");

			if (!ShotCalculator.CalculateBankShot(_shotStartPoint, _gm.hoop.position, _gm.backboard, _gm.maxHeight,
			    out _bankShotVelocity))
				Debug.LogWarning("Couldn't calculate bank shot!");

			var directValue = (1f - minPowerFraction) / (maxShotPowerMultiplier - minPowerFraction);
			var bankValue = (_bankShotVelocity.magnitude / _directShotVelocity.magnitude - minPowerFraction) / (maxShotPowerMultiplier - minPowerFraction);

			gameEvents.OnPerfectZonesChanged.Invoke(directValue, bankValue, perfectThreshold);
		}
	}
}
