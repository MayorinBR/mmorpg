using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChibiHairExample
{
    public class PagerPanel : MonoBehaviour
    {
        [SerializeField]
        private int page = 1;

        [SerializeField]
        private Text pageText;

        private int numPages = 4;

        [SerializeField]
        private GameObject[] pagePanels;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private Button previousButton;

        // Start is called before the first frame update
        void Start()
        {
            //the first child is excluded because it holds the next and previous buttons and is not a real page
            numPages = gameObject.transform.childCount - 1;

            pagePanels = new GameObject[numPages];
            for (int i = 1; i < gameObject.transform.childCount; i++)
            {
                pagePanels[i - 1] = gameObject.transform.GetChild(i).gameObject;
            }

            pageChanged();
        }

        public void nextPage()
        {
            page += 1;
            pageChanged();

        }
        public void previousPage()
        {
            page -= 1;
            pageChanged();
        }


        private void pageChanged()
        {
            pageText.text = page + "/" + numPages;
            for (int i = 0; i < pagePanels.Length; i++)
            {
                pagePanels[i].SetActive(false);
            }

            pagePanels[page - 1].SetActive(true);

            if (page <= 1)
            {
                previousButton.interactable = false;
            }
            else
            {
                previousButton.interactable = true;
            }

            if (page >= numPages)
            {
                nextButton.interactable = false;
            }
            else
            {
                nextButton.interactable = true;
            }

        }
    }
}
