public interface IDamageable
{
    // returns true if this hit killed the target
    bool TakeDamage(int amount);
}