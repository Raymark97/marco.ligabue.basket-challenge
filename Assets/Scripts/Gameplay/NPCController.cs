using System.Collections;
using Core;
using UnityEngine;

namespace Gameplay {
    /// <summary>
    /// Handles the autonomous shooting logic of an AI-controlled opponent.
    /// Uses the same physics and trajectory calculations as the player,
    /// but performs shots at randomized intervals and with probabilistic accuracy.
    /// </summary>
    public class NPCController : MonoBehaviour, IShotController {
        
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;
        [SerializeField] private GameObject ballPrefab;
        
        [Header("AI Behaviour")]
        [SerializeField, Tooltip("Vertical offset from the NPC position where the shot is spawned.")]
        private float shotStartHeight = 1.8f;
        
        [SerializeField, Tooltip("Delay before the NPC starts shooting after the match begins.")]
        private float initialDelay = 2f;

        [SerializeField, Range(0.5f, 3f), Tooltip("Minimum time between NPC shots.")]
        private float minShootInterval = 1.5f;

        [SerializeField, Range(0.5f, 3f), Tooltip("Maximum time between NPC shots.")]
        private float maxShootInterval = 2.5f;

        [SerializeField, Range(0f, 1f), Tooltip("Probability for the NPC to perform a perfect shot.")]
        private float perfectChance = 0.3f;

        [SerializeField, Range(0f, 1f), Tooltip("Probability for the NPC to attempt a bank shot.")]
        private float bankChance = 0.4f;

        [SerializeField, Tooltip("Multiplier applied to bank shot probability when backboard bonus is active.")]
        private float bankBonusMultiplier = 1.5f;

        [SerializeField, Range(0.8f, 1.2f), Tooltip("Amount of random variation applied to shot power for realism.")]
        private float accuracyNoise = 0.05f;
        
        private Vector3 _directShotVelocity;
        private Vector3 _bankShotVelocity;
        private Vector3 _shotStartPoint;
        private bool _backboardBonusActive;
        private GameManager _gm;
        private Coroutine _shootRoutine;
        private bool _isActive;


        /// <summary>
        /// Initializes references, subscribes to game events, and starts the NPC shooting loop.
        /// </summary>
        private void Start() {
            _gm = GameManager.Instance;
            StartShooting();
            gameEvents.OnBackboardBonusUpdated.AddListener(UpdateBackboardBonusState);
        }
        /// <summary>
        /// Updates internal state when the backboard bonus changes.
        /// Increases the probability of bank shots when the bonus is active.
        /// </summary>
        private void UpdateBackboardBonusState(int bonus) {
            _backboardBonusActive = bonus > 0;
        }


        #region Shooting Logic
        /// <summary>
        /// Starts the NPC shooting coroutine.
        /// </summary>
        public void StartShooting() {
            if (_shootRoutine != null) StopCoroutine(_shootRoutine);
            _shootRoutine = StartCoroutine(ShootLoop());
        }

        /// <summary>
        /// Stops the NPC shooting coroutine.
        /// </summary>
        public void StopShooting() {
            if (_shootRoutine != null) StopCoroutine(_shootRoutine);
            _shootRoutine = null;
        }

        /// <summary>
        /// Coroutine that controls NPC shooting timing and frequency.
        /// </summary>
        private IEnumerator ShootLoop() {
            yield return new WaitForSeconds(initialDelay);
            _isActive = true;
            while (_isActive) {
                yield return new WaitForSeconds(Random.Range(minShootInterval, maxShootInterval));
                Shoot();
            }
        }

        /// <summary>
        /// Recalculates both direct and bank trajectories using the shared <see cref="ShotCalculator"/>.
        /// </summary>
        public void RecalculateTrajectories() {
            _gm = GameManager.Instance;
            _shotStartPoint = transform.position + new Vector3(0, shotStartHeight, 0);

            if (!ShotCalculator.CalculateDirectShot(_shotStartPoint, _gm.hoop.position, _gm.maxHeight, out _directShotVelocity))
                Debug.LogWarning("NPC couldn't calculate direct shot!");
            if (!ShotCalculator.CalculateBankShot(_shotStartPoint, _gm.hoop.position, _gm.backboard, _gm.maxHeight, out _bankShotVelocity))
                Debug.LogWarning("NPC couldn't calculate bank shot!");
        }

        /// <summary>
        /// Performs a single NPC shot, selecting between direct and bank trajectories
        /// based on probabilities and current backboard bonus.
        /// </summary>
        private void Shoot() {
            // Adjust bank shot probability if backboard bonus is active
            var actualBankChance = _backboardBonusActive
                ? Mathf.Clamp01(bankChance * bankBonusMultiplier)
                : bankChance;

            // Decide shot type
            var useBank = Random.value < actualBankChance;
            var isPerfect = Random.value < perfectChance;

            // Select base velocity and apply noise if not perfect
            var idealVel = useBank ? _bankShotVelocity : _directShotVelocity;
            var powerFactor = isPerfect
                ? 1f
                : Random.Range(1f - accuracyNoise, 1f + accuracyNoise);

            var shotVel = idealVel.normalized * (idealVel.magnitude * powerFactor);

            // Instantiate ball
            var ball = Instantiate(ballPrefab, _shotStartPoint, Quaternion.identity);
            var rb = ball.GetComponent<Rigidbody>();
            ball.transform.GetChild(0).tag = "NPCBall";
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(shotVel * rb.mass, ForceMode.Impulse);
            
            gameEvents.OnBallThrown.Invoke(1, ball);
            Debug.Log($"NPC shot ({(useBank ? "Bank" : "Direct")}, {(isPerfect ? "Perfect" : "Approx")})");
        }
        #endregion
    }
}
