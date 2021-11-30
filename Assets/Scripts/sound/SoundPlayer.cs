using System.Collections.Generic;
using UnityEngine;

namespace sound
{
    public class SoundPlayer : AudioBase
    {
        public static readonly string SOUND_VOLUME_KEY = "sound_volume";
        public static readonly string SOUND_DISABLED_KEY = "sound_disabled";

        [System.Serializable]
        public enum SoundType
        {
            MenuOpen,
            MenuClose,
            ScreenChange,
            Refresh,
            GenericButton,
            LevelUp,
        }

        [System.Serializable]
        public class TypeAndSound
        {
            public SoundType soundType;
            public AudioClip audioClip;
        }

        [field: SerializeField]
        private TypeAndSound[] typesAndSounds;

        private Dictionary<SoundType, AudioClip> sounds;

        protected new void Start()
        {
            volumeKey = SOUND_VOLUME_KEY;
            disabledKey = SOUND_DISABLED_KEY;
            base.Start();

            // Convert the list into a dictionary
            sounds = new Dictionary<SoundType, AudioClip>();
            foreach (var typeAndSound in typesAndSounds)
            {
                sounds[typeAndSound.soundType] = typeAndSound.audioClip;
            }
        }

        void Update()
        {
        }

        public void PlaySound(SoundType soundType)
        {
            audioSource.PlayOneShot(sounds[soundType]);
        }

    }
}
