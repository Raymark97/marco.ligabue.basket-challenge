using Core;
using Gameplay;
using UnityEngine;

namespace Systems {
    public class ShotCompetitionManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;
        [SerializeField] private Transform[] shotPositions;      // posizioni da cui si tira
        [SerializeField] private PlayerController player;        // player controllato
        [SerializeField] private NPCController npc;              // NPC automatico
        [SerializeField] private Transform hoop;                 // canestro
        [SerializeField] private Transform playerCamPos;         // posizione base della camera

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
            MoveAndFaceHoop(player.transform, _playerIndex);
            MoveAndFaceHoop(npc.transform, _npcIndex);
            UpdateCameraToPlayerPosition();
        }

        private void OnScoreAdded(int playerId, bool perfect, bool bankShot) {
            if (playerId == 0 && !_playerFinished) {
                Advance(ref _playerIndex, player.transform, ref _playerFinished);
            } else if (playerId == 1 && !_npcFinished) {
                Advance(ref _npcIndex, npc.transform, ref _npcFinished);
            }
        }

        private void OnShotMiss(int playerId) {
            // Se sbaglia, resta nella stessa posizione
        }

        private void Advance(ref int index, Transform controller, ref bool finishedFlag) {
            index++;
            if (index >= shotPositions.Length) {
                finishedFlag = true;
                gameEvents.OnPlayerFinished.Invoke(controller == player.transform ? 0 : 1);
                return;
            }

            MoveAndFaceHoop(controller, index);

            if (controller == player.transform)
                UpdateCameraToPlayerPosition();
        }

        private void MoveAndFaceHoop(Transform controller, int index) {
            if (index < 0 || index >= shotPositions.Length) return;

            var target = shotPositions[index];
            controller.position = target.position;

            var dir = hoop.position - controller.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                controller.rotation = Quaternion.LookRotation(dir);
        }

        private void UpdateCameraToPlayerPosition() {
            if (!playerCamPos || !player) return;

            var pos = player.transform.position;
            var back = -player.transform.forward;

            playerCamPos.position = pos + back * 3f + Vector3.up * 1.5f;
            playerCamPos.rotation = Quaternion.LookRotation(player.transform.forward, Vector3.up);
        }
    }
}
