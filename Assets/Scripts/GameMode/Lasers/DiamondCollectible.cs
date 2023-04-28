using UnityEngine;

namespace GameMode.Lasers
{
    public class DiamondCollectible : MonoBehaviour
    {
        #region Serialized Fields
        
        [Tooltip("how much points the player gets for collecting that diamond")]
        [SerializeField] private int diamondValue;

        [SerializeField] public bool isBig = false;
        
        #endregion

    }
}