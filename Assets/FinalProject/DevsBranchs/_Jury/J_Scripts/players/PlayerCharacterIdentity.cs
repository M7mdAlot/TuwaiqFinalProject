using UnityEngine;
using Aegis.Core;

public class PlayerCharacterIdentity : MonoBehaviour
{
    public enum PlayerType
    {
        Aegis,
        X
    }

    public PlayerType playerType = PlayerType.Aegis;

    // Bridge only: lets Mohammed's core (Aegis.Core.Side / CampaignConfig) read
    // the same identity this component already tracks, without changing playerType.
    public Side Side => playerType == PlayerType.Aegis ? Side.Good : Side.Evil;
}