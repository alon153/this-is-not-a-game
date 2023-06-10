using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Basics;
using Basics.Player;
using GameMode.Paint;
using Managers;
using ScriptableObjects.GameModes.Modes;
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
        #region Non-Serialized Fields
        
        private float _paintIntervals = 0.05f;
        private List<Sprite> _paintingSprites;
        private Splash _paintPrefab;
        private float _threshold = 0.02f;
        private float _coloringOffset = 0.5f;
        private int _totalPoints = 10;
        private int _colorCountSkip = 100;

        private static int order = Int32.MinValue;
        
        private float[] _paintTime;
        private GameObject _splashContainer;
        private Dictionary<int, float> _percentages = new Dictionary<int, float>();

        #endregion

        #region GameModeBase Methods

        protected override void ExtractScriptableObject(GameModeObject input)
        {
            PaintModeObject sObj = (PaintModeObject) input;
            _paintIntervals = sObj._paintIntervals;
            _paintingSprites = sObj._paintingSprites;
            _paintPrefab = sObj._paintPrefab;
            _threshold = sObj._threshold;
            _coloringOffset = sObj._coloringOffset;
            _totalPoints = sObj._totalPoints;
            _colorCountSkip = sObj._colorCountSkip;
        }

        /// <summary>
        /// Registers as a MoveListener for all players and creates a container for all Splash objects.
        /// </summary>
        protected override void InitRound_Inner()
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
        }

        protected override void InitArena_Inner()
        {
            Arena arena = Object.Instantiate(ModeArena, Vector3.zero, Quaternion.identity);
            _splashContainer = new GameObject();
            _splashContainer.name = "Splash Container";
            _splashContainer.transform.SetParent(arena.transform);

            GameManager.Instance.CurrArena = arena;
        }

        /// <summary>
        /// Destroy the splash container and unregisters as MoveListener
        /// </summary>
        protected override void ClearRound_Inner()
        {
            Object.Destroy(_splashContainer.gameObject);
            
            foreach (var player in GameManager.Instance.Players)
            {
                player.UnRegisterMoveListener(this);
            }
        }
        
        protected override void EndRound_Inner()
        {
            ScoreManager.Instance.SetPlayerScores(CalculateScore_Inner());
        }
        
        protected override void OnTimeOver_Inner()
        {
            GameManager.Instance.FreezePlayers(timed: false);
            CountColors();
        }

        protected override Dictionary<int, float> CalculateScore_Inner()
        {
            Dictionary<int, float> scores = new Dictionary<int, float>();
            foreach (var index in _percentages.Keys)
            {
                scores[index] = (int) (_percentages[index] * _totalPoints*10);
            }

            return scores;
        }

        public override void OnTimerOver()
        {
            OnTimeOver_Inner();
        }

        #endregion

        #region Private Methods
        
        private void CountColors()
        {
            Dictionary<int, Color> colors = new Dictionary<int, Color>();
            for (int i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                colors[i] = GameManager.Instance.PlayerColor(i).AddOffset(_coloringOffset);
            }
            GameManager.Instance.StartCoroutine(CountColors_Inner(colors, _threshold));
        }

        public void OnMove(PlayerController player, Vector3 from, Vector3 to)
        {
            if (Time.time >= _paintTime[player.Index])
            {
                bool hasSprite = _paintingSprites != null && _paintingSprites.Count > player.Index; 
                
                var sprite = hasSprite ? _paintingSprites[player.Index] : player.Renderer.RegularSprite;
                
                DrawSprite(player.transform.position, sprite);
                _paintTime[player.Index] = Time.time + _paintIntervals;
            }
        }

        private void DrawSprite(Vector3 center, Sprite sprite)
        {
            var splash = Object.Instantiate(_paintPrefab, center, Quaternion.identity, _splashContainer.transform);
            splash.Renderer.sortingOrder = ++order;
            splash.Renderer.sprite = sprite;
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
            var arena = GameManager.Instance.CurrArena;
            var bottomLeft = Camera.main.WorldToScreenPoint(arena.BottomLeft);
            var topRight = Camera.main.WorldToScreenPoint(arena.TopRight);
            int width = (int) (topRight.x - bottomLeft.x);
            int height = (int) (topRight.y - bottomLeft.y);
            float total = ((float) width * height) / _colorCountSkip;
            
            //Count how many pixels are colored with each color
            foreach (var key in colors.Keys)
            {
                _percentages[key] = 0;
            }

            var pixels = tex.GetPixels((int) bottomLeft.x, (int) bottomLeft.y, width, height);
            for (int i=0; i<pixels.Length; i+=_colorCountSkip)
            {
                var color = pixels[i];
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
                    _percentages[min]++;
            }

            var keys = _percentages.Keys.ToList();
            foreach (var key in keys)
            {
                _percentages[key] /= total;
            }
            
            EndRound();
        }

        #endregion
    }
}