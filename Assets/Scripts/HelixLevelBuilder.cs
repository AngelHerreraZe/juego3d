using UnityEngine;

/// <summary>
/// Generador procedimental de la torre Helix Jump y sus niveles de plataformas.
/// Basado directamente en el procedimiento del tutorial QYUgb5pHi9c:
/// 1. Cilindro central (Tower / Helix) con colisionador y material.
/// 2. Anillos helicoidales espaciados verticalmente cada N unidades.
/// 3. Cada anillo se compone de sectores distribuidos en múltiplos de 30 grados (0° a 330°).
/// 4. Se generan huecos (gaps) controlados para permitir el paso de la esfera.
/// 5. El último nivel es la plataforma de meta (Goal).
/// </summary>
public class HelixLevelBuilder : MonoBehaviour
{
    [Header("Configuración de la Torre Central")]
    [SerializeField] private float towerHeight = 35.0f;
    [SerializeField] private float towerRadius = 1.0f;
    [SerializeField] private Material towerMaterial;

    [Header("Configuración de Plataformas")]
    [SerializeField] private int numberOfRings = 7;
    [SerializeField] private float distanceBetweenRings = 4.0f;
    [SerializeField] private float ringOuterRadius = 2.4f;
    [SerializeField] private Material platformMaterial;
    [SerializeField] private Material hazardMaterial;
    [SerializeField] private Material goalMaterial;

    [Header("Generación al Iniciar")]
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
        // Limpiar hijos existentes si ya se había construido
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 1. Crear el cilindro central de la torre
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "CentralCylinder";
        cylinder.transform.SetParent(transform);
        cylinder.transform.localPosition = new Vector3(0, -towerHeight / 2f + 2f, 0);
        cylinder.transform.localScale = new Vector3(towerRadius * 2f, towerHeight / 2f, towerRadius * 2f);

        if (towerMaterial != null)
        {
            cylinder.GetComponent<Renderer>().sharedMaterial = towerMaterial;
        }

        // 2. Construir cada uno de los niveles / anillos
        for (int r = 0; r < numberOfRings; r++)
        {
            float ringY = -r * distanceBetweenRings;
            bool isGoal = (r == numberOfRings - 1);

            GameObject ringObj = new GameObject($"Ring_Level_{r}");
            ringObj.transform.SetParent(transform);
            ringObj.transform.localPosition = new Vector3(0, ringY, 0);

            // Cada anillo tiene 12 sectores de 30 grados
            // Definimos qué sectores serán huecos (gaps) para que la bola caiga
            int gapStart = (r * 3 + 1) % 12;
            int gapSize = isGoal ? 0 : 3; // El nivel final no tiene hueco para ganar

            for (int s = 0; s < 12; s++)
            {
                // Si el sector está dentro del rango del hueco, no se genera plataforma
                bool isGap = false;
                for (int g = 0; g < gapSize; g++)
                {
                    if (s == (gapStart + g) % 12) { isGap = true; break; }
                }

                if (isGap) continue;

                // Crear segmento de plataforma utilizando una caja escalada y rotada
                float angle = s * 30.0f;
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"Segment_{s}_{angle}deg";
                segment.transform.SetParent(ringObj.transform);

                // Posicionamiento radial alrededor del cilindro central
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

                // Asignar etiquetas y materiales
                if (isGoal)
                {
                    segment.tag = "Goal";
                    if (goalMaterial != null) segment.GetComponent<Renderer>().sharedMaterial = goalMaterial;
                }
                else
                {
                    // Cada anillo puede tener 1 sector de peligro (Hazard)
                    bool isHazard = (s == (gapStart + 4) % 12 && r > 1);
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

        Debug.Log($"Torre Helix generada exitosamente con {numberOfRings} niveles.");
    }
}
