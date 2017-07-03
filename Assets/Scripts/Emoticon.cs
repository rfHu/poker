﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    void Awake() {
    }

    public void Init(Vector2 fromSeat, GameObject toSeat, bool isToMe) 
    {
        var _rectTransform = GetComponent<RectTransform>();
        //transform.SetParent(G.UICvs.transform, false);
        _rectTransform.anchoredPosition = fromSeat;

        if (toSeat.GetComponent<Seat>().GetPos() == SeatPosition.Left)
        {
            transform.localScale = new Vector3(-1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1);
        }

        Vector2 aimSeat = toSeat.transform.localPosition + new Vector3(0, 65, 0);

        if (isToMe) 
        {
            aimSeat += new Vector2(195,-250);
        }

        Tween tween = _rectTransform.DOLocalMove(aimSeat, 0.6f).SetEase(Ease.OutQuad);
        tween.Play();
    }
}
