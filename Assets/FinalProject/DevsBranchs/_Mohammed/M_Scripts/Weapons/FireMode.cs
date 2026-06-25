namespace Aegis.Weapons
{
    /// <summary>How a weapon fires (GDD §9).</summary>
    public enum FireMode
    {
        Single, // one shot per press
        Auto,   // rapid-fire while held (e.g. Arc Lance)
        Charge, // hold to charge, release to fire (e.g. Charge Cannon)
        Beam    // continuous beam
    }
}
