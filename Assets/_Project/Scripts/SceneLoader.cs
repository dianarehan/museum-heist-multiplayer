using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;
   
    public void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}