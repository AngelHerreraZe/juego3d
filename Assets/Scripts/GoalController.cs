using UnityEngine;

public class GoalController : MonoBehaviour
{
    private bool reached = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (reached) return;

        if (collision.gameObject.GetComponent<PlayerBallController>() != null || collision.gameObject.CompareTag("Player"))
        {
            reached = true;
            Debug.Log("Pasamos de nivel");
            if (GameManager.singleton != null)
            {
                GameManager.singleton.NextLevel();
            }
        }
    }
}
