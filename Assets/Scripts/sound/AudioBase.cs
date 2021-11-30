using UnityEngine;

namespace sound
{
    public class AudioBase : MonoBehaviour
    {
        // Subclasses should override these if necessary
        protected string volumeKey = "audio_volume";
        protected string disabledKey = "audio_disabled";

        [field: SerializeField]
        public float volume { get; private set; } = 0.6f;
        [field: SerializeField]
        public bool audioDisabled { get; private set; } = false;

        protected AudioSource audioSource { get; private set; }

        protected void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (PlayerPrefs.HasKey(volumeKey))
            {
                volume = PlayerPrefs.GetFloat(volumeKey);
            }
            SetVolume(volume, false);
            if (PlayerPrefs.HasKey(disabledKey))
            {
                audioDisabled = PlayerPrefs.GetInt(disabledKey) > 0;
            }
            SetAudioDisabled(audioDisabled);
        }

        void Update()
        {
        }

        public void SetVolume(float volume, bool saveSettings = true)
        {
            volume = Mathf.Clamp(volume, 0, 1);
            audioSource.volume = volume;

            if (saveSettings)
            {
                this.volume = volume;
                PlayerPrefs.SetFloat(volumeKey, volume);
                PlayerPrefs.Save();
                SetAudioDisabled(false);
            }
        }

        public void SetAudioDisabled(bool audioDisabled)
        {
            this.audioDisabled = audioDisabled;
            PlayerPrefs.SetInt(disabledKey, audioDisabled ? 1 : 0);
            PlayerPrefs.Save();

            if (audioDisabled)
            {
                SetVolume(0, false);
            }
            else
            {
                SetVolume(volume, false);
            }
        }
    }
}
