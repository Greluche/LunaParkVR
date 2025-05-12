using UnityEngine;

public class Jail : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        DuckScore.score += 1;
    }
}
