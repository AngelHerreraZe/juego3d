using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

/// <summary>
/// Script de Editor para automatizar la configuración completa del proyecto Helix Jump 3D:
/// - Creación de materiales URP / Standard.
/// - Construcción de la escena 'MainMenuScene' con GUI responsiva y navegación fluida.
/// - Construcción de la escena 'GameScene' con jugador 3D, cámara con restricciones (X, Z sincronizados, Z fija, Y desacoplada), torre y HUD interactivo.
/// - Configuración de Build Settings para la secuencia de ejecución requerida.
/// </summary>
public static class HelixGameSetup
{
    private const string MaterialsPath = "Assets/Materials";
    private const string ScenesPath = "Assets/Scenes";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Helix Jump 3D/Configurar Proyecto y Escenas")]
    public static void SetupFullProject()
    {
        Debug.Log("=== INICIANDO CONFIGURACIÓN INTEGRAL DE HELIX JUMP 3D ===");

        EnsureDirectories();
        CreateMaterials();
        CreateMainMenuScene();
        CreateGameScene();
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== ¡PROYECTO HELIX JUMP 3D CONFIGURADO CON ÉXITO! ===");
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder(MaterialsPath)) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(ScenesPath)) AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    private static Material GetOrCreateMaterial(string matName, Color color)
    {
        string path = $"{MaterialsPath}/{matName}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static void CreateMaterials()
    {
        GetOrCreateMaterial("BallMaterial", new Color(0.15f, 0.45f, 0.95f));       // Azul vibrante
        GetOrCreateMaterial("TowerMaterial", new Color(0.20f, 0.25f, 0.32f));      // Gris oscuro
        GetOrCreateMaterial("PlatformMaterial", new Color(0.96f, 0.62f, 0.05f));   // Amarillo/Naranja lúdico
        GetOrCreateMaterial("HazardMaterial", new Color(0.92f, 0.22f, 0.22f));     // Rojo obstáculo
        GetOrCreateMaterial("GoalMaterial", new Color(0.06f, 0.72f, 0.48f));       // Verde meta
    }

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Cámara y Luz
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.12f, 0.20f);
        camObj.transform.position = new Vector3(0, 0, -10);
        camObj.tag = "MainCamera";

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 2. Event System
        CreateEventSystem();

        // 3. Canvas Principal
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 4. MenuManager
        MenuManager menuMgr = canvasObj.AddComponent<MenuManager>();

        // 5. Panel Principal
        GameObject mainPanel = CreateUIPanel(canvasObj.transform, "MainPanel", new Color(0, 0, 0, 0));

        // Título del Juego
        CreateUIText(mainPanel.transform, "TitleText", "HELIX JUMP 3D", 72, FontStyle.Bold, Color.white,
                     new Vector2(0, 450), new Vector2(900, 160));

        // Subtítulo
        CreateUIText(mainPanel.transform, "SubtitleText", "Propuesta en 3D para Móviles - Actividad 3188", 30, FontStyle.Normal, new Color(0.8f, 0.85f, 0.95f),
                     new Vector2(0, 360), new Vector2(900, 80));

        // Botón Jugar
        GameObject playBtn = CreateUIButton(mainPanel.transform, "PlayButton", "JUGAR / INICIAR", 38,
                                            new Color(0.15f, 0.55f, 0.95f), new Vector2(0, 100), new Vector2(500, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.GetComponent<Button>().onClick, menuMgr.PlayGame);

        // Botón Instrucciones
        GameObject instrBtn = CreateUIButton(mainPanel.transform, "InstructionsButton", "CÓMO JUGAR / CONTROLES", 30,
                                             new Color(0.25f, 0.35f, 0.50f), new Vector2(0, -50), new Vector2(500, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(instrBtn.GetComponent<Button>().onClick, menuMgr.ShowInstructions);

        // Botón Salir
        GameObject quitBtn = CreateUIButton(mainPanel.transform, "QuitButton", "SALIR", 28,
                                            new Color(0.70f, 0.20f, 0.25f), new Vector2(0, -180), new Vector2(500, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.GetComponent<Button>().onClick, menuMgr.QuitGame);

        // 6. Panel de Instrucciones (Inicialmente inactivo)
        GameObject instrPanel = CreateUIPanel(canvasObj.transform, "InstructionsPanel", new Color(0.05f, 0.08f, 0.15f, 0.95f));
        instrPanel.SetActive(false);

        CreateUIText(instrPanel.transform, "InstrTitle", "CONTROLES Y REGLAS 3D", 50, FontStyle.Bold, Color.white,
                     new Vector2(0, 480), new Vector2(900, 120));

        string instrContent = "• Desplazamiento Tridimensional (X, Y, Z):\n" +
                              "   - Eje X: Desplazamiento lateral (A / D o Flechas Izq / Der).\n" +
                              "   - Eje Y: Rebote físico controlado e impulso por script al caer.\n" +
                              "   - Eje Z: Desplazamiento en profundidad (W / S o Flechas Arr / Abajo).\n" +
                              "   - Salto Manual: Tecla ESPACIO o botón táctil de salto.\n\n" +
                              "• Rotación de la Torre Helicoidal:\n" +
                              "   - Arrastra el ratón o desliza el dedo en pantalla móvil.\n" +
                              "   - Busca los huecos para descender de nivel.\n\n" +
                              "• Estabilidad de Cámara:\n" +
                              "   - Seguimiento sincronizado en los ejes X y Z.\n" +
                              "   - Distancia fija y constante en el eje Z.\n" +
                              "   - Eje Y desacoplado para evitar vibraciones en móvil.";

        CreateUIText(instrPanel.transform, "InstrBody", instrContent, 28, FontStyle.Normal, Color.white,
                     new Vector2(0, 60), new Vector2(850, 600));

        GameObject backBtn = CreateUIButton(instrPanel.transform, "BackButton", "VOLVER AL MENÚ", 32,
                                            new Color(0.15f, 0.55f, 0.95f), new Vector2(0, -420), new Vector2(450, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.GetComponent<Button>().onClick, menuMgr.ShowMainPanel);

        // Enlazar referencias en MenuManager mediante SerializedObject
        SerializedObject so = new SerializedObject(menuMgr);
        so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        so.FindProperty("instructionsPanel").objectReferenceValue = instrPanel;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        Debug.Log("Escena guardada: " + MainMenuScenePath);
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Iluminación Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1.0f, 0.98f, 0.95f);
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 2. Materiales cargados
        Material ballMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/BallMaterial.mat");
        Material towerMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/TowerMaterial.mat");
        Material platMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlatformMaterial.mat");
        Material hazMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/HazardMaterial.mat");
        Material goalMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/GoalMaterial.mat");

        // 3. Torre Helix (Objeto Padre rotatorio)
        GameObject towerObj = new GameObject("HelixTower");
        towerObj.transform.position = Vector3.zero;
        HelixTowerController towerCtrl = towerObj.AddComponent<HelixTowerController>();
        HelixLevelBuilder levelBuilder = towerObj.AddComponent<HelixLevelBuilder>();

        SerializedObject soBuilder = new SerializedObject(levelBuilder);
        soBuilder.FindProperty("towerMaterial").objectReferenceValue = towerMat;
        soBuilder.FindProperty("platformMaterial").objectReferenceValue = platMat;
        soBuilder.FindProperty("hazardMaterial").objectReferenceValue = hazMat;
        soBuilder.FindProperty("goalMaterial").objectReferenceValue = goalMat;
        soBuilder.ApplyModifiedProperties();

        // Generar geometría de la torre inmediatamente
        levelBuilder.BuildHelixTower();

        // 4. Personaje Principal (Ball / Esfera)
        GameObject ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballObj.name = "PlayerBall";
        ballObj.tag = "Player";
        ballObj.transform.position = new Vector3(0, 3.5f, 1.7f); // Sobre el primer sector
        ballObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (ballMat != null) ballObj.GetComponent<Renderer>().sharedMaterial = ballMat;

        PlayerBallController ballCtrl = ballObj.AddComponent<PlayerBallController>();

        // 5. Cámara Principal con Restricciones Solicitadas
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.16f, 0.24f);
        camObj.tag = "MainCamera";

        // Posición inicial: fija en Z respecto a la bola, altura estable en Y
        camObj.transform.position = new Vector3(0, 3.5f, -6.5f);
        camObj.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        CameraFollow3D camFollow = camObj.AddComponent<CameraFollow3D>();
        SerializedObject soCam = new SerializedObject(camFollow);
        soCam.FindProperty("target").objectReferenceValue = ballObj.transform;
        soCam.FindProperty("fixedOffsetZ").floatValue = -6.5f;
        soCam.FindProperty("offsetX").floatValue = 0.0f;
        soCam.FindProperty("fixedHeightY").floatValue = 3.5f;
        soCam.FindProperty("smoothTimeX").floatValue = 0.12f;
        soCam.FindProperty("pitchAngle").floatValue = 20.0f;
        soCam.ApplyModifiedProperties();

        // 6. Event System
        CreateEventSystem();

        // 7. Canvas y HUD de Partida
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameHUDManager hudMgr = canvasObj.AddComponent<GameHUDManager>();

        // HUD Panel
        GameObject hudPanel = CreateUIPanel(canvasObj.transform, "HUDPanel", new Color(0, 0, 0, 0));

        // Texto de Coordenadas 3D (Criterio de Verificación de Movimiento en X, Y, Z)
        Text coordsText = CreateUIText(hudPanel.transform, "TelemetryCoordinatesText",
                                     "Posición 3D:\nX: +0.00  |  Y: +0.00  |  Z: +0.00\nVelocidad: (Vx: 0.0, Vy: 0.0, Vz: 0.0)",
                                     24, FontStyle.Bold, Color.white,
                                     new Vector2(-240, 840), new Vector2(550, 140), TextAnchor.UpperLeft);

        // Texto de Puntos
        Text scoreText = CreateUIText(hudPanel.transform, "ScoreText", "Puntos: 0", 34, FontStyle.Bold,
                                    new Color(1f, 0.85f, 0.2f), new Vector2(250, 840), new Vector2(350, 80), TextAnchor.UpperRight);

        // Botón Pausa
        GameObject pauseBtn = CreateUIButton(hudPanel.transform, "PauseButton", "PAUSA", 24,
                                             new Color(0.2f, 0.25f, 0.35f, 0.9f), new Vector2(440, 840), new Vector2(140, 70));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pauseBtn.GetComponent<Button>().onClick, hudMgr.TogglePause);

        // Controles Táctiles Móviles en Pantalla (Panel Inferior)
        GameObject controlsPanel = CreateUIPanel(hudPanel.transform, "MobileControlsPanel", new Color(0, 0, 0, 0));
        
        // Botones de Movimiento en X (Izquierda / Derecha)
        GameObject btnLeft = CreateUIButton(controlsPanel.transform, "BtnLeft", "◄ X-", 32,
                                           new Color(0.2f, 0.4f, 0.7f, 0.75f), new Vector2(-360, -750), new Vector2(150, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLeft.GetComponent<Button>().onClick, hudMgr.OnTouchLeft);

        GameObject btnRight = CreateUIButton(controlsPanel.transform, "BtnRight", "X+ ►", 32,
                                            new Color(0.2f, 0.4f, 0.7f, 0.75f), new Vector2(-190, -750), new Vector2(150, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnRight.GetComponent<Button>().onClick, hudMgr.OnTouchRight);

        // Botones de Desplazamiento en Z (Profundidad Adelante / Atrás)
        GameObject btnFwd = CreateUIButton(controlsPanel.transform, "BtnForward", "▲ Z+", 30,
                                          new Color(0.15f, 0.6f, 0.45f, 0.75f), new Vector2(190, -700), new Vector2(140, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnFwd.GetComponent<Button>().onClick, hudMgr.OnTouchForward);

        GameObject btnBack = CreateUIButton(controlsPanel.transform, "BtnBackward", "▼ Z-", 30,
                                           new Color(0.15f, 0.6f, 0.45f, 0.75f), new Vector2(190, -820), new Vector2(140, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBack.GetComponent<Button>().onClick, hudMgr.OnTouchBackward);

        // Botón Salto (Eje Y)
        GameObject btnJump = CreateUIButton(controlsPanel.transform, "BtnJump", "SALTO (Y)", 30,
                                           new Color(0.85f, 0.55f, 0.1f, 0.85f), new Vector2(370, -750), new Vector2(170, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnJump.GetComponent<Button>().onClick, hudMgr.OnTouchJump);

        // Panel de Pausa (Inicialmente inactivo)
        GameObject pausePanel = CreateUIPanel(canvasObj.transform, "PausePanel", new Color(0.08f, 0.12f, 0.20f, 0.95f));
        pausePanel.SetActive(false);

        CreateUIText(pausePanel.transform, "PauseTitle", "JUEGO PAUSADO", 60, FontStyle.Bold, Color.white,
                     new Vector2(0, 350), new Vector2(800, 120));

        GameObject resumeBtn = CreateUIButton(pausePanel.transform, "ResumeBtn", "CONTINUAR", 34,
                                              new Color(0.15f, 0.55f, 0.95f), new Vector2(0, 120), new Vector2(450, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resumeBtn.GetComponent<Button>().onClick, hudMgr.ResumeGame);

        GameObject restartBtn = CreateUIButton(pausePanel.transform, "RestartBtn", "REINICIAR ESCENA", 32,
                                               new Color(0.25f, 0.35f, 0.50f), new Vector2(0, -10), new Vector2(450, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.GetComponent<Button>().onClick, hudMgr.RestartLevel);

        GameObject returnMenuBtn = CreateUIButton(pausePanel.transform, "ReturnMenuBtn", "MENÚ PRINCIPAL", 30,
                                                  new Color(0.70f, 0.20f, 0.25f), new Vector2(0, -140), new Vector2(450, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(returnMenuBtn.GetComponent<Button>().onClick, hudMgr.ReturnToMainMenu);

        // Panel de Game Over
        GameObject gameOverPanel = CreateUIPanel(canvasObj.transform, "GameOverPanel", new Color(0.25f, 0.05f, 0.05f, 0.95f));
        gameOverPanel.SetActive(false);
        CreateUIText(gameOverPanel.transform, "OverTitle", "¡FIN DEL JUEGO!", 60, FontStyle.Bold, new Color(1f, 0.3f, 0.3f),
                     new Vector2(0, 250), new Vector2(800, 120));
        GameObject retryBtn = CreateUIButton(gameOverPanel.transform, "RetryBtn", "REINTENTAR", 34,
                                             new Color(0.85f, 0.25f, 0.25f), new Vector2(0, 50), new Vector2(450, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.GetComponent<Button>().onClick, hudMgr.RestartLevel);
        GameObject overMenuBtn = CreateUIButton(gameOverPanel.transform, "OverMenuBtn", "MENÚ PRINCIPAL", 30,
                                                new Color(0.3f, 0.3f, 0.4f), new Vector2(0, -80), new Vector2(450, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(overMenuBtn.GetComponent<Button>().onClick, hudMgr.ReturnToMainMenu);

        // Panel de Victoria
        GameObject victoryPanel = CreateUIPanel(canvasObj.transform, "VictoryPanel", new Color(0.05f, 0.25f, 0.12f, 0.95f));
        victoryPanel.SetActive(false);
        CreateUIText(victoryPanel.transform, "VicTitle", "¡NIVEL COMPLETADO!", 60, FontStyle.Bold, new Color(0.3f, 1f, 0.5f),
                     new Vector2(0, 250), new Vector2(800, 120));
        GameObject vicPlayAgainBtn = CreateUIButton(victoryPanel.transform, "VicPlayAgainBtn", "JUGAR DE NUEVO", 34,
                                                    new Color(0.1f, 0.7f, 0.35f), new Vector2(0, 50), new Vector2(450, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(vicPlayAgainBtn.GetComponent<Button>().onClick, hudMgr.RestartLevel);
        GameObject vicMenuBtn = CreateUIButton(victoryPanel.transform, "VicMenuBtn", "MENÚ PRINCIPAL", 30,
                                               new Color(0.3f, 0.3f, 0.4f), new Vector2(0, -80), new Vector2(450, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(vicMenuBtn.GetComponent<Button>().onClick, hudMgr.ReturnToMainMenu);

        // Enlazar referencias de GameHUDManager mediante SerializedObject
        SerializedObject soHud = new SerializedObject(hudMgr);
        soHud.FindProperty("player").objectReferenceValue = ballCtrl;
        soHud.FindProperty("tower").objectReferenceValue = towerCtrl;
        soHud.FindProperty("hudPanel").objectReferenceValue = hudPanel;
        soHud.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        soHud.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        soHud.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        soHud.FindProperty("coordinatesText").objectReferenceValue = coordsText;
        soHud.FindProperty("scoreText").objectReferenceValue = scoreText;
        soHud.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, GameScenePath);
        Debug.Log("Escena guardada: " + GameScenePath);
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[2];
        scenes[0] = new EditorBuildSettingsScene(MainMenuScenePath, true);
        scenes[1] = new EditorBuildSettingsScene(GameScenePath, true);
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build Settings configurado: [0] MainMenuScene, [1] GameScene");
    }

    private static void CreateEventSystem()
    {
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreateUIPanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        return panel;
    }

    private static Text CreateUIText(Transform parent, string name, string content, int fontSize,
                                    FontStyle fontStyle, Color color, Vector2 anchoredPos, Vector2 sizeDelta,
                                    TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return text;
    }

    private static GameObject CreateUIButton(Transform parent, string name, string textContent, int fontSize,
                                             Color btnColor, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = btnColor * 1.15f;
        cb.pressedColor = btnColor * 0.85f;
        btn.colors = cb;

        // Texto del botón
        CreateUIText(btnObj.transform, "Text", textContent, fontSize, FontStyle.Bold, Color.white,
                     Vector2.zero, sizeDelta, TextAnchor.MiddleCenter);

        return btnObj;
    }
}
