﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    public GameObject emoticonEntity;
    private Animator _animator;
    private Image _image;

    int pid;
    GameObject toSeat;

    void Awake() {
        _animator = emoticonEntity.GetComponent<Animator>();
        _image = emoticonEntity.GetComponent<Image>();
    }

    public void Init(GameObject fromSeat, GameObject toSeat, int pid, bool isToMe) 
    {
        this.pid = pid;
        this.toSeat = toSeat;

        transform.SetParent(G.UICvs.transform, false);
        transform.position = fromSeat.GetComponent<RectTransform>().position;

        if (toSeat.GetComponent<Seat>().GetPos() == SeatPosition.Right)
            transform.localScale = new Vector3(-1, 1, 0);
        

        Vector3 aimSeat = toSeat.transform.position;

        if (isToMe) 
        {
            aimSeat -= new Vector3(0,80,0);
        }

        Tween tween = transform.DOMove(aimSeat, 0.6f).SetEase(Ease.OutQuad);
        tween.Play();
        _animator.SetTrigger(pid.ToString());

    }
}
