using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void LoadGameScene()
    {
        Debug.Log("Button clicked");
        SceneManager.LoadScene("GameScene");
    }
}
