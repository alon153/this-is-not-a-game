using System.Collections.Generic;
using GameMode.Modes;
using ScriptableObjects.GameModes.Modes;
using UnityEngine;

namespace GameMode.Paint
{
    [CreateAssetMenu(fileName = "PaintModeObjects", menuName = "ScriptableObjects/GameModes/PaintModeObject")]
    public class PaintModeObject : GameModeObject
    {
        public float _paintIntervals = 0.05f;
        public List<Sprite> _paintingSprites;
        public Splash _paintPrefab;
        public float _threshold = 0.02f;
        public float _coloringOffset = 0.5f;
        public int _totalPoints = 10;
        public int _colorCountSkip = 100;
    }
}