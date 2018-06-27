using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.AdventureGame
{
    public class Triggerable : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }



        public void OnTriggered()
        {
            Debug.Log("Area Triggered");
        }
    }
}