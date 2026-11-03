using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Gestor de la interfaz en partida (HUD), controles táctiles para móviles y menús de navegación:
/// Compatible con el nuevo Input System de Unity 6 (com.unity.inputsystem):
/// - Muestra coordenadas en tiempo real (X, Y, Z) para verificar el desplazamiento tridimensional.
/// - Menú de Pausa con opciones de Continuar, Reiniciar y Volver al Menú Principal.
/// - Pantallas de Game Over y Victoria con navegación completa.
/// </summary>
public class GameHUDManager : MonoBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    [Header("Referencias del Jugador")]
    [SerializeField] private PlayerBallController player;
    [SerializeField] private HelixTowerController tower;

    [Header("Paneles de Navegación UI")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Textos Informativos (UnityEngine.UI)")]
    [Tooltip("Texto para desplegar coordenadas X, Y, Z en tiempo real")]
    [SerializeField] private Text coordinatesText;
    [SerializeField] private Text scoreText;

    [Header("Nombres de Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private int score = 0;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        isPaused = false;

        if (player == null) player = FindAnyObjectByType<PlayerBallController>();
        if (tower == null) tower = FindAnyObjectByType<HelixTowerController>();

        if (hudPanel != null) hudPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        UpdateScoreText();
    }

    void Update()
    {
        // 1. Despliegue de telemetría tridimensional (X, Y, Z) en el HUD
        if (coordinatesText != null && player != null)
        {
            Vector3 pos = player.CurrentPosition;
            Vector3 vel = player.CurrentVelocity;
            coordinatesText.text = $"Posición 3D:\n" +
                                   $"X: {pos.x:+0.00;-0.00}  |  Y: {pos.y:+0.00;-0.00}  |  Z: {pos.z:+0.00;-0.00}\n" +
                                   $"Velocidad: (Vx: {vel.x:F1}, Vy: {vel.y:F1}, Vz: {vel.z:F1})";
        }

        // 2. Control de pausa con Input System (Escape o P)
        bool pausePressed = false;
        if (Keyboard.current != null)
        {
            pausePressed = Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame;
        }
        else
        {
            try
            {
                pausePressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
            }
            catch (System.InvalidOperationException) { }
        }

        if (pausePressed)
        {
            TogglePause();
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntos: {score}";
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0.0f : 1.0f;

        if (pausePanel != null) pausePanel.SetActive(isPaused);
        if (hudPanel != null) hudPanel.SetActive(!isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1.0f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1.0f;
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0.0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    public void TriggerVictory()
    {
        Time.timeScale = 0.0f;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    // Controles táctiles móviles en pantalla
    public void OnTouchLeft() { if (player != null) player.SetInputX(-1f); }
    public void OnTouchRight() { if (player != null) player.SetInputX(1f); }
    public void OnTouchForward() { if (player != null) player.SetInputZ(1f); }
    public void OnTouchBackward() { if (player != null) player.SetInputZ(-1f); }
    public void OnTouchJump() { if (player != null) player.DoManualJump(); }
    public void OnRotateTowerLeft() { if (tower != null) tower.RotateByButton(-1f); }
    public void OnRotateTowerRight() { if (tower != null) tower.RotateByButton(1f); }
}
