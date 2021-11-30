using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace view
{
    public class MapViewBehavior : MonoBehaviour
    {
        [field: SerializeField]
        public bool playing { get; private set; }

        [field: SerializeField]
        private Slider slider;
        [field: SerializeField]
        private GameObject playButton;
        [field: SerializeField]
        private GameObject pauseButton;
        [field: SerializeField]
        private Button speedUpButton;
        [field: SerializeField]
        private Button slowDownButton;
        [field: SerializeField]
        private TextMeshProUGUI speedText;
        [field: SerializeField]
        private Transform playbackObjectParent;

        [System.Serializable]
        public class SpeedAndText
        {
            public float speed;
            public string text;
        }

        [field: SerializeField]
        private int defaultSpeedIndex;
        [field: SerializeField]
        private SpeedAndText[] speeds;
        [field: SerializeField]
        private int speedIndex;

        private map.HybridTransform hybridTransform;
        private sound.SoundPlayer soundPlayer;

        void Start()
        {
            hybridTransform = FindObjectOfType<map.HybridTransform>();
            soundPlayer = FindObjectOfType<sound.SoundPlayer>();

            speedIndex = defaultSpeedIndex;

            SetSpeedText();
        }

        void Update()
        {
            if (!playing)
            {
                return;
            }

            float sliderValue = slider.value;
            sliderValue += Time.deltaTime * speeds[speedIndex].speed;
            if (sliderValue >= slider.maxValue)
            {
                SetPlaying(false);
            }
            slider.value = Mathf.Clamp(sliderValue, slider.minValue, slider.maxValue);
        }

        public void SetPlayingButton(bool playing)
        {
            soundPlayer.PlaySound(sound.SoundPlayer.SoundType.GenericButton);
            SetPlaying(playing);
        }

        public void SetPlaying(bool playing)
        {
            this.playing = playing;
            playButton.SetActive(!playing);
            pauseButton.SetActive(playing);

            for (int i = 0; i < playbackObjectParent.childCount; i++)
            {
                CreatureBehavior creatureBehavior = playbackObjectParent.GetChild(i).GetComponent<CreatureBehavior>();
                if (playing)
                {
                    creatureBehavior.AnimateWalk();
                }
                else
                {
                    creatureBehavior.AnimateIdle();
                }
            }
        }

        public void SpeedUp()
        {
            soundPlayer.PlaySound(sound.SoundPlayer.SoundType.GenericButton);
            speedIndex++;
            if (speedIndex >= speeds.Length - 1)
            {
                speedIndex = speeds.Length - 1;
                speedUpButton.interactable = false;
            }
            slowDownButton.interactable = true;
            SetSpeedText();
            SetPlaying(true);
        }

        public void SlowDown()
        {
            soundPlayer.PlaySound(sound.SoundPlayer.SoundType.GenericButton);
            speedIndex--;
            if (speedIndex <= 0)
            {
                speedIndex = 0;
                slowDownButton.interactable = false;
            }
            speedUpButton.interactable = true;
            SetSpeedText();
            SetPlaying(true);
        }

        private void SetSpeedText()
        {
            speedText.text = "Speed: " + speeds[speedIndex].text;
        }

        public void ClosePlayback()
        {
            for (int i = 0; i < playbackObjectParent.childCount; i++)
            {
                if ((playbackObjectParent.GetChild(i) != null) && (playbackObjectParent.GetChild(i).gameObject != null))
                {
                    Destroy(playbackObjectParent.GetChild(i).gameObject);
                }
            }
            soundPlayer.PlaySound(sound.SoundPlayer.SoundType.ScreenChange);
            hybridTransform.SwitchView(map.HybridTransform.ViewType.Metrics);
        }
    }
}
