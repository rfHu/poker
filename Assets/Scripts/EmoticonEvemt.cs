using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkTonic.MasterAudio;

public class EmoticonEvemt : MonoBehaviour {

    public GameObject tomatoPart2;

    public void Destroy()
    {
        GameObject.Destroy(transform.parent.gameObject);
    }


    public void Destroythis() {
        GameObject.Destroy(gameObject);
    }

    public void TomatoPart2() 
    {
        tomatoPart2.SetActive(true);
    }

    public void LabberSound() 
    {
        MasterAudio.PlaySound("mf_lengjing");
    }

    public void TomatoSound()
    {
        MasterAudio.PlaySound("mf_xihongshi");
    }

    public void FlowerSound()
    {
        MasterAudio.PlaySound("mf_meigui");
    }

    public void CatchSound()
    {
        MasterAudio.PlaySound("mf_zhuoji");
    }

    public void BoomSound()
    {
        MasterAudio.PlaySound("mf_zhadan");
    }
}
