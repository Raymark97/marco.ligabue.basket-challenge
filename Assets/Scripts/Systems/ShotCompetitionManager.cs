using Core;
using Gameplay;
using UnityEngine;
namespace Systems {
    public class ShotCompetitionManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;
        [SerializeField] private Transform[] shotPositions;
        [SerializeField] private PlayerController player;
        [SerializeField] private NPCController npc;
        [SerializeField] private Transform hoop;
        [SerializeField] private Transform playerCamPos;

        [Header("Settings")]
        [SerializeField, Tooltip("Offset laterale dal punto base per separare player e NPC")]
        private float sideOffset = 1.5f;

        private int _playerIndex;
        private int _npcIndex;
        private bool _playerFinished;
        private bool _npcFinished;

        private void OnEnable() {
            gameEvents.OnScoreAdded.AddListener(OnScoreAdded);
            gameEvents.OnShotMiss.AddListener(OnShotMiss);
        }

        private void OnDisable() {
            gameEvents.OnScoreAdded.RemoveListener(OnScoreAdded);
            gameEvents.OnShotMiss.RemoveListener(OnShotMiss);
        }

        private void Start() {
            PositionPlayer(_playerIndex);
            PositionNPC(_npcIndex);
            player.RecalculateTrajectories();
            npc.RecalculateTrajectories();
        }

        private void OnScoreAdded(int playerId, bool perfect, bool bankShot) {
            if (playerId == 0 && !_playerFinished)
	            AdvancePlayer(ref _playerIndex, player, ref _playerFinished, true);
            else if (playerId == 1 && !_npcFinished)
	            AdvancePlayer(ref _npcIndex, npc, ref _npcFinished, false);
        }

        private void OnShotMiss(int playerId) {
            // Nessun avanzamento
        }

        private void AdvancePlayer(ref int index, IShotController controller, ref bool finishedFlag, bool isPlayer) {
	        index++;
	        if (index >= shotPositions.Length) {
		        finishedFlag = true;
		        gameEvents.OnPlayerFinished.Invoke(isPlayer ? 0 : 1);
		        return;
	        }

	        if (isPlayer)
		        PositionPlayer(index);
	        else
		        PositionNPC(index);

	        controller.RecalculateTrajectories();
        }

        private void PositionPlayer(int index) {
            if (index < 0 || index >= shotPositions.Length) return;

            var basePos = shotPositions[index];
            var forward = (hoop.position - basePos.position).normalized;
            forward.y = 0f;
            var right = Vector3.Cross(Vector3.up, forward);

            var playerPos = basePos.position - right * (sideOffset / 2f);

            player.transform.SetPositionAndRotation(playerPos, Quaternion.LookRotation(forward));
        }

        private void PositionNPC(int index) {
            if (index < 0 || index >= shotPositions.Length) return;

            var basePos = shotPositions[index];
            var forward = (hoop.position - basePos.position).normalized;
            forward.y = 0f;
            var right = Vector3.Cross(Vector3.up, forward);

            var npcPos = basePos.position + right * (sideOffset / 2f);

            npc.transform.SetPositionAndRotation(npcPos, Quaternion.LookRotation(forward));
        }
    }
}
