using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChibiHairExample
{

    public class HairSelector : MonoBehaviour
    {
        [SerializeField]
        private GameObject hairObj;

        [SerializeField]
        private Mesh[] hairMeshes;

        [SerializeField]
        private Color hairColor = Color.white;

        // Start is called before the first frame update
        void Start()
        {

        }

        /// <summary>
        /// Called from the UI buttons when a hair is selected. Changes the hair mesh.
        /// </summary>
        /// <param name="hairIndex">The hair. Starts at 0</param>
        public void selectHair(int hairIndex)
        {
            //if the hair selected is out of range of the possible hair shapes, then it is bald
            if (hairIndex >= hairMeshes.Length)
            {
                hairObj.SetActive(false);
            }
            else
            {
                hairObj.SetActive(true);
                hairObj.GetComponent<MeshFilter>().mesh = hairMeshes[hairIndex];
            }
        }

        public void changeHairColor(Color newColor)
        {
            hairColor = newColor;
            hairObj.GetComponent<MeshRenderer>().material.color = newColor;
        }

        public Color getHairColor()
        {
            return hairColor;
        }


    }
}
