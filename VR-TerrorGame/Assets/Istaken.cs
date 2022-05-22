using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public class Istaken : MonoBehaviour
    {

        public InteractiveItem linterna;
        
        public GameObject trigger;
        bool hasLinterna;
        
        void Start()
        {
           hasLinterna = false;
        }

        void Update()
        {
            if(linterna.itemTaken == true && hasLinterna == false)
            {
                hasLinterna = true;
                trigger.SetActive(true);
            }

        }

        

    }
}


