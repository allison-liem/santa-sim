using System.Collections;
using TMPro;
using UnityEngine;

namespace view
{
    public class CreatureBehavior : MonoBehaviour
    {
        [field: Header("Creature")]
        [field: SerializeField]
        private GameObject creatureAnimation;
        [field: SerializeField]
        private float faceRightSign = -1;
        [field: SerializeField]
        private float minimumScale = 0.1f;
        [field: SerializeField]
        private float maximumScale = 0.3f;
        [field: SerializeField]
        private float indoorScale = 0.3f;

        [field: Header("Map")]
        [field: SerializeField]
        private map.TransformBehavior mapTransform;
        [field: SerializeField]
        private map.MapTileViewer mapTileViewer;

        [field: Header("Current")]
        [field: SerializeField]
        private logic.sim.Position position;
        [field: SerializeField]
        private float heading;

        private bool started;

        private Animator animator;
        private TextMeshPro text;

        void Awake()
        {
            mapTransform = FindObjectOfType<map.TransformBehavior>();
            mapTileViewer = FindObjectOfType<map.MapTileViewer>();
            animator = creatureAnimation.GetComponent<Animator>();
            text = GetComponentInChildren<TextMeshPro>();
        }

        void Start()
        {
            if (mapTileViewer != null)
            {
                mapTileViewer.mapChangedListeners += Redraw;
            }

            started = true;

            AnimateWalk();
        }

        void OnDestroy()
        {
            map.MapTileViewer mapTileViewer = FindObjectOfType<map.MapTileViewer>();
            if (mapTileViewer != null)
            {
                mapTileViewer.mapChangedListeners -= Redraw;
            }
        }

        void Update()
        {
        }

        public void SetPose(logic.sim.Position position, float heading)
        {
            this.position = position;
            this.heading = heading;
            if (started)
            {
                Redraw();
            }
            else
            {
                StartCoroutine(RedrawLater());
            }
        }

        public void SetCount(int count)
        {
            if (text == null)
            {
                return;
            }
            if (count >= 1)
            {
                text.text = count.ToString();
            }
            else
            {
                text.text = "";
            }
        }

        public void AnimateWalk()
        {
            animator.SetInteger("State", 2);
        }

        public void AnimateIdle()
        {
            animator.SetInteger("State", 0);
        }

        private IEnumerator RedrawLater()
        {
            yield return null;
            Redraw();
        }

        private void Redraw()
        {
            if (position is logic.sim.LatLonPosition)
            {
                Vector2 visualPosition = mapTransform.GetMapTransform().GetVector2FromPosition(position);
                transform.position = visualPosition;

                Vector3 scale = creatureAnimation.transform.localScale;
                // Scale the creature based on the camera size
                float fraction = utils.Utils.ComputeFraction(mapTileViewer.minCameraSize, mapTileViewer.maxCameraSize, mapTileViewer.mainCamera.orthographicSize);
                float scaleValue = utils.Utils.InterpolateFloat(minimumScale, maximumScale, fraction);

                if ((heading >= 0) && (heading <= Mathf.PI))
                {
                    scale.x = faceRightSign * scaleValue;
                }
                else
                {
                    scale.x = -faceRightSign * scaleValue;
                }
                scale.y = scaleValue;
                creatureAnimation.transform.localScale = scale;

                creatureAnimation.SetActive(true);
            }
            else if (position is logic.sim.IndoorPosition)
            {
                Vector2 visualPosition = mapTransform.GetMapTransform().GetVector2FromPosition(position);
                transform.position = visualPosition;

                Vector3 scale = creatureAnimation.transform.localScale;
                if ((heading >= 0) && (heading <= Mathf.PI))
                {
                    scale.x = faceRightSign * indoorScale;
                }
                else
                {
                    scale.x = -faceRightSign * indoorScale;
                }
                scale.y = indoorScale;
                creatureAnimation.transform.localScale = scale;

                creatureAnimation.SetActive(true);
            }
            else 
            {
                creatureAnimation.SetActive(false);
            }

            // Make sure the text is always flipped the right way
            var creatureScale = creatureAnimation.transform.localScale;
            var textScale = text.transform.localScale;
            textScale.x = Mathf.Sign(creatureScale.x) * Mathf.Abs(textScale.x);
            text.transform.localScale = textScale;
        }
    }
}
