using System;
using System.Collections;
using System.Collections.Generic;
using Basics;
using Basics.Player;
using Managers;
using UnityEngine;
using Utilities;
using Utilities.Listeners;
using Object = UnityEngine.Object;

namespace GameMode.Modes
{
    [Serializable]
    public class PaintMode : GameModeBase, IOnMoveListener
    {
        #region Serialized Fields

        [Tooltip("The lengths of the intervals (in seconds) between each coloring of the arena")]
        [SerializeField] private float _paintIntervals = 0.05f;
        [SerializeField] private Sprite _paintingSprite;
        [SerializeField] private Splash _paintPrefab;
        [SerializeField] private float _threshold = 0.02f;
        [SerializeField] private float _coloringOffset = 0.5f;

        #endregion

        #region Non-Serialized Fields
        
        private float _paintTime;
        private GameObject _splashContainer;
        
        #endregion

        #region GameModeBase Methods

        public override void InitRound()
        {
            foreach (var player in GameManager.Players)
            {
                player.RegisterMoveListener(this);
            }

            _paintTime = Time.time;

            _splashContainer = new GameObject();
            _splashContainer.name = "Splash Container";
            _splashContainer.transform.SetParent(GameManager.Instance.Arena.transform);
        }

        public override void ClearRound()
        {
            Object.Destroy(_splashContainer.gameObject);
            
            foreach (var player in GameManager.Players)
            {
                player.UnRegisterMoveListener(this);
            }
        }

        public override void OnTimeOVer()
        {
            CountColors();
        }

        #endregion

        #region Private Methods

        private void CountColors()
        {
            Dictionary<int, Color> colors = new Dictionary<int, Color>();
            for (int i = 0; i < GameManager.Instance.PlayerColors.Count; i++)
            {
                colors[i] = GameManager.Instance.PlayerColors[i].AddOffset(_coloringOffset);
            }
            GameManager.Instance.StartCoroutine(CountColors_Inner(colors, _threshold));
        }

        public void OnMove(PlayerController player, Vector3 from, Vector3 to)
        {
            if (Time.time >= _paintTime)
            {
                var sprite = _paintingSprite == null ? player.Renderer.sprite : _paintingSprite;
                var color = GameManager.Instance.PlayerColors[player.Index];
                DrawSprite(player.transform.position, sprite, color.AddOffset(_coloringOffset));
                _paintTime = Time.time + _paintIntervals;
            }
        }

        private void DrawSprite(Vector3 center, Sprite sprite, Color color=default)
        {
            var splash = Object.Instantiate(_paintPrefab, center, Quaternion.identity, _splashContainer.transform);
            splash.Renderer.sprite = sprite;
            splash.Renderer.color = color;
        }
        
        private IEnumerator CountColors_Inner(Dictionary<int, Color> colors, float thresh)
        {
            yield return new WaitForEndOfFrame();

            var arenaTrans = GameManager.Instance.Arena.transform;
            var scale = arenaTrans.lossyScale;
            scale.z = 0;
            var pos = arenaTrans.position;

            //Get Screen Shot
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

            //Get the arena's pixel range
            var bottomLeft = Camera.main.WorldToScreenPoint(pos - scale / 2);
            var topRight = Camera.main.WorldToScreenPoint(pos + scale / 2);
            float total = (topRight.x - bottomLeft.x) * (topRight.y - bottomLeft.y);
            
            Dictionary<int, int> percentages = new Dictionary<int, int>();
            foreach (var key in colors.Keys)
            {
                percentages[key] = 0;
            }

            for (int x = (int) bottomLeft.x; x <= topRight.x; x++)
            for (int y = (int) bottomLeft.y; y <= topRight.y; y++)
            {
                var color = tex.GetPixel(x, y);
                int min = -1;
                float minDist = Mathf.Infinity;
                foreach (int key in colors.Keys)
                {
                    var dist = Vector3.Distance(
                        new Vector3(color.r, color.g, color.b),
                        new Vector3(colors[key].r, colors[key].g, colors[key].b)
                    );

                    if (min == -1 || dist < minDist)
                    {
                        min = key;
                        minDist = dist;
                    }
                }

                if (min != -1 &&  minDist <= thresh)
                    percentages[min]++;
            }
            
            foreach (var key in percentages.Keys)
            {
                percentages[key] = (int) (percentages[key] * 100/total);
            }
        }

        #endregion
    }
}