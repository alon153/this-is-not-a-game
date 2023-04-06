using System;
using System.Collections;
using System.Collections.Generic;
using Basics.Player;
using UnityEngine;
using UnityEngine.Events;

namespace GameMode.Pool
{
    public class PoolHole : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Border Control")]
        [SerializeField] private List<GameObject> holeBordersColliders = new List<GameObject>();

        [SerializeField] private bool topBorderActive = false;

        [SerializeField] private bool bottomBorderActive = false;

        [SerializeField] private bool leftBorderActive = false;

        [SerializeField] private bool rightBorderActive = false;

        [SerializeField] private UnityEvent<PlayerController> onFallIntoHallEvent;


        #endregion

        #region Non Serlized Fields

        private enum Borders {Top, Bottom, Left, Right}

        #endregion
        
        #region MonoBehaviour Methods

        // Start is called before the first frame update
        void Start()
        {
            SetUpBorders();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                // player falls inside the hole, will probably change later
                onFallIntoHallEvent.Invoke(playerController);
            }
        }

        #endregion

        #region Private Methods

        private void SetUpBorders()
        {
            holeBordersColliders[(int) Borders.Top].SetActive(topBorderActive);
            holeBordersColliders[(int) Borders.Bottom].SetActive(bottomBorderActive);
            holeBordersColliders[(int) Borders.Left].SetActive(leftBorderActive);
            holeBordersColliders[(int) Borders.Right].SetActive(rightBorderActive);
        }
        
        #endregion

        #region Public Methods

        #endregion
    }
    
}
