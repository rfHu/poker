﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EmoticonEvent : MonoBehaviour {

    public void Destroythis() {
        PoolMan.Despawn(transform);
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

    public void DaggerSound() 
    {
        G.PlaySound("mf_dagger");
    }

    public void DogSound() 
    {
        G.PlaySound("mf_dog");
    }

    public void GhostSound()
    {
        G.PlaySound("mf_ghost");
    }

    public void KissSound()
    {
        G.PlaySound("mf_kiss");
    }

    public void ToujiSound()
    {
        G.PlaySound("mf_touji");
    }

    public void SharkEmo2() 
    {
        PoolMan.Spawn("Emoticon/Emo15_Shark").SetParent(G.UICvs.transform, false);
    }

    public void GanbeiSound()
    {
        G.PlaySound("mf_ganbei");
    }

    public void DiaoyuSound()
    {
        G.PlaySound("mf_diaoyu");
    }

    public void DianzanSound()
    {
        G.PlaySound("mf_dianzan");
    }

    public void ShitSound()
    {
        G.PlaySound("mf_shit");
    }

    public void SharkSound()
    {
        G.PlaySound("mf_shark");
    }

    public void win27Sound() 
    {
        G.PlaySound("win27Sound");
    }
}
