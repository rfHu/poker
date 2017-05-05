using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

[RequireComponent(typeof(Text))]
public class ScrollText: MonoBehaviour {
    private Text text;
    private RectTransform rectTransform;
    private float distance = 42f;
    private IDisposable disposable;
    private Tween tween;
    private float duration = 0.5f;

    public Action OnComplete;

    void Awake()
    {
        text = GetComponent<Text>();        
        rectTransform = GetComponent<RectTransform>();
    }

    private void startScroll() {
        if (disposable != null) {
            disposable.Dispose();
        }

        disposable = Observable.Timer(TimeSpan.FromSeconds(2.5)).Subscribe((_) => {
            var height = rectTransform.rect.height;
            var y = rectTransform.anchoredPosition.y;

            if (height - y > distance) {
                tween = rectTransform.DOAnchorPosY(y + distance, duration).OnComplete(startScroll);
            } else {
                // tween = cg.DOFade(0, duration).OnComplete(startScroll);
                if (OnComplete != null) {
                    OnComplete();
                }
            }
        }).AddTo(this);
    }

    public void SetText(string str) {
        if (disposable != null) {
            disposable.Dispose();
        }

        if (tween != null) {
            tween.Kill();
        }

        // 还原状态
        rectTransform.anchoredPosition = new Vector2(0, 0);
        // cg.DOFade(1, duration);

        text.text = str;
        startScroll();
    }
}