using UnityEngine;

public class PassScorePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.singleton != null)
        {
            GameManager.singleton.AddScore(1);
        }
    }
}
