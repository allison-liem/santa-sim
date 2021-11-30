using System.Collections.Generic;
using UnityEngine;

namespace sound
{
    public class MusicPlayer : AudioBase
    {
        public static readonly string MUSIC_VOLUME_KEY = "music_volume";
        public static readonly string MUSIC_DISABLED_KEY = "music_disabled";

        [field: SerializeField]
        private AudioClip[] mainMenuMusic;
        [field: SerializeField]
        private AudioClip[] indoorMusic;
        [field: SerializeField]
        private AudioClip[] mapViewMusic;
        [field: SerializeField]
        private AudioClip[] metricsMusic;
        [field: SerializeField]
        private AudioClip[] lastDayIndoorMusic;
        [field: SerializeField]
        private AudioClip[] lastDayMapViewMusic;
        [field: SerializeField]
        private AudioClip[] lastDayMetricsMusic;

        [field: SerializeField]
        private map.HybridTransform.ViewType viewType = map.HybridTransform.ViewType.MainMenu;

        private game.GameState gameState;
        private List<AudioClip> mainMenuMusicRemaining;
        private List<AudioClip> indoorMusicRemaining;
        private List<AudioClip> mapViewMusicRemaining;
        private List<AudioClip> metricsMusicRemaining;

        protected new void Start()
        {
            volumeKey = MUSIC_VOLUME_KEY;
            disabledKey = MUSIC_DISABLED_KEY;
            base.Start();

            gameState = FindObjectOfType<game.GameStateBehavior>().gameState;

            mainMenuMusicRemaining = new List<AudioClip>(mainMenuMusic);
            indoorMusicRemaining = new List<AudioClip>(indoorMusic);
            mapViewMusicRemaining = new List<AudioClip>(mapViewMusic);
            metricsMusicRemaining = new List<AudioClip>(metricsMusic);
        }

        void Update()
        {
            if (!audioSource.isPlaying)
            {
                bool lastDay = IsLastDay();

                switch (viewType)
                {
                    case map.HybridTransform.ViewType.MainMenu:
                        PlayMusic(mainMenuMusicRemaining, mainMenuMusic);
                        break;
                    case map.HybridTransform.ViewType.Indoors:
                        PlayMusic(indoorMusicRemaining, lastDay ? lastDayIndoorMusic : indoorMusic);
                        break;
                    case map.HybridTransform.ViewType.PlaybackMap:
                    case map.HybridTransform.ViewType.PlaybackIndoors:
                        PlayMusic(mapViewMusicRemaining, lastDay ? lastDayMapViewMusic : mapViewMusic);
                        break;
                    case map.HybridTransform.ViewType.Metrics:
                        PlayMusic(metricsMusicRemaining, lastDay ? lastDayMetricsMusic : metricsMusic);
                        break;
                }
            }
        }

        public void SetViewType(map.HybridTransform.ViewType viewType)
        {
            if (viewType != this.viewType)
            {
                this.viewType = viewType;
                audioSource.Stop();
            }
        }

        private void PlayMusic(List<AudioClip> musicRemaining, AudioClip[] music)
        {
            if (musicRemaining.Count <= 0)
            {
                musicRemaining.AddRange(music);
            }

            int index = Random.Range(0, musicRemaining.Count);
            audioSource.clip = musicRemaining[index];
            audioSource.Play();
            musicRemaining.RemoveAt(index);
        }

        private bool IsLastDay()
        {
            return logic.sim.TimeUtils.Shorter(logic.sim.TimeUtils.Elapsed(gameState.currentTime, game.GameState.END_TIME), game.GameState.ONE_DAY);
        }
    }
}
