using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameMode.Juggernaut
{
    public class JuggerCanvasAddOn : MonoBehaviour
    {
        #region Public Fields
        
        [HideInInspector]
        public Color arrowColor = Color.white;
        
        [HideInInspector]
        public int lives = 0;
        
        [HideInInspector]
        public GameObject lifeObject;

        #endregion

        #region Private fields

        private GameObject _lifeGridObject;

        private GameObject _arrowObject;

        private Image _arrowImg;

        private RectTransform _arrowTransform;

        private readonly List<Image> _lives = new List<Image>();

        private int _nextActiveHeart = 0;

        private readonly Color _visible = Color.white;

        private readonly Color _invisible = new Color(1, 1, 1, 0);

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.CompareTag("LifeGrid"))
                {
                    _lifeGridObject = child.gameObject;
                    SetLifeGrid(lives, lifeObject);
                }

                else if (child.gameObject.CompareTag("Arrow"))
                {
                    _arrowImg = child.GetComponent<Image>();
                    _arrowImg.color = arrowColor;
                    _arrowObject = child.gameObject;
                    _arrowTransform = child.GetComponent<RectTransform>();
                }
            }
        }

        #region Public Methods

        public void SetArrowDirection(Vector2 playerDir)
        {
            // Rotate the arrow object around the Z-axis
            float angle = Vector2.SignedAngle(Vector2.down, playerDir);
            _arrowObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void SetArrowColor(Color newColor) => _arrowImg.color = newColor;
        
        public void EliminateLife()
        {
            if (!(_nextActiveHeart < 0))
            {
                _lives[_nextActiveHeart].color = _invisible;
                _nextActiveHeart -= 1;
            }
        }

        /// <summary>
        /// changes the canvas from arrow to lives depend on the player state.
        /// </summary>
        /// <param name="state">
        /// if state is shooter - it will disable life and show direction arrow.
        /// if state is juggernaut - it will disable arrow and show lives grid. 
        /// </param>
        public void SetAddOnCanvas(JuggernautGameMode.PlayerState state)
        {
            switch (state)
            {
                case JuggernautGameMode.PlayerState.Shooter:
                    _arrowObject.SetActive(true);
                    _lifeGridObject.SetActive(false);
                    ShowAllLifeOnGrid();
                    break;
                case JuggernautGameMode.PlayerState.Juggernaut:
                    _arrowObject.SetActive(false);
                    ShowAllLifeOnGrid();
                    _lifeGridObject.SetActive(true);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void SetLifeGrid(int lifeCount, GameObject lifeObj)
        {
            for (int i = 0; i < lifeCount; ++i)
            {
                var life = Instantiate(lifeObj, _lifeGridObject.transform, true);
                var img = life.GetComponent<Image>();
                img.color = _invisible;
                _lives.Add(img);
            }

            _nextActiveHeart = lifeCount - 1;
        }

        private void ShowAllLifeOnGrid()
        {
            for (int i = 0; i < _lives.Count; i++)
            {
                _lives[i].color = _visible;
            }

            _nextActiveHeart = _lives.Count - 1;
        }

        #endregion
    }
}
