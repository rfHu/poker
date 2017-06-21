using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkTonic.MasterAudio;
using DG.Tweening;

public class EmoticonEvent : MonoBehaviour {


    public GameObject laberPart2;
    public RectTransform[] ice;

    int times = 0;

    float[] posX = { -120, 29, 132 };

    public void Destroy()
    {
        GameObject.Destroy(transform.parent.gameObject);
    }



    public void Destroythis() {
        GameObject.Destroy(gameObject);
    }

    public void LabberPart2() 
    {
        laberPart2.SetActive(true);
        for (int i = 0; i < ice.Length; i++)
		{
            ice[i].DOLocalMoveX(posX[i], 0.3f).SetEase(Ease.Linear);
            ice[i].DOLocalMoveY(-95, 0.3f).SetEase(Ease.OutBounce);
	    }
    }

    public void LabberSound() 
    {
        G.PlaySound("mf_lengjing");
    }

    public void TomatoSound()
    {
        G.PlaySound("mf_xihongshi");
    }

    public void FlowerSound()
    {
        G.PlaySound("mf_meigui");
    }

    public void CatchSound()
    {
        G.PlaySound("mf_zhuoji");
    }

    public void BoomSound()
    {
        G.PlaySound("mf_zhadan");
    }
}
