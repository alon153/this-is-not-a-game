using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Basics;
using Basics.Player;
using Managers;
using UnityEditor.Build;
using UnityEngine;
using Utilities;
using Utilities.Interfaces;
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
        
        private float[] _paintTime;
        private GameObject _splashContainer;

        #endregion

        #region GameModeBase Methods

        /// <summary>
        /// Registers as a MoveListener for all players and creates a container for all Splash objects.
        /// </summary>
        public override void InitRound()
        {   
            foreach (var player in GameManager.Instance.Players)
            {
                player.RegisterMoveListener(this);
            }

            _paintTime = new float[GameManager.Instance.Players.Count];
            for (int i = 0; i < _paintTime.Length; i++)
            {
                _paintTime[i] = Time.time;
            }
            
            InitArena();
            _splashContainer = new GameObject();
            _splashContainer.name = "Splash Container";
            _splashContainer.transform.SetParent(GameManager.Instance.DefaultArena.transform);
            
        }

        public override void InitArena()
        {
            GameObject arenaObJ = Object.Instantiate(ModeArena.gameObject, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Destroy the splash container and unregisters as MoveListener
        /// </summary>
        public override void ClearRound()
        {
            Object.Destroy(_splashContainer.gameObject);
            
            foreach (var player in GameManager.Instance.Players)
            {
                player.UnRegisterMoveListener(this);
            }
        }
        
        public override void OnTimeOVer()
        {
            GameManager.Instance.FreezePlayers(timed: false);
            CountColors();
        }

        #endregion

        #region Private Methods
        
        private void CountColors()
        {
            Dictionary<int, Color> colors = new Dictionary<int, Color>();
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                colors[i] = GameManager.Instance.PlayerColors[i].AddOffset(_coloringOffset);
            }
            GameManager.Instance.StartCoroutine(CountColors_Inner(colors, _threshold));
        }

        public void OnMove(PlayerController player, Vector3 from, Vector3 to)
        {
            if (Time.time >= _paintTime[player.Index])
            {
                var sprite = _paintingSprite == null ? player.Renderer.sprite : _paintingSprite;
                var color = player.Color;
                DrawSprite(player.transform.position, sprite, color.AddOffset(_coloringOffset));
                _paintTime[player.Index] = Time.time + _paintIntervals;
            }
        }

        private void DrawSprite(Vector3 center, Sprite sprite, Color color=default)
        {
            var splash = Object.Instantiate(_paintPrefab, center, Quaternion.identity, _splashContainer.transform);
            splash.Renderer.sprite = sprite;
            splash.Renderer.color = color;
        }
        
        /// <summary>
        /// Checks for a set of players indexes and colors how much of the arena has each player colored
        /// </summary>
        /// <param name="colors">
        /// Keys - indexes of the players, Values - the colors of each player
        /// </param>
        /// <param name="thresh">
        /// A color on the arena will be treated as the color of player i if the distance between the color on
        /// the arena and the color of the player doesn't pass this value
        /// </param>
        /// <returns></returns>
        private IEnumerator CountColors_Inner(Dictionary<int, Color> colors, float thresh)
        {
            yield return new WaitForEndOfFrame();
            
            //Get Screen Shot
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

            //Get the arena's pixel range
            var arena = GameManager.Instance.DefaultArena;
            var bottomLeft = Camera.main.WorldToScreenPoint(arena.BottomLeft);
            var topRight = Camera.main.WorldToScreenPoint(arena.TopRight);
            float total = (topRight.x - bottomLeft.x) * (topRight.y - bottomLeft.y);
            
            //Count how many pixels are colored with each color
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

            var keys = percentages.Keys.ToList();
            foreach (var key in keys)
            {
                percentages[key] = (int) (percentages[key] * 100/total);
                ScoreManager.Instance.SetPlayerScore(key, percentages[key]);
            }
            
            GameManager.Instance.EndRound();
        }

        #endregion
    }
}