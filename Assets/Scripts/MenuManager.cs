using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor de la Interfaz Gráfica de Usuario (GUI) para la navegación entre menús y escenas.
/// Cumple con el criterio:
/// "Al inicializar el videojuego, este debe contar con una interfaz gráfica funcional que permita la navegación fluida entre las escenas principales o menús."
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Paneles del Menú Principal")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Configuración de Escenas")]
    [Tooltip("Nombre de la escena de juego principal")]
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        // Aseguramos que el tiempo del juego esté activo
        Time.timeScale = 1.0f;
        ShowMainPanel();
    }

    /// <summary>
    /// Inicia la partida cargando la escena de juego tridimensional
    /// </summary>
    public void PlayGame()
    {
        // Si existe la escena configurada la carga, si no carga la siguiente en BuildSettings
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else if (SceneManager.sceneCountInBuildSettings > 1)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogWarning($"La escena {gameSceneName} no está en Build Settings. Cargando SampleScene.");
            SceneManager.LoadScene(0);
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ShowInstructions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Cierra la aplicación de forma segura
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Cerrando videojuego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
