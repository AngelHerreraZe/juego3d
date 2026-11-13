using UnityEngine;

public class DeathPart : MonoBehaviour
{
    private void OnEnable()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.red;
            if (rend.material.HasProperty("_BaseColor"))
            {
                rend.material.SetColor("_BaseColor", Color.red);
            }
        }
    }

    public void HitDeathPart()
    {
        if (GameManager.singleton != null)
        {
            GameManager.singleton.RestartLevel();
        }
    }
}
