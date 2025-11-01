using Core;
using System.Collections;
using UnityEngine;

namespace Camera {
	public class CameraFollowShot : MonoBehaviour {
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private Transform hoop;

		[Header("Camera Settings")]
		[SerializeField] private float zoomDistance = 3f;
		[SerializeField] private float followSmooth = 2f;
		[SerializeField] private float zoomFOV = 40f;
		[SerializeField] private float baseHeightOffset = 1.5f;
		[SerializeField] private float returnDelay = 0.8f;
		[SerializeField] private float startDelay = 0.5f;

		private UnityEngine.Camera _cam;
		private float _baseFOV;
		private Coroutine _routine;
		private Transform _ball;

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

		private void OnShotEnded(int playerId) => ReturnToPlayer(playerId);
		private void OnShotEnded(int playerId, bool __, bool ___) => ReturnToPlayer();

		private IEnumerator FollowRoutine(Transform ball) {
			yield return new WaitForSeconds(startDelay);
			_ball = ball;

			var zoomPos = hoop.position - hoop.forward * zoomDistance + Vector3.up * baseHeightOffset;

			var t = 0f;
			var startFOV = _cam.fieldOfView;
			while (t < 1f) {
				t += Time.deltaTime * 1f;
				_cam.fieldOfView = Mathf.Lerp(startFOV, zoomFOV, t);
				_cam.transform.position = Vector3.Lerp(_cam.transform.position, zoomPos, t * 0.8f);
				_cam.transform.rotation = Quaternion.Slerp(_cam.transform.rotation,
				Quaternion.LookRotation(hoop.position - _cam.transform.position), t * 0.8f);
				yield return null;
			}


			var previousY = _ball.position.y;
			while (_ball) {
				var lookTarget = Vector3.Lerp(_ball.position, hoop.position, 0.7f);


				if (_ball.position.y < previousY - 0.05f)
					break;
				previousY = _ball.position.y;

				_cam.transform.position = Vector3.Lerp(_cam.transform.position, zoomPos, Time.deltaTime * followSmooth);
				_cam.transform.rotation = Quaternion.Slerp(_cam.transform.rotation,
				Quaternion.LookRotation(lookTarget - _cam.transform.position), Time.deltaTime * followSmooth);

				yield return null;
			}
			yield return new WaitForSeconds(returnDelay);

			ReturnToPlayer();
		}

		private void ReturnToPlayer(int playerId = 0) {
			if(playerId != 0) return;
			if (_routine != null)
				StopCoroutine(_routine);
			_cam.transform.position = transform.position;
			_cam.transform.rotation = transform.rotation;
			_cam.fieldOfView = _baseFOV;
		}
	}
}
