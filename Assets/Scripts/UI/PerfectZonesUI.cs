using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class PerfectZonesUI : MonoBehaviour {
		[Header("References")]
		[SerializeField] private Slider slider;
		[SerializeField] private Image directZone;
		[SerializeField] private Image bankZone;
		[SerializeField] private GameEvents gameEvents;

		private RectTransform _sliderRect;
		

		private void OnEnable() {
			gameEvents.OnPerfectZonesChanged.AddListener(UpdateZones);
		}
		private void OnDisable() {
			gameEvents.OnPerfectZonesChanged.RemoveListener(UpdateZones);
		}
		private void Awake() {
			_sliderRect = slider.GetComponent<RectTransform>();
		}

		private void UpdateZones(float directValue, float bankValue, float threshold) {
			if (!_sliderRect) _sliderRect = slider.GetComponent<RectTransform>();


			UpdateZone(directZone.rectTransform, directValue, threshold);
			UpdateZone(bankZone.rectTransform, bankValue, threshold);
		}

		private void UpdateZone(RectTransform zone, float center, float threshold) {
			if (!zone || _sliderRect == null) return;

			var start = Mathf.Clamp01(center - threshold);
			var end = Mathf.Clamp01(center + threshold);

			var zoneHeight = (end - start) * _sliderRect.rect.height;

			var zoneCenter = (start + end) * 0.5f * _sliderRect.rect.height;

			zone.sizeDelta = new(zone.sizeDelta.x, zoneHeight);
			zone.anchoredPosition = new(0, zoneCenter - _sliderRect.rect.height * 0.5f);
		}

	}
}
