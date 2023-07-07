using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private EventReference _clickButton;
        [SerializeField] private EventReference _selectButton;
        
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
        
        public void SelectSound()
        {
            AudioManager.PlayOneShot(_selectButton);
        }

        public void ClickSound()
        {
            AudioManager.PlayOneShot(_clickButton);
        }
    }
}
