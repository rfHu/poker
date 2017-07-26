using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    public void Init(Vector2 fromSeat, GameObject toSeat, bool isToMe) 
    {
        transform.SetParent(G.UICvs.transform, false);

        var _rectTransform = GetComponent<RectTransform>();
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
