using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text currentScoreText;
    public Text bestScoreText;
    public Text currentLevelText;

    void Update()
    {
        if (GameManager.singleton != null)
        {
            if (currentScoreText != null)
            {
                currentScoreText.text = "Score: " + GameManager.singleton.currentScore;
            }
            if (bestScoreText != null)
            {
                bestScoreText.text = "Best: " + GameManager.singleton.bestScore;
            }
            if (currentLevelText != null)
            {
                currentLevelText.text = "Level: " + GameManager.singleton.currentLevel;
            }
        }
    }
}
