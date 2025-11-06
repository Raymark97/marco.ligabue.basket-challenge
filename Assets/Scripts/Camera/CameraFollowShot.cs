using Core;
using System.Collections;
using UnityEngine;

namespace Camera {
	/// <summary>
	/// Manages the dynamic camera behaviour during a player's shot.
	/// The camera follows the ball while it travels towards the hoop,
	/// applies a smooth zoom effect, and returns to the player afterwards.
	/// </summary>
	public class CameraFollowShot : MonoBehaviour {
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private Transform hoop;

		[Header("Camera Settings")]
		[SerializeField, Tooltip("How smoothly the camera follows the ball's position.")]
		private float followSmooth = 2f;
		[SerializeField, Tooltip("Field of View when zoomed in near the hoop.")]
		private float zoomFOV = 40f;
		[SerializeField, Tooltip("Delay before returning the camera to the player after a shot ends.")]
		private float returnDelay = 0.8f;
		[SerializeField, Tooltip("Maximum distance from the hoop used to compute zoom interpolation.")]
		private float maxFollowDistance = 12f;
		[SerializeField, Tooltip("Vertical offset applied when following the ball to stay near hoop height.")]
		private float heightOffset = 0.3f;
		[SerializeField, Tooltip("How quickly the camera adjusts its FOV while zooming.")]
		private float zoomSmooth = 3f;
		[SerializeField, Tooltip("Distance to maintain behind the ball during flight.")]
		private float followDistance = 2.5f;
		[SerializeField, Tooltip("How smoothly the camera rotates toward the target hoop.")]
		private float rotationSmooth = 6f;


		private UnityEngine.Camera _cam;
		private float _baseFOV;
		private Coroutine _routine;

		private void Awake() {
			_cam = GetComponentInChildren<UnityEngine.Camera>();
			_baseFOV = _cam.fieldOfView;
		}

		private void Start() {
			ReturnToPlayer();
		}

		/// <summary>
		/// Registers to global gameplay events.
		/// </summary>
		private void OnEnable() {
			gameEvents.OnBallThrown.AddListener(OnBallThrown);
			gameEvents.OnScoreAdded.AddListener(OnShotEnded);
			gameEvents.OnShotMiss.AddListener(OnShotEnded);
		}

		/// <summary>
		/// Unregisters from global gameplay events.
		/// </summary>
		private void OnDisable() {
			gameEvents.OnBallThrown.RemoveListener(OnBallThrown);
			gameEvents.OnScoreAdded.RemoveListener(OnShotEnded);
			gameEvents.OnShotMiss.RemoveListener(OnShotEnded);
		}

		/// <summary>
		/// Starts following the player's ball when a new shot is thrown.
		/// </summary>
		private void OnBallThrown(int playerId, GameObject ball) {
			if (playerId != 0) return;

			if (_routine != null)
				StopCoroutine(_routine);

			_routine = StartCoroutine(FollowRoutine(ball.transform));
		}

		/// <summary>
		/// Returns the camera to the player view when a shot ends (scored or missed).
		/// </summary>
		private void OnShotEnded(int playerId) {
			if (playerId != 0) return;
			ReturnToPlayer();
		}

		private void OnShotEnded(int playerId, bool __, bool ___) {
			if (playerId != 0) return;
			ReturnToPlayer();
		}

		/// <summary>
		/// Follows the ball while it is moving upwards.
		/// Adjusts position, rotation, and FOV dynamically.
		/// Stops following when the ball starts descending.
		/// </summary>
		private IEnumerator FollowRoutine(Transform ball) {
			var rb = ball.GetComponent<Rigidbody>();
			var startFOV = _cam.fieldOfView;
			var hoopHeight = hoop.position.y;

			while (ball) {
				var distToHoop = Vector3.Distance(ball.position, hoop.position);

				// Adjust zoom based on distance
				var targetFOV = Mathf.Lerp(zoomFOV, startFOV, distToHoop / maxFollowDistance);
				_cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * zoomSmooth);

				// Smooth follow motion
				var followOffset = -rb.velocity.normalized * followDistance;
				var targetPos = ball.position + followOffset;
				targetPos.y = Mathf.Lerp(targetPos.y, hoopHeight + heightOffset, 0.7f);

				_cam.transform.position = Vector3.Lerp(
				_cam.transform.position,
				targetPos,
				Time.deltaTime * followSmooth
				);

				// Smooth rotation toward hoop
				var lookTarget = Vector3.Lerp(ball.position, hoop.position, 0.6f);
				lookTarget.y = hoopHeight;
				var lookDir = lookTarget - _cam.transform.position;

				_cam.transform.rotation = Quaternion.Slerp(
				_cam.transform.rotation,
				Quaternion.LookRotation(lookDir),
				Time.deltaTime * rotationSmooth
				);

				if (rb.velocity.y < 0)
					break;

				yield return null;
			}

			yield return new WaitForSeconds(returnDelay);
			ReturnToPlayer();
		}

		/// <summary>
		/// Resets the camera to its default position, rotation, and FOV.
		/// </summary>
		private void ReturnToPlayer() {
			if (_routine != null)
				StopCoroutine(_routine);

			_cam.transform.position = transform.position;
			_cam.transform.rotation = transform.rotation;
			_cam.fieldOfView = _baseFOV;
		}
	}
}
