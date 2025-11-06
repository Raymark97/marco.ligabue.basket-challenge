using Core;
using System.Collections;
using UnityEngine;

namespace Camera {
	public class CameraFollowShot : MonoBehaviour {
		[Header("References")] [SerializeField]
		private GameEvents gameEvents;

		[SerializeField] private Transform hoop;

		[Header("Camera Settings")] [SerializeField]
		private float zoomDistance = 3f;

		[SerializeField] private float followSmooth = 2f;
		[SerializeField] private float zoomFOV = 40f;
		[SerializeField] private float baseHeightOffset = 1.5f;
		[SerializeField] private float returnDelay = 0.8f;
		[SerializeField] private float startDelay = 0.5f;

		private UnityEngine.Camera _cam;
		private float _baseFOV;
		private Coroutine _routine;
		private Transform _ball;
		public float maxFollowDistance = 12f;
		public float heightOffset = .3f;
		public float zoomSmooth = 3;
		public float followDistance = 2.5f;
		public float rotationSmooth = 6;


		private void Awake() {
			_cam = GetComponentInChildren<UnityEngine.Camera>();
			_baseFOV = _cam.fieldOfView;
		}

		private void Start() {
			ReturnToPlayer();
		}

		private void OnEnable() {
			gameEvents.OnBallThrown.AddListener(OnBallThrown);
			gameEvents.OnScoreAdded.AddListener(OnShotEnded);
			gameEvents.OnShotMiss.AddListener(OnShotEnded);
		}

		private void OnDisable() {
			gameEvents.OnBallThrown.RemoveListener(OnBallThrown);
			gameEvents.OnScoreAdded.RemoveListener(OnShotEnded);
			gameEvents.OnShotMiss.RemoveListener(OnShotEnded);
		}

		private void OnBallThrown(int playerId, GameObject ball) {
			if (playerId != 0) return;
			if (_routine != null)
				StopCoroutine(_routine);

			_routine = StartCoroutine(FollowRoutine(ball.transform));
		}

		private void OnShotEnded(int playerId) {
			if (playerId != 0) return;
			ReturnToPlayer();
		}
		private void OnShotEnded(int playerId, bool __, bool ___) {
			if (playerId != 0) return;
			ReturnToPlayer();
		}

		private IEnumerator FollowRoutine(Transform ball) {
			_ball = ball;
			var rb = _ball.GetComponent<Rigidbody>();
			var startFOV = _cam.fieldOfView;

			var hoopHeight = hoop.position.y;

			while (_ball) {
				var distToHoop = Vector3.Distance(_ball.position, hoop.position);

				var targetFOV = Mathf.Lerp(zoomFOV, startFOV, distToHoop / maxFollowDistance);
				_cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * zoomSmooth);

				var followOffset = -rb.velocity.normalized * followDistance;
				var targetPos = _ball.position + followOffset;
				targetPos.y = Mathf.Lerp(targetPos.y, hoopHeight + heightOffset, 0.7f);

				_cam.transform.position = Vector3.Lerp(
					_cam.transform.position,
					targetPos,
					Time.deltaTime * followSmooth
				);

				var lookTarget = Vector3.Lerp(_ball.position, hoop.position, 0.6f);
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


		private void ReturnToPlayer() {
			if (_routine != null)
				StopCoroutine(_routine);

			_cam.transform.position = transform.position;
			_cam.transform.rotation = transform.rotation;
			_cam.fieldOfView = _baseFOV;
		}
	}
}
