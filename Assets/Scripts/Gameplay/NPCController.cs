using System.Collections;
using Core;
using UnityEngine;

namespace Gameplay {
    public class NPCController : MonoBehaviour, IShotController {
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;
        [SerializeField] private GameObject ballPrefab;

        [Header("Gameplay")]
        [SerializeField] private float shotStartHeight = 1.8f;

        [Header("AI Behaviour")]
        [SerializeField] private float initialDelay = 2f;
        [SerializeField, Range(0.5f, 3f)] private float minShootInterval = 1.5f;
        [SerializeField, Range(0.5f, 3f)] private float maxShootInterval = 2.5f;
        [SerializeField, Range(0f, 1f), Tooltip("Probability to hit a perfect shot")] private float perfectChance = 0.3f;
        [SerializeField, Range(0f, 1f), Tooltip("Probability to hit a bank shot")] private float bankChance = 0.4f;
        [SerializeField, Tooltip("Multiplier for bank shot chance when backboard bonus is active")] private float bankBonusMultiplier = 1.5f;
        [SerializeField, Range(0.8f, 1.2f), Tooltip("Random variation of power")] private float accuracyNoise = 0.05f;

        private Vector3 _directShotVelocity;
        private Vector3 _bankShotVelocity;
        private Vector3 _shotStartPoint;
        private bool _backboardBonusActive;
        private GameManager _gm;
        private Coroutine _shootRoutine;
        private bool _isActive;

        private void Start() {
            _gm = GameManager.Instance;
            StartShooting();
            gameEvents.OnBackboardBonusUpdated.AddListener(UpdateBackboardBonusState);
        }
        private void UpdateBackboardBonusState(int bonus) {
            _backboardBonusActive = bonus > 0;
        }

        public void StartShooting() {
            if (_shootRoutine != null) StopCoroutine(_shootRoutine);
            _shootRoutine = StartCoroutine(ShootLoop());
        }

        public void StopShooting() {
            if (_shootRoutine != null) StopCoroutine(_shootRoutine);
            _shootRoutine = null;
        }

        private IEnumerator ShootLoop() {
            yield return new WaitForSeconds(initialDelay);
            _isActive = true;
            while (_isActive) {
                yield return new WaitForSeconds(Random.Range(minShootInterval, maxShootInterval));
                Shoot();
            }
        }

        public void RecalculateTrajectories() {
            _gm = GameManager.Instance;
            _shotStartPoint = transform.position + new Vector3(0, shotStartHeight, 0);

            if (!ShotCalculator.CalculateDirectShot(_shotStartPoint, _gm.hoop.position, _gm.maxHeight, out _directShotVelocity))
                Debug.LogWarning("NPC couldn't calculate direct shot!");
            if (!ShotCalculator.CalculateBankShot(_shotStartPoint, _gm.hoop.position, _gm.backboard, _gm.maxHeight, out _bankShotVelocity))
                Debug.LogWarning("NPC couldn't calculate bank shot!");
        }

        private void Shoot() {
            var actualBankChance = _backboardBonusActive
                ? Mathf.Clamp01(bankChance * bankBonusMultiplier)
                : bankChance;
            var useBank = Random.value < actualBankChance;
            var isPerfect = Random.value < perfectChance;

            var idealVel = useBank ? _bankShotVelocity : _directShotVelocity;
            var powerFactor = isPerfect
                ? 1f
                : Random.Range(1f - accuracyNoise, 1f + accuracyNoise);

            var shotVel = idealVel.normalized * (idealVel.magnitude * powerFactor);

            var ball = Instantiate(ballPrefab, _shotStartPoint, Quaternion.identity);
            var rb = ball.GetComponent<Rigidbody>();
            ball.transform.GetChild(0).tag = "NPCBall";
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(shotVel * rb.mass, ForceMode.Impulse);

            gameEvents.OnBallThrown.Invoke(1, ball);
            Debug.Log($"NPC shot ({(useBank ? "Bank" : "Direct")}, {(isPerfect ? "Perfect" : "Approx")})");
        }
    }
}
