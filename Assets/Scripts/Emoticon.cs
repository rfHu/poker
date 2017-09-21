using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    public void Init(Vector2 fromSeat, GameObject toSeat, bool isToMe, bool canTurn = true) 
    {
        transform.SetParent(G.UICvs.transform, false);

        var _rectTransform = GetComponent<RectTransform>();
        _rectTransform.anchoredPosition = fromSeat;

        Vector2 aimSeat = toSeat.transform.localPosition + new Vector3(0, 65, 0);

        if (isToMe) 
        {
            aimSeat += new Vector2(195,-250);
        }

        Tween tween = _rectTransform.DOLocalMove(aimSeat, 0.6f).SetEase(Ease.OutQuad);
        tween.Play();

        _rectTransform.localScale = Vector3.zero;

        Vector3 finalScale = (toSeat.GetComponent<Seat>().GetPos() == SeatPosition.Left && canTurn) ? new Vector3(-1, 1) : new Vector3(1, 1);

        Tween scaleTween = _rectTransform.DOScale(finalScale, 0.6f);
        scaleTween.Play();
    }
}
