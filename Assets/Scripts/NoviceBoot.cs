using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoviceBoot : MonoBehaviour {

    public GameObject[] Staps;

    int clickTimes = 0;

    public void OnClick() 
    {
        clickTimes++;
        if (clickTimes > 3)
        {
            Destroy(gameObject);
        }
        else
        {
            Staps[clickTimes].SetActive(true);
            Staps[clickTimes - 1].SetActive(false);
        }
    }
}
