using UnityEngine;

public class HelixLevelBuilder : MonoBehaviour
{
    [SerializeField] private float towerHeight = 35.0f;
    [SerializeField] private float towerRadius = 1.0f;
    [SerializeField] private Material towerMaterial;
    [SerializeField] private int numberOfRings = 7;
    [SerializeField] private float distanceBetweenRings = 4.0f;
    [SerializeField] private float ringOuterRadius = 2.4f;
    [SerializeField] private Material platformMaterial;
    [SerializeField] private Material hazardMaterial;
    [SerializeField] private Material goalMaterial;
    [SerializeField] private bool generateOnStart = true;

    void Start()
    {
        if (generateOnStart)
        {
            BuildHelixTower();
        }
    }

    [ContextMenu("Construir Torre Helix")]
    public void BuildHelixTower()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        int level = GameManager.singleton != null ? GameManager.singleton.currentLevel : 1;
        int activeRings = Mathf.Clamp(numberOfRings + (level - 1), 6, 20);
        float totalHeight = Mathf.Max(towerHeight, activeRings * distanceBetweenRings + 10.0f);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "CentralCylinder";
        cylinder.transform.SetParent(transform);
        cylinder.transform.localPosition = new Vector3(0, -totalHeight / 2f + 2f, 0);
        cylinder.transform.localScale = new Vector3(towerRadius * 2f, totalHeight / 2f, towerRadius * 2f);

        if (towerMaterial != null)
        {
            cylinder.GetComponent<Renderer>().sharedMaterial = towerMaterial;
        }

        for (int r = 0; r < activeRings; r++)
        {
            float ringY = -r * distanceBetweenRings;
            bool isGoal = (r == activeRings - 1);

            GameObject ringObj = new GameObject($"Ring_Level_{r}");
            ringObj.transform.SetParent(transform);
            ringObj.transform.localPosition = new Vector3(0, ringY, 0);

            int gapStart = (r * 3 + 1) % 12;
            int gapSize = isGoal ? 0 : Mathf.Max(1, 4 - (level / 4));

            if (!isGoal)
            {
                GameObject passObj = new GameObject("PassTrigger");
                passObj.transform.SetParent(ringObj.transform);
                passObj.transform.localPosition = Vector3.zero;
                BoxCollider passCol = passObj.AddComponent<BoxCollider>();
                passCol.isTrigger = true;
                passCol.size = new Vector3(ringOuterRadius * 2f, 0.5f, ringOuterRadius * 2f);
                passObj.AddComponent<PassTrigger>();
            }

            for (int s = 0; s < 12; s++)
            {
                bool isGap = false;
                for (int g = 0; g < gapSize; g++)
                {
                    if (s == (gapStart + g) % 12) { isGap = true; break; }
                }

                if (isGap) continue;

                float angle = s * 30.0f;
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"Segment_{s}_{angle}deg";
                segment.transform.SetParent(ringObj.transform);

                float rad = angle * Mathf.Deg2Rad;
                float segDist = (towerRadius + ringOuterRadius) / 2f;
                float segWidth = (ringOuterRadius - towerRadius);
                float segLength = Mathf.Tan(15.0f * Mathf.Deg2Rad) * segDist * 2.1f;

                segment.transform.localPosition = new Vector3(
                    Mathf.Sin(rad) * segDist,
                    0,
                    Mathf.Cos(rad) * segDist
                );
                segment.transform.localRotation = Quaternion.Euler(0, angle, 0);
                segment.transform.localScale = new Vector3(segLength, 0.25f, segWidth);

                if (isGoal)
                {
                    segment.tag = "Goal";
                    if (goalMaterial != null) segment.GetComponent<Renderer>().sharedMaterial = goalMaterial;
                    segment.AddComponent<GoalController>();
                }
                else
                {
                    bool isHazard = (s == (gapStart + 4) % 12 && r > 1) || (level >= 3 && s == (gapStart + 7) % 12 && r > 2);
                    if (isHazard)
                    {
                        segment.tag = "Hazard";
                        if (hazardMaterial != null) segment.GetComponent<Renderer>().sharedMaterial = hazardMaterial;
                    }
                    else
                    {
                        segment.tag = "Untagged";
                        if (platformMaterial != null) segment.GetComponent<Renderer>().sharedMaterial = platformMaterial;
                    }
                }
            }
        }
    }
}
