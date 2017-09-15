using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoviceBoot : MonoBehaviour {

    public GameObject[] Steps;

    int clickTimes = 0;

    public void OnClick() 
    {
        clickTimes++;
        if (clickTimes > Steps.Length-1)
        {
            Destroy(gameObject);
        }
        else
        {
            Steps[clickTimes].SetActive(true);
            Steps[clickTimes - 1].SetActive(false);
        }
    }
}
