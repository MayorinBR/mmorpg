using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChibiHairExample
{

    public class ColorButton : MonoBehaviour
    {
        [SerializeField]
        private HairSelector hairSelector;

        private Image myImage;


        // Start is called before the first frame update
        void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(changeColor);
            myImage = gameObject.GetComponent<Image>();

        }


        public void changeColor()
        {
            hairSelector.changeHairColor(myImage.color);
        }

    }
}