using UnityEngine;

public class PassTrigger : MonoBehaviour
{
    private bool passed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (passed) return;

        if (other.GetComponent<PlayerBallController>() != null || other.CompareTag("Player"))
        {
            passed = true;
            if (GameManager.singleton != null)
            {
                GameManager.singleton.AddScore(2);
            }
        }
    }
}
