using System.Collections.Generic;
using UnityEngine;

public class HelixController : MonoBehaviour
{
    private Vector2 lastTapPosition;
    private Vector3 startRotation;

    public Transform topTransform;
    public Transform goalTransform;
    public GameObject helixLevelPrefab;

    public List<Stage> allStages = new List<Stage>();
    public float helixDistance;
    private List<GameObject> spawnedLevels = new List<GameObject>();

    private void Awake()
    {
        startRotation = transform.localEulerAngles;
        if (topTransform != null && goalTransform != null)
        {
            helixDistance = topTransform.localPosition.y - (goalTransform.localPosition.y + 0.1f);
        }
        if (allStages.Count > 0)
        {
            LoadStage(0);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 curTapPos = Input.mousePosition;
            if (lastTapPosition == Vector2.zero)
            {
                lastTapPosition = curTapPos;
            }

            float dis = lastTapPosition.x - curTapPos.x;
            lastTapPosition = curTapPos;

            transform.Rotate(Vector3.up * dis);
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastTapPosition = Vector2.zero;
        }
    }

    public void LoadStage(int stageNumber)
    {
        if (allStages.Count == 0) return;
        stageNumber = Mathf.Clamp(stageNumber, 0, allStages.Count - 1);

        Stage stage = allStages[stageNumber];
        if (stage == null)
        {
            Debug.Log("No stage");
            return;
        }

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = stage.stageBackgroundColor;
        }

        RenderSettings.ambientLight = stage.stageBackgroundColor;
        RenderSettings.ambientSkyColor = stage.stageBackgroundColor;

        Light dirLight = RenderSettings.sun;
        if (dirLight == null)
        {
            dirLight = FindAnyObjectByType<Light>();
        }
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.color = Color.Lerp(Color.white, stage.stageBackgroundColor, 0.2f);
        }

        PlayerBallController ball = FindAnyObjectByType<PlayerBallController>();
        if (ball != null)
        {
            ball.ApplyVisualCustomization(stage.stageBallColor);
        }

        transform.localEulerAngles = startRotation;

        foreach (GameObject go in spawnedLevels)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        spawnedLevels.Clear();

        float levelDistance = helixDistance / Mathf.Max(1, stage.levels.Count);
        float spawnPositionY = topTransform != null ? topTransform.localPosition.y : 0f;

        for (int i = 0; i < stage.levels.Count; i++)
        {
            spawnPositionY -= levelDistance;
            GameObject level = Instantiate(helixLevelPrefab, transform);
            level.transform.localPosition = new Vector3(0, spawnPositionY, 0);
            spawnedLevels.Add(level);

            int partsToDisable = 12 - stage.levels[i].partCount;
            List<GameObject> disabledParts = new List<GameObject>();

            while (disabledParts.Count < partsToDisable && disabledParts.Count < level.transform.childCount)
            {
                int randomIndex = Random.Range(0, level.transform.childCount);
                GameObject randomPart = level.transform.GetChild(randomIndex).gameObject;
                if (!disabledParts.Contains(randomPart))
                {
                    randomPart.SetActive(false);
                    disabledParts.Add(randomPart);
                }
            }

            List<GameObject> leftParts = new List<GameObject>();
            foreach (Transform t in level.transform)
            {
                if (t.gameObject.activeInHierarchy)
                {
                    Renderer rend = t.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material.color = stage.stageLevelPartColor;
                        if (rend.material.HasProperty("_BaseColor"))
                        {
                            rend.material.SetColor("_BaseColor", stage.stageLevelPartColor);
                        }
                    }
                    leftParts.Add(t.gameObject);
                }
            }

            List<GameObject> deathParts = new List<GameObject>();
            int targetDeathParts = Mathf.Clamp(stage.levels[i].deathPartCount, 0, leftParts.Count);
            while (deathParts.Count < targetDeathParts && leftParts.Count > 0)
            {
                int randomIndex = Random.Range(0, leftParts.Count);
                GameObject randomPart = leftParts[randomIndex];
                if (!deathParts.Contains(randomPart))
                {
                    randomPart.AddComponent<DeathPart>();
                    deathParts.Add(randomPart);
                }
            }
        }
    }
}
