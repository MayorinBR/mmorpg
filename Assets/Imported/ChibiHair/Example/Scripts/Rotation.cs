using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChibiHairExample
{
    public class Rotation : MonoBehaviour
    {
        private Vector2 mouseStartPos;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseStartPos = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                gameObject.transform.Rotate(new Vector3(0, mouseStartPos.x - Input.mousePosition.x, 0));
                mouseStartPos = Input.mousePosition;
            }

        }
    }
}