using UnityEngine;

public class HealthTest : MonoBehaviour, IDamageable
{
    public int health = 100;

    // FIX: returns true if this hit killed the target
    public bool TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{name} took {amount} damage. HP: {health}");

        if (health <= 0)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}