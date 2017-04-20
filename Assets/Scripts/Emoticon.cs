﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    public GameObject emoticonEntity;
    private Animator _animator;
    private RectTransform _rectTransform;
    private Image _image;

    int pid;
    GameObject toSeat;

    void Awake() {
        _animator = emoticonEntity.GetComponent<Animator>();
        _image = emoticonEntity.GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Init(GameObject fromSeat, GameObject toSeat, int pid, bool isToMe) 
    {
        this.pid = pid;
        this.toSeat = toSeat;

        transform.SetParent(G.UICvs.transform, false);
        _rectTransform.anchoredPosition = fromSeat.GetComponent<RectTransform>().anchoredPosition;

        if (toSeat.GetComponent<Seat>().GetPos() == SeatPosition.Right)
            transform.localScale = new Vector3(-1, 1, 0);


        Vector2 aimSeat = toSeat.transform.localPosition + new Vector3(0, 60, 0);

        if (isToMe) 
        {
            aimSeat -= new Vector2(0,165);
        }

        Tween tween = _rectTransform.DOLocalMove(aimSeat, 0.6f).SetEase(Ease.OutQuad);
        tween.Play();
        _animator.SetTrigger(pid.ToString());

    }
}
