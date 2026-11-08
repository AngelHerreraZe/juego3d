using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _singleton;

    public static GameManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindAnyObjectByType<GameManager>();
                if (_singleton == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _singleton = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _singleton;
        }
        private set
        {
            _singleton = value;
        }
    }

    public int currentScore = 0;
    public int bestScore = 0;
    public int currentLevel = 1;
    public float difficultyMultiplier = 1.0f;

    void Awake()
    {
        if (_singleton == null)
        {
            _singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        bestScore = PlayerPrefs.GetInt("Highscore", 0);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        UpdateDifficulty();
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("Highscore", bestScore);
            PlayerPrefs.Save();
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
        UpdateDifficulty();
        currentScore += 10;
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("Highscore", bestScore);
            PlayerPrefs.Save();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartLevel()
    {
        currentScore = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetProgress()
    {
        currentScore = 0;
        bestScore = 0;
        currentLevel = 1;
        PlayerPrefs.DeleteKey("Highscore");
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.Save();
        UpdateDifficulty();
    }

    private void UpdateDifficulty()
    {
        difficultyMultiplier = 1.0f + (currentLevel - 1) * 0.15f;
    }
}
