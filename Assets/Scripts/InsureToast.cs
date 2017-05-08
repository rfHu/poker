using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UniRx;

public class InsureToast: MonoBehaviour {
    public Text MsgText;
    public RawImage Avatar;
    public Text NameText;

    private CanvasGroup cvg;
    private float duration = 0.3f;
    private Tween tween;

    void Awake()
    {
        cvg = GetComponent<CanvasGroup>();
        addEvents();
    }

    private void addEvents() {

    }

    public void Show(Dictionary<string, object> data) {
        if (tween != null) {
            tween.Kill();
        }

        tween = cvg.DOFade(1, duration);

        MsgText.text = "";
        
        NameText.text = "";
        StartCoroutine(_.LoadImage("aaa", (texture) => {
            Avatar.texture = _.Circular(texture);
        }));
    }

    public void Hide() {
        if (tween != null) {
            tween.Kill();
        }
        tween = cvg.DOFade(0, duration);
    }
}