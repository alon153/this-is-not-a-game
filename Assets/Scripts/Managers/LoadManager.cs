using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{

    public class LoadManager : MonoBehaviour
    {
        public enum Scene
        {
            LOBBY,
            GAME,
        }

        #region Serialized Fields

        [SerializeField] private string _lobbySceneName = "Lobby Scene";
        [SerializeField] private string _gameSceneName = "Game Scene";

        #endregion

        #region Non-Serialized Fields

        private CanvasGroup _loadingCanvas;
        private Scene _startScene;

        private static LoadManager _instance;
        private static readonly float _durationTime = 0.5f;
        private static readonly float _loadScreenTime = 0.5f;

        #endregion

        #region Properties

        public static Scene CurrentScene { get; private set; }

        #endregion

        #region Function Events

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _loadingCanvas = GetComponentInChildren<CanvasGroup>();
            _startScene = SceneManager.GetActiveScene().name == _lobbySceneName ? Scene.LOBBY : Scene.GAME;
            CurrentScene = _startScene;
            transform.SetParent(null);
            DontDestroyOnLoad(_instance.gameObject);
        }

        #endregion

        #region Public Methods

        public static void ReloadStartingScene()
        {
            switch (_instance._startScene)
            {
                case Scene.LOBBY:
                    LoadLobby();
                    break;
                case Scene.GAME:
                    LoadGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void LoadLobby()
        {
            _instance.LoadScene(_instance._lobbySceneName, onLoad: (() => { CurrentScene = Scene.LOBBY; }));
        }

        public static void LoadGame()
        {
            _instance.LoadScene(_instance._gameSceneName, onLoad: (() => { CurrentScene = Scene.GAME; }));
        }


        private void LoadScene(string name, LoadSceneMode mode = LoadSceneMode.Single, Action beforeFade = null,
            Action onLoad = null, Action afterFade = null)
        {
            StartCoroutine(LoadScene_Inner(
                name, mode, _durationTime, _loadScreenTime,
                beforeFade, onLoad, afterFade));
        }

        #endregion

        #region Private Methods

        private IEnumerator LoadScene_Inner(
            string name,
            LoadSceneMode mode, float fadeDuration = 0f, float loadScreenDuration = 0f,
            Action beforeFade = null, Action onLoad = null, Action afterFade = null)
        {
            beforeFade?.Invoke();
            yield return null;

            yield return InterpolateLoadingAlpha(0, 1, fadeDuration);

            var loadTime = Time.time + loadScreenDuration;

            var scene = SceneManager.LoadSceneAsync(name, mode);
            scene.allowSceneActivation = false;

            while (scene.progress < 0.9f || Time.time < loadTime)
            {
                yield return new WaitForSeconds(0.1f);
            }

            onLoad.Invoke();
            yield return null;

            scene.allowSceneActivation = true;
            yield return InterpolateLoadingAlpha(1, 0, fadeDuration);

            afterFade?.Invoke();
            yield return null;
        }

        private IEnumerator InterpolateLoadingAlpha(float from, float to, float duration)
        {
            float interpolationTime = Time.time + duration;

            _loadingCanvas.alpha = from;
            yield return null;

            float t = from;
            while (Time.time < interpolationTime)
            {
                t = 1 - (interpolationTime - Time.time) / duration;
                _loadingCanvas.alpha = (1 - t) * from + t * to;
                yield return null;
            }

            _loadingCanvas.alpha = to;
            yield return null;
        }

        #endregion
    }
}