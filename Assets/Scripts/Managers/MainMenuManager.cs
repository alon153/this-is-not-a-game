using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        public void LoadGame()
        {
            Debug.Log("Loading Game");
            SceneManager.LoadScene("Main");
        }

        public void QuitGame()
        {   
            Debug.Log("Quitting Game");
            Application.Quit();
        }
    }
}
