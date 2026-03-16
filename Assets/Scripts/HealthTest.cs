using UnityEngine;

public class HealthTest : MonoBehaviour, IDamageable
{
    public int health = 100;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{name} took {amount} damage. HP: {health}");

        if (health <= 0)
            Destroy(gameObject);
    }
}