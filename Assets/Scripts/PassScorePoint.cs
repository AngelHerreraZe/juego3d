using UnityEngine;

public class PassScorePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerBallController ball = other.GetComponent<PlayerBallController>();
        if (ball != null)
        {
            ball.perfectPass++;
        }

        if (GameManager.singleton != null)
        {
            GameManager.singleton.AddScore(1);
        }
    }
}
