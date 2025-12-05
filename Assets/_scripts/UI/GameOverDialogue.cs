using System.Collections;
using UnityEngine;

namespace Assets._scripts.UI
{
    public class GameOverDialogue : MonoBehaviour
    {

        public void OnGiveUpYesPressed()
        {
            GameManager.Instance.SetPanelVisibility(GameManager.Instance.GiveUpPanel, false);
            GameManager.Instance.SetPanelVisibility(GameManager.Instance.GameOverPanel, true);
        }
        public void OnGiveUpNoPressed() {
            GameManager.Instance.SetPanelVisibility(GameManager.Instance.GiveUpPanel, false);
            GameManager.Instance.SetPanelVisibility(GameManager.Instance.GameOverPanel, false);

            GameManager.Instance.ResumeGame();
        }

        public void OnGameOverTryAgainPressed()
        {
            GameManager.Instance.SetPanelVisibility(GameManager.Instance.GameOverPanel, false);

            GameManager.Instance.GameLoop(GameManager.GameState.GameOverRestart); // run game over sequence
            // ensure panels are hidden
        }

        public void OnGameOverQuitPressed()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}