using Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gameplay {
	/// <summary>
	/// Handles player input, power charging, and ball shooting.
	/// Calculates trajectories using <see cref="ShotCalculator"/> and triggers
	/// gameplay events through <see cref="GameEvents"/>.
	/// </summary>
	public class PlayerController : MonoBehaviour, IShotController {
		
		[Header("References")]
		[SerializeField] private Slider slider;
		[SerializeField] private GameEvents gameEvents;
		
		[Header("Input")]
		[SerializeField, Tooltip("Sensitivity for mouse input (desktop).")]
		private float mouseSensitivity = 0.1f;

		[SerializeField, Tooltip("Sensitivity for touch input (mobile).")]
		private float touchSensitivity = 0.1f;

		[SerializeField, Tooltip("Maximum charge duration before auto-releasing the shot (in seconds).")]
		private float maxTime = 1f;

		[Header("Gameplay")]
		[SerializeField] private GameObject ballPrefab;
		
		[SerializeField, Range(0f, 0.3f), Tooltip("Allowed deviation for a shot to be considered perfect.")]
		private float perfectThreshold = 0.1f;

		[SerializeField, Tooltip("Maximum multiplier applied to the shot power.")]
		private float maxShotPowerMultiplier = 1.2f;

		[SerializeField, Tooltip("Minimum fraction of the ideal shot power.")]
		private float minPowerFraction = 0.3f;

		[SerializeField, Tooltip("Vertical offset from the player position where the shot starts.")]
		private float shotStartHeight = 1.8f;

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
		private float directValue;
		private float bankValue;

		/// <summary>
		/// Initializes references, subscribes to gameplay events, and sets sensitivity
		/// based on the current platform (mouse vs touch).
		/// </summary>
		private void Start() {
			_gm = GameManager.Instance;
			gameEvents.OnFireStateChanged.AddListener(TrackFireballState);
			gameEvents.OnBasketMade.AddListener(OnScoreAdded);
			gameEvents.OnShotMiss.AddListener(OnShotMiss);

		#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			_currentSensitivity = touchSensitivity;
			Debug.Log("Using touch sensitivity");
		#else
			_currentSensitivity = mouseSensitivity;
			Debug.Log("Using mouse sensitivity");
		#endif
		}
		
		/// <summary>
		/// Handles charge updates and user input every frame.
		/// </summary>
		private void Update() {
			if (_cooldown) return;
			HandleInput();

			if (_isDragging)
				slider.value = Mathf.Clamp01((_endY - _startY) * _currentSensitivity);
		}

		#region Event Subscriptions
		/// <summary>
		/// Tracks whether the player’s ball is currently in fireball mode.
		/// </summary>
		private void TrackFireballState(int playerId, bool state) {
			if (playerId != 0) return;
			_fireballActive = state;
		}

		/// <summary>
		/// Resets slider and cooldown when the player misses a shot.
		/// </summary>
		private void OnShotMiss(int playerId) {
			if (playerId != 0) return;
			ResetSlider();
		}

		/// <summary>
		/// Resets slider and cooldown when the player scores a basket.
		/// </summary>
		private void OnScoreAdded(int playerId, bool __, bool ___) {
			if (playerId != 0) return;
			ResetSlider();
		}
		#endregion

		#region Input Handling
		/// <summary>
		/// Clears slider progress and allows the next charge.
		/// </summary>
		private void ResetSlider() {
			slider.value = 0;
			_cooldown = false;
		}
		
		/// <summary>
		/// Detects drag start, update, and release depending on input state.
		/// </summary>
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

		/// <summary>
		/// Called when the player starts dragging to charge a shot.
		/// </summary>
		private void StartDrag(float y) {
			_isDragging = true;
			_startY = _endY = y;
			_timer = 0;
		}

		/// <summary>
		/// Updates drag distance and accumulates charge time.
		/// </summary>
		private void UpdateDrag(float y) {
			if (!_isDragging) return;
			_endY = Mathf.Max(y, _endY);
			_timer += Time.deltaTime;
		}

		/// <summary>
		/// Ends the charge process and triggers the shot.
		/// </summary>
		private void EndDrag() {
			if (!_isDragging) return;
			_isDragging = false;
			_cooldown = true;
			Shoot(slider.value);
		}
		#endregion


		#region Shooting Logic
		/// <summary>
		/// Instantiates the ball and applies calculated shot force.
		/// Determines whether the shot is direct, banked, or perfect
		/// based on precomputed trajectory values.
		/// </summary>
		private void Shoot(float charge) {
			var direct = _directShotVelocity;
			var bank = _bankShotVelocity;

			var diffDirect = Mathf.Abs(charge - directValue);
			var diffBank = Mathf.Abs(charge - bankValue);
			
			var perfectDirect = diffDirect <= perfectThreshold;
			var perfectBank = diffBank <= perfectThreshold;
			
			var chargePower = Mathf.Lerp(minPowerFraction, maxShotPowerMultiplier, charge);
			var currentPower = chargePower * direct.magnitude;

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

		/// <summary>
		/// Recomputes both direct and bank trajectories using <see cref="ShotCalculator"/>.
		/// Invokes UI updates for perfect zone visualization.
		/// </summary>
		public void RecalculateTrajectories() {
			_gm = GameManager.Instance;
			_shotStartPoint = transform.position + new Vector3(0, shotStartHeight, 0);

			if (!ShotCalculator.CalculateDirectShot(_shotStartPoint, _gm.hoop.position, _gm.maxHeight, out _directShotVelocity))
				Debug.LogWarning("Couldn't calculate direct shot!");

			if (!ShotCalculator.CalculateBankShot(_shotStartPoint, _gm.hoop.position, _gm.backboard, _gm.maxHeight, out _bankShotVelocity))
				Debug.LogWarning("Couldn't calculate bank shot!");

			directValue = (1f - minPowerFraction) / (maxShotPowerMultiplier - minPowerFraction);
			bankValue = (_bankShotVelocity.magnitude / _directShotVelocity.magnitude - minPowerFraction) / (maxShotPowerMultiplier - minPowerFraction);

			Debug.Log($"Values: {directValue} - {bankValue}");
			gameEvents.OnPerfectZonesChanged.Invoke(directValue, bankValue, perfectThreshold);
		}
		#endregion
	}
}
