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

			var zoneWidth = (end - start) * _sliderRect.rect.width;
			var zoneCenter = (start + end) * 0.5f * _sliderRect.rect.width;

			zone.sizeDelta = new(zoneWidth, zone.sizeDelta.y);
			zone.anchoredPosition = new(zoneCenter - _sliderRect.rect.width * 0.5f, 0);
		}
	}
}
