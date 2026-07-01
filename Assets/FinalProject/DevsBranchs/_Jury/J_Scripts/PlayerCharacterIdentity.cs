using UnityEngine;

public class PlayerCharacterIdentity : MonoBehaviour
{
    public enum PlayerType
    {
        Aegis,
        X
    }

    public PlayerType playerType = PlayerType.Aegis;
}