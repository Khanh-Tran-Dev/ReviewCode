using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurZom.Core
{
    public class MainLabel : MonoBehaviour
    {
        [SerializeField] public List<GameObject> labelList;
        int currentLabel = 0;
        void Start()
        {
            labelList[currentLabel].SetActive(true);
        }

        public void NextLabel()
        {
            labelList[currentLabel].SetActive(false);
            currentLabel++;
            if (currentLabel >= labelList.Count)
            {
                currentLabel = 0;
            }
            labelList[currentLabel].SetActive(true);
        }

        public void PreviousLabel()
        {
            labelList[currentLabel].SetActive(false);
            currentLabel--;
            if (currentLabel < 0)
            {
                currentLabel = labelList.Count - 1;
            }
            labelList[currentLabel].SetActive(true);
        }
    }
}

