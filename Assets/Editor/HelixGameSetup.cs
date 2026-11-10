using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;

public static class HelixGameSetup
{
    private const string MaterialsPath = "Assets/Materials";
    private const string ScenesPath = "Assets/Scenes";
    private const string PrefabsPath = "Assets/Prefabs";
    private const string StagesPath = "Assets/Stages";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Helix Jump 3D/Configurar Proyecto y Escenas")]
    public static void SetupFullProject()
    {
        EnsureDirectories();
        CreateMaterials();
        CreateHelixLevelPrefab();
        CreateStages();
        CreateMainMenuScene();
        CreateGameScene();
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder(MaterialsPath)) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(ScenesPath)) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder(PrefabsPath)) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(StagesPath)) AssetDatabase.CreateFolder("Assets", "Stages");
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
        GetOrCreateMaterial("BallMaterial", new Color(0.15f, 0.45f, 0.95f));
        GetOrCreateMaterial("TowerMaterial", new Color(0.20f, 0.25f, 0.32f));
        GetOrCreateMaterial("PlatformMaterial", new Color(0.96f, 0.62f, 0.05f));
        GetOrCreateMaterial("HazardMaterial", new Color(0.92f, 0.22f, 0.22f));
        GetOrCreateMaterial("GoalMaterial", new Color(0.06f, 0.72f, 0.48f));
    }

    private static void CreateHelixLevelPrefab()
    {
        string prefabPath = $"{PrefabsPath}/HelixLevelPrefab.prefab";
        Material platMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlatformMaterial.mat");

        GameObject root = new GameObject("HelixLevelPrefab");

        GameObject triggerObj = new GameObject("PassScoreTrigger");
        triggerObj.transform.SetParent(root.transform);
        triggerObj.transform.localPosition = Vector3.zero;
        BoxCollider bc = triggerObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(4.5f, 0.1f, 4.5f);
        triggerObj.AddComponent<PassScorePoint>();

        float towerRadius = 1.0f;
        float ringOuterRadius = 2.4f;
        float segDist = (towerRadius + ringOuterRadius) / 2f;
        float segWidth = (ringOuterRadius - towerRadius);
        float segLength = Mathf.Tan(15.0f * Mathf.Deg2Rad) * segDist * 2.1f;

        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30.0f;
            float rad = angle * Mathf.Deg2Rad;
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = $"HelixPart_{i}";
            part.transform.SetParent(root.transform);
            part.transform.localPosition = new Vector3(Mathf.Sin(rad) * segDist, 0, Mathf.Cos(rad) * segDist);
            part.transform.localRotation = Quaternion.Euler(0, angle, 0);
            part.transform.localScale = new Vector3(segLength, 0.25f, segWidth);

            if (platMat != null)
            {
                part.GetComponent<Renderer>().sharedMaterial = platMat;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        GameObject.DestroyImmediate(root);
    }

    private static void CreateStages()
    {
        CreateStageAsset("Stage_0", new Color(0.12f, 0.18f, 0.28f), new Color(0.95f, 0.72f, 0.15f), new Color(0.95f, 0.25f, 0.25f),
            new List<Level>
            {
                new Level { partCount = 10, deathPartCount = 0 },
                new Level { partCount = 10, deathPartCount = 1 },
                new Level { partCount = 9,  deathPartCount = 1 },
                new Level { partCount = 9,  deathPartCount = 2 },
                new Level { partCount = 8,  deathPartCount = 2 },
                new Level { partCount = 8,  deathPartCount = 3 }
            });

        CreateStageAsset("Stage_1", new Color(0.18f, 0.10f, 0.28f), new Color(0.25f, 0.85f, 0.35f), new Color(0.20f, 0.80f, 0.95f),
            new List<Level>
            {
                new Level { partCount = 9, deathPartCount = 1 },
                new Level { partCount = 9, deathPartCount = 2 },
                new Level { partCount = 8, deathPartCount = 2 },
                new Level { partCount = 8, deathPartCount = 3 },
                new Level { partCount = 8, deathPartCount = 3 },
                new Level { partCount = 7, deathPartCount = 4 },
                new Level { partCount = 7, deathPartCount = 4 },
                new Level { partCount = 6, deathPartCount = 5 }
            });

        CreateStageAsset("Stage_2", new Color(0.25f, 0.08f, 0.12f), new Color(0.15f, 0.90f, 0.85f), new Color(1.0f, 0.85f, 0.10f),
            new List<Level>
            {
                new Level { partCount = 8, deathPartCount = 2 },
                new Level { partCount = 8, deathPartCount = 3 },
                new Level { partCount = 7, deathPartCount = 3 },
                new Level { partCount = 7, deathPartCount = 4 },
                new Level { partCount = 7, deathPartCount = 4 },
                new Level { partCount = 6, deathPartCount = 5 },
                new Level { partCount = 6, deathPartCount = 5 },
                new Level { partCount = 5, deathPartCount = 5 }
            });
    }

    private static void CreateStageAsset(string name, Color bg, Color partsColor, Color ballColor, List<Level> levels)
    {
        string path = $"{StagesPath}/{name}.asset";
        Stage stage = AssetDatabase.LoadAssetAtPath<Stage>(path);
        if (stage == null)
        {
            stage = ScriptableObject.CreateInstance<Stage>();
            stage.stageBackgroundColor = bg;
            stage.stageLevelPartColor = partsColor;
            stage.stageBallColor = ballColor;
            stage.levels = levels;
            AssetDatabase.CreateAsset(stage, path);
        }
        else
        {
            stage.stageBackgroundColor = bg;
            stage.stageLevelPartColor = partsColor;
            stage.stageBallColor = ballColor;
            stage.levels = levels;
            EditorUtility.SetDirty(stage);
        }
    }

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.12f, 0.18f);
        camObj.tag = "MainCamera";

        CreateEventSystem();

        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        MenuManager menuMgr = canvasObj.AddComponent<MenuManager>();

        GameObject mainPanel = CreateUIPanel(canvasObj.transform, "MainPanel", new Color(0, 0, 0, 0));

        CreateUIText(mainPanel.transform, "GameTitle", "HELIX JUMP 3D", 68, FontStyle.Bold,
                     new Color(1f, 0.85f, 0.2f), new Vector2(0, 480), new Vector2(900, 140));

        CreateUIText(mainPanel.transform, "GameSubtitle", "Niveles de Juego Dinamicos y Procedimentales", 28, FontStyle.Italic,
                     new Color(0.85f, 0.9f, 1f), new Vector2(0, 360), new Vector2(900, 80));

        GameObject playBtn = CreateUIButton(mainPanel.transform, "PlayButton", "JUGAR / INICIAR", 38,
                                            new Color(0.15f, 0.55f, 0.95f), new Vector2(0, 100), new Vector2(500, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.GetComponent<Button>().onClick, menuMgr.PlayGame);

        GameObject instrBtn = CreateUIButton(mainPanel.transform, "InstructionsButton", "COMO JUGAR / CONTROLES", 30,
                                             new Color(0.25f, 0.35f, 0.50f), new Vector2(0, -50), new Vector2(500, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(instrBtn.GetComponent<Button>().onClick, menuMgr.ShowInstructions);

        GameObject quitBtn = CreateUIButton(mainPanel.transform, "QuitButton", "SALIR", 28,
                                            new Color(0.70f, 0.20f, 0.25f), new Vector2(0, -180), new Vector2(500, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.GetComponent<Button>().onClick, menuMgr.QuitGame);

        GameObject instrPanel = CreateUIPanel(canvasObj.transform, "InstructionsPanel", new Color(0.05f, 0.08f, 0.15f, 0.95f));
        instrPanel.SetActive(false);

        CreateUIText(instrPanel.transform, "InstrTitle", "CONTROLES Y REGLAS 3D", 50, FontStyle.Bold, Color.white,
                     new Vector2(0, 480), new Vector2(900, 120));

        string instrContent = "Desplazamiento Tridimensional (X, Y, Z):\n" +
                              " - Eje X: Desplazamiento lateral (A / D o Flechas Izq / Der).\n" +
                              " - Eje Y: Rebote fisico controlado al caer.\n" +
                              " - Eje Z: Desplazamiento en profundidad (W / S o Flechas Arr / Abajo).\n" +
                              " - Salto Manual: Tecla ESPACIO o boton tactil.\n\n" +
                              "Rotacion de la Torre Helicoidal:\n" +
                              " - Arrastra el raton o desliza el dedo en pantalla.\n" +
                              " - Busca los huecos para descender de nivel.\n\n" +
                              "Mecanica de Niveles Procedimentales:\n" +
                              " - Partes de muerte rojas (DeathPart) reinician el bucle al perder.\n" +
                              " - Plataforma verde final (Goal) conduce al siguiente nivel.\n" +
                              " - Multiples etapas configurables desde ScriptableObjects.";

        CreateUIText(instrPanel.transform, "InstrBody", instrContent, 28, FontStyle.Normal, Color.white,
                     new Vector2(0, 60), new Vector2(850, 600));

        GameObject backBtn = CreateUIButton(instrPanel.transform, "BackButton", "VOLVER AL MENU", 32,
                                            new Color(0.15f, 0.55f, 0.95f), new Vector2(0, -420), new Vector2(450, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.GetComponent<Button>().onClick, menuMgr.ShowMainPanel);

        SerializedObject so = new SerializedObject(menuMgr);
        so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        so.FindProperty("instructionsPanel").objectReferenceValue = instrPanel;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1.0f, 0.98f, 0.95f);
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        Material ballMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/BallMaterial.mat");
        Material towerMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/TowerMaterial.mat");
        Material platMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlatformMaterial.mat");
        Material hazMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/HazardMaterial.mat");
        Material goalMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/GoalMaterial.mat");
        GameObject levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/HelixLevelPrefab.prefab");

        Stage stage0 = AssetDatabase.LoadAssetAtPath<Stage>($"{StagesPath}/Stage_0.asset");
        Stage stage1 = AssetDatabase.LoadAssetAtPath<Stage>($"{StagesPath}/Stage_1.asset");
        Stage stage2 = AssetDatabase.LoadAssetAtPath<Stage>($"{StagesPath}/Stage_2.asset");

        GameObject towerObj = new GameObject("HelixTower");
        towerObj.transform.position = Vector3.zero;
        HelixTowerController towerCtrl = towerObj.AddComponent<HelixTowerController>();
        HelixController helixCtrl = towerObj.AddComponent<HelixController>();

        GameObject centralCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centralCylinder.name = "CentralCylinder";
        centralCylinder.transform.SetParent(towerObj.transform);
        centralCylinder.transform.localPosition = new Vector3(0, -15.0f, 0);
        centralCylinder.transform.localScale = new Vector3(2.0f, 20.0f, 2.0f);
        if (towerMat != null) centralCylinder.GetComponent<Renderer>().sharedMaterial = towerMat;

        GameObject topObj = new GameObject("TopTransform");
        topObj.transform.SetParent(towerObj.transform);
        topObj.transform.localPosition = new Vector3(0, 3.5f, 0);

        GameObject goalObj = new GameObject("GoalTransform");
        goalObj.transform.SetParent(towerObj.transform);
        goalObj.transform.localPosition = new Vector3(0, -25.0f, 0);

        GameObject goalPlat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalPlat.name = "GoalPlatform";
        goalPlat.tag = "Goal";
        goalPlat.transform.SetParent(goalObj.transform);
        goalPlat.transform.localPosition = Vector3.zero;
        goalPlat.transform.localScale = new Vector3(5.0f, 0.25f, 5.0f);
        if (goalMat != null) goalPlat.GetComponent<Renderer>().sharedMaterial = goalMat;
        goalPlat.AddComponent<GoalController>();

        SerializedObject soHelix = new SerializedObject(helixCtrl);
        soHelix.FindProperty("topTransform").objectReferenceValue = topObj.transform;
        soHelix.FindProperty("goalTransform").objectReferenceValue = goalObj.transform;
        soHelix.FindProperty("helixLevelPrefab").objectReferenceValue = levelPrefab;

        SerializedProperty stagesProp = soHelix.FindProperty("allStages");
        stagesProp.ClearArray();
        if (stage0 != null) { stagesProp.InsertArrayElementAtIndex(0); stagesProp.GetArrayElementAtIndex(0).objectReferenceValue = stage0; }
        if (stage1 != null) { stagesProp.InsertArrayElementAtIndex(1); stagesProp.GetArrayElementAtIndex(1).objectReferenceValue = stage1; }
        if (stage2 != null) { stagesProp.InsertArrayElementAtIndex(2); stagesProp.GetArrayElementAtIndex(2).objectReferenceValue = stage2; }
        soHelix.ApplyModifiedProperties();

        GameObject ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballObj.name = "PlayerBall";
        ballObj.tag = "Player";
        ballObj.transform.position = new Vector3(0, 3.5f, 1.7f);
        ballObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        if (ballMat != null) ballObj.GetComponent<Renderer>().sharedMaterial = ballMat;
        PlayerBallController ballCtrl = ballObj.AddComponent<PlayerBallController>();

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.16f, 0.24f);
        camObj.tag = "MainCamera";
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

        CreateEventSystem();

        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameHUDManager hudMgr = canvasObj.AddComponent<GameHUDManager>();

        GameObject hudPanel = CreateUIPanel(canvasObj.transform, "HUDPanel", new Color(0, 0, 0, 0));

        Text coordsText = CreateUIText(hudPanel.transform, "TelemetryCoordinatesText",
                                     "Posicion 3D:\nX: +0.00  |  Y: +0.00  |  Z: +0.00",
                                     24, FontStyle.Bold, Color.white,
                                     new Vector2(-240, 840), new Vector2(550, 140), TextAnchor.UpperLeft);

        Text scoreText = CreateUIText(hudPanel.transform, "ScoreText", "Puntos: 0", 34, FontStyle.Bold,
                                    new Color(1f, 0.85f, 0.2f), new Vector2(250, 840), new Vector2(350, 80), TextAnchor.UpperRight);

        GameObject pauseBtn = CreateUIButton(hudPanel.transform, "PauseButton", "PAUSA", 24,
                                             new Color(0.2f, 0.25f, 0.35f, 0.9f), new Vector2(440, 840), new Vector2(140, 70));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pauseBtn.GetComponent<Button>().onClick, hudMgr.TogglePause);

        GameObject controlsPanel = CreateUIPanel(hudPanel.transform, "MobileControlsPanel", new Color(0, 0, 0, 0));

        GameObject leftBtn = CreateUIButton(controlsPanel.transform, "BtnLeftX", "<", 48,
                                            new Color(0.2f, 0.5f, 0.8f, 0.65f), new Vector2(-420, -780), new Vector2(160, 140));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(leftBtn.GetComponent<Button>().onClick, hudMgr.OnTouchLeft);

        GameObject rightBtn = CreateUIButton(controlsPanel.transform, "BtnRightX", ">", 48,
                                             new Color(0.2f, 0.5f, 0.8f, 0.65f), new Vector2(-240, -780), new Vector2(160, 140));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(rightBtn.GetComponent<Button>().onClick, hudMgr.OnTouchRight);

        GameObject jumpBtn = CreateUIButton(controlsPanel.transform, "BtnManualJump", "SALTO", 26,
                                            new Color(0.85f, 0.5f, 0.1f, 0.75f), new Vector2(0, -780), new Vector2(200, 140));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(jumpBtn.GetComponent<Button>().onClick, hudMgr.OnTouchJump);

        GameObject rotLBtn = CreateUIButton(controlsPanel.transform, "BtnRotateTowerLeft", "<<", 36,
                                            new Color(0.3f, 0.35f, 0.45f, 0.65f), new Vector2(240, -780), new Vector2(140, 140));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(rotLBtn.GetComponent<Button>().onClick, hudMgr.OnRotateTowerLeft);

        GameObject rotRBtn = CreateUIButton(controlsPanel.transform, "BtnRotateTowerRight", ">>", 36,
                                            new Color(0.3f, 0.35f, 0.45f, 0.65f), new Vector2(400, -780), new Vector2(140, 140));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(rotRBtn.GetComponent<Button>().onClick, hudMgr.OnRotateTowerRight);

        GameObject victoryPanel = CreateUIPanel(canvasObj.transform, "VictoryPanel", new Color(0.05f, 0.18f, 0.10f, 0.95f));
        victoryPanel.SetActive(false);
        CreateUIText(victoryPanel.transform, "VicTitle", "NIVEL COMPLETADO!", 56, FontStyle.Bold,
                     new Color(0.3f, 0.95f, 0.4f), new Vector2(0, 200), new Vector2(800, 120));

        GameObject nextLvlBtn = CreateUIButton(victoryPanel.transform, "NextLevelButton", "SIGUIENTE NIVEL", 34,
                                               new Color(0.15f, 0.75f, 0.35f), new Vector2(0, -50), new Vector2(460, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextLvlBtn.GetComponent<Button>().onClick, hudMgr.RestartLevel);

        GameObject vMenuBtn = CreateUIButton(victoryPanel.transform, "VictoryMenuButton", "MENU PRINCIPAL", 28,
                                             new Color(0.25f, 0.35f, 0.45f), new Vector2(0, -180), new Vector2(420, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(vMenuBtn.GetComponent<Button>().onClick, hudMgr.ReturnToMainMenu);

        GameObject gameOverPanel = CreateUIPanel(canvasObj.transform, "GameOverPanel", new Color(0.18f, 0.05f, 0.05f, 0.95f));
        gameOverPanel.SetActive(false);
        CreateUIText(gameOverPanel.transform, "GOTitle", "FIN DE LA PARTIDA", 56, FontStyle.Bold,
                     new Color(0.95f, 0.25f, 0.25f), new Vector2(0, 200), new Vector2(800, 120));

        GameObject retryBtn = CreateUIButton(gameOverPanel.transform, "RetryButton", "REINTENTAR", 34,
                                             new Color(0.85f, 0.2f, 0.25f), new Vector2(0, -50), new Vector2(440, 110));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.GetComponent<Button>().onClick, hudMgr.RestartLevel);

        GameObject goMenuBtn = CreateUIButton(gameOverPanel.transform, "GOMenuButton", "MENU PRINCIPAL", 28,
                                              new Color(0.25f, 0.35f, 0.45f), new Vector2(0, -180), new Vector2(420, 90));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.GetComponent<Button>().onClick, hudMgr.ReturnToMainMenu);

        SerializedObject soHud = new SerializedObject(hudMgr);
        soHud.FindProperty("player").objectReferenceValue = ballCtrl;
        soHud.FindProperty("tower").objectReferenceValue = towerCtrl;
        soHud.FindProperty("coordinatesText").objectReferenceValue = coordsText;
        soHud.FindProperty("scoreText").objectReferenceValue = scoreText;
        soHud.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        soHud.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        soHud.FindProperty("hudPanel").objectReferenceValue = hudPanel;
        soHud.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }



    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[2];
        scenes[0] = new EditorBuildSettingsScene(MainMenuScenePath, true);
        scenes[1] = new EditorBuildSettingsScene(GameScenePath, true);
        EditorBuildSettings.scenes = scenes;
    }

    private static void CreateEventSystem()
    {
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
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

        CreateUIText(btnObj.transform, "Text", textContent, fontSize, FontStyle.Bold, Color.white,
                     Vector2.zero, sizeDelta, TextAnchor.MiddleCenter);

        return btnObj;
    }
}
