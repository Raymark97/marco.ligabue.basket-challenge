using Core;
using System.Collections;
using UnityEngine;

namespace Camera {
    public class CameraFollowShot : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;
        [SerializeField] private Transform hoop;          // Canestro o tabellone
        [SerializeField] private Transform playerCamPos;  // Posizione base della camera

        [Header("Camera Settings")]
        [SerializeField] private float zoomDistance = 3f;
        [SerializeField] private float followSmooth = 2f;
        [SerializeField] private float zoomFOV = 40f;
        [SerializeField] private float baseHeightOffset = 1.5f;
        [SerializeField] private float returnDelay = 0.8f;

        private UnityEngine.Camera _cam;
        private float _baseFOV;
        private Coroutine _routine;
        private Transform _ball;

        private void Awake() {
            _cam = GetComponentInChildren<UnityEngine.Camera>();
            _baseFOV = _cam.fieldOfView;
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
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(FollowRoutine(ball.transform));
        }

        private void OnShotEnded(int _) => ReturnToPlayer();
        private void OnShotEnded(int _, bool __, bool ___) => ReturnToPlayer();

        private IEnumerator FollowRoutine(Transform ball) {
            _ball = ball;
            
            var zoomPos = hoop.position - hoop.forward * zoomDistance + Vector3.up * baseHeightOffset;
            
            var t = 0f;
            var startFOV = _cam.fieldOfView;
            while (t < 1f) {
                t += Time.deltaTime * 1.5f;
                _cam.fieldOfView = Mathf.Lerp(startFOV, zoomFOV, t);
                transform.position = Vector3.Lerp(transform.position, zoomPos, t * 0.8f);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(hoop.position - transform.position), t * 0.8f);
                yield return null;
            }

            
            var previousY = _ball.position.y;
            while (_ball != null) {
                var lookTarget = Vector3.Lerp(_ball.position, hoop.position, 0.7f);

                
                if (_ball.position.y < previousY - 0.05f)
                    break;
                previousY = _ball.position.y;

                transform.position = Vector3.Lerp(transform.position, zoomPos, Time.deltaTime * followSmooth);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime * followSmooth);

                yield return null;
            }

            // Breve pausa sul tabellone
            yield return new WaitForSeconds(returnDelay);

            ReturnToPlayer();
        }

        private void ReturnToPlayer() {
            if (_routine != null)
                StopCoroutine(_routine);
            transform.position = playerCamPos.position;
            transform.rotation = playerCamPos.rotation;
            _cam.fieldOfView = _baseFOV;
        }
    }
}
