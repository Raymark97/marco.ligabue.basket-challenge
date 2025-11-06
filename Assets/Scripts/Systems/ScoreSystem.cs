using Core;
using System.Collections;
using UnityEngine;

namespace Systems {
    /// <summary>
    /// Manages player and NPC scoring, including perfect shot bonuses,
    /// backboard multipliers, and the temporary fireball state.
    /// </summary>
    public class ScoreSystem : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;

        [Header("Scoring")]
        [SerializeField] private int normalPoints = 2;
        [SerializeField] private int perfectPoints = 3;
        [SerializeField] private int backboardBonus = 1;

        [Header("Fireball Settings")]
        [SerializeField] private float chargesToFillFire = 10f;
        [SerializeField] private float chargesPerShot = 1f;
        [SerializeField] private float fireDuration = 5f;
        [SerializeField] private float fireDecayRate = 1f;

        private int _playerPoints, _npcPoints;
        private float _fireChargePlayer, _fireChargeNPC;
        private bool _fireActivePlayer, _fireActiveNPC;
        private Coroutine _fireCoroutinePlayer, _fireCoroutineNPC;

        private void OnEnable() {
            gameEvents.OnScoreAdded.AddListener(AddPoints);
            gameEvents.OnBackboardBonusUpdated.AddListener(SetBackboardBonus);
            gameEvents.OnShotMiss.AddListener(OnMiss);
            gameEvents.OnMatchEnded.AddListener(OnMatchEnded);
        }

        private void OnDisable() {
            gameEvents.OnScoreAdded.RemoveListener(AddPoints);
            gameEvents.OnBackboardBonusUpdated.RemoveListener(SetBackboardBonus);
            gameEvents.OnShotMiss.RemoveListener(OnMiss);
            gameEvents.OnMatchEnded.RemoveListener(OnMatchEnded);
        }

        /// <summary>
        /// Updates the active backboard bonus multiplier value.
        /// </summary>
        private void SetBackboardBonus(int bonus) {
            backboardBonus = bonus;
        }

        /// <summary>
        /// Adds score points for the specified player, considering perfect shots,
        /// backboard bonuses, and fireball state.
        /// </summary>
        /// <param name="playerId">0 for Player, 1 for NPC.</param>
        /// <param name="perfect">Whether the shot was perfect.</param>
        /// <param name="backShot">Whether the shot hit the backboard.</param>
        public void AddPoints(int playerId, bool perfect, bool backShot) {
            var points = perfect ? perfectPoints : normalPoints;
            if (backShot) points += backboardBonus;

            AddFireCharge(playerId, perfect ? chargesPerShot * 2 : chargesPerShot);

            if (IsFireActive(playerId))
                points *= 2;

            if (playerId == 0) _playerPoints += points;
            else _npcPoints += points;

            var newScore = playerId == 0 ? _playerPoints : _npcPoints;
            gameEvents.OnScoreUpdated.Invoke(playerId, newScore);
        }

        /// <summary>
        /// Called when a shot misses; ends fire mode for the specified player.
        /// </summary>
        private void OnMiss(int playerId) {
            EndFireMode(playerId);
        }

        /// <summary>
        /// Ends both players' fire modes at the end of the match.
        /// </summary>
        private void OnMatchEnded() {
            EndFireMode(0);
            EndFireMode(1);
        }
        
        /// <summary>
        /// Increases the player's fire charge and triggers fire mode when full.
        /// </summary>
        private void AddFireCharge(int playerId, float charges) {
            ref var charge = ref playerId == 0 ? ref _fireChargePlayer : ref _fireChargeNPC;
            ref var active = ref playerId == 0 ? ref _fireActivePlayer : ref _fireActiveNPC;

            if (active) return;

            charge += charges;
            var normalized = Mathf.Clamp01(charge / chargesToFillFire);
            gameEvents.OnFireChargeChanged.Invoke(playerId, normalized);

            if (charge >= chargesToFillFire) {
                active = true;
                gameEvents.OnFireStateChanged.Invoke(playerId, true);
                ref var routine = ref playerId == 0 ? ref _fireCoroutinePlayer : ref _fireCoroutineNPC;
                routine = StartCoroutine(FireModeRoutine(playerId));
            }
        }

        /// <summary>
        /// Coroutine that handles fire mode duration and gradual decay.
        /// </summary>
        private IEnumerator FireModeRoutine(int playerId) {
            var charge = 1f;
            gameEvents.OnFireChargeChanged.Invoke(playerId, charge);

            while (charge > 0f) {
                charge -= Time.deltaTime / fireDuration * fireDecayRate;
                gameEvents.OnFireChargeChanged.Invoke(playerId, Mathf.Clamp01(charge));
                yield return null;
            }

            EndFireMode(playerId);
        }

        /// <summary>
        /// Terminates fire mode, resets charge, and emits corresponding events.
        /// </summary>
        private void EndFireMode(int playerId) {
            ref var active = ref playerId == 0 ? ref _fireActivePlayer : ref _fireActiveNPC;
            ref var charge = ref playerId == 0 ? ref _fireChargePlayer : ref _fireChargeNPC;
            ref var routine = ref playerId == 0 ? ref _fireCoroutinePlayer : ref _fireCoroutineNPC;

            if (routine != null) StopCoroutine(routine);
            active = false;
            charge = 0f;
            gameEvents.OnFireStateChanged.Invoke(playerId, false);
            gameEvents.OnFireChargeChanged.Invoke(playerId, 0f);
        }

        /// <summary>
        /// Returns whether the specified player currently has an active fire mode.
        /// </summary>
        private bool IsFireActive(int playerId) {
            return playerId == 0 ? _fireActivePlayer : _fireActiveNPC;
        }
    }
}
