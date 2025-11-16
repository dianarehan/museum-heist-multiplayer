using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static string WinnerMessage = "Game Over";

    [SerializeField] private TMP_Text gameOverText;

    void Start()
    {
        if (gameOverText != null)
            gameOverText.text = WinnerMessage;
        else
            Debug.LogError("GameOverManager: No TMP text assigned!");
    }
}
