public enum DamageType
{
    Physical, Electric
}

public interface IDamageable
{
    void TakeDamage(int amount, DamageType damageType = DamageType.Physical);
}