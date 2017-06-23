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


    public void Destroy()
    {
        PoolMan.Despawn(transform.parent);
    }



    public void Destroythis() {
        PoolMan.Despawn(transform);
    }

    public void LabberPart2() 
    {
        float[] posX = { -120, 29, 132 };
        for (int i = 0; i < ice.Length; i++)
		{
            ice[i].DOLocalMoveX(posX[i], 0.3f).SetEase(Ease.Linear);
            ice[i].DOLocalMoveY(-95, 0.3f).SetEase(Ease.OutBounce);
	    }
    }

    public void ResetLabberP2()
    {
        float[] posXR = { -20, -5, 30 };
        for (int i = 0; i < ice.Length; i++)
        {
            ice[i].GetComponent<RectTransform>().localPosition = new Vector2(posXR[i], 100);
        }
        laberPart2.SetActive(false);
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
