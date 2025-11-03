using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio {
	[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
	public class AudioLibrary : ScriptableObject {
		[SerializeField] private List<AudioEntry> entries = new();

		private Dictionary<string, AudioClip> lookup;

		private void OnEnable() {
			lookup = new();
			foreach (var e in entries.Where(e => !lookup.ContainsKey(e.key)))
				lookup.Add(e.key, e.clip);
		}

		public AudioClip GetClip(string key) {
			if (lookup == null) OnEnable();
			lookup.TryGetValue(key, out var clip);
			return clip;
		}
	}
}
