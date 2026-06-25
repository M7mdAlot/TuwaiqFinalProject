namespace Aegis.Core
{
    /// <summary>
    /// A contract for anything that can take damage (player, enemies, destructibles).
    /// A "contract" means: any script that says "I am IDamageable" promises to provide
    /// these members, so weapons can damage it without knowing what it actually is.
    /// Tier 0 — no dependencies.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        /// <summary>Apply <paramref name="amount"/> of damage to this thing.</summary>
        void TakeDamage(float amount);
    }
}
