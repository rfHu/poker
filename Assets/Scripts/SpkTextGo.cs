using UnityEngine;
using System;
using DG.Tweening;
using UniRx;

public class SpkTextGo: MonoBehaviour {
    public ScrollText Text;
    public GameObject Arrow; 
    public GameObject TextCont;
    public CanvasGroup cvg;

    public String Uid;

    private Tween tween;
    private float duration = 0.2f;

    void Awake()
    {
        cvg = GetComponent<CanvasGroup>();

        RxSubjects.GameReset.AsObservable().Subscribe((_) => {
            if (gameObject.activeSelf == false)
            {
                return;
            }

            if (tween != null)
            {
                tween.Kill();
            }
            gameObject.SetActive(false);

        }).AddTo(this);
    }
    
    public void ShowMessage(string text) {
        if (tween != null) {
            tween.Kill();
        }

        gameObject.SetActive(true);
        cvg.alpha = 0;

        tween = cvg.DOFade(1, duration);
        Text.SetText(text);        
        Text.OnComplete = () => {
            cvg.DOFade(0, duration).OnComplete(() => {
                gameObject.SetActive(false);
            });
        };
    }

    public void ChangePos(SeatPosition pos) {
        var rt = GetComponent<RectTransform>();
        var trt = TextCont.GetComponent<RectTransform>(); 
        var offsetX = 120;
        rt.anchoredPosition = new Vector2(0, 68);

        Arrow.SetActive(true);

        // 当玩家参与游戏时，调整特殊的位置
        if (Uid == GameData.Shared.Uid)  {
            rt.anchoredPosition = new Vector2(0, -210);
            // rt.anchoredPosition = new Vector2(-270, -110);
            trt.anchoredPosition = new Vector2(0, 0);
            Arrow.SetActive(false);
        }  else if (pos == SeatPosition.Right || pos == SeatPosition.TopLeft) {
            trt.anchoredPosition = new Vector2(-offsetX, 0);
        } else if (pos == SeatPosition.Left || pos == SeatPosition.TopRight) { 
            trt.anchoredPosition = new Vector2(offsetX, 0);
        } else if (pos == SeatPosition.Top || pos == SeatPosition.Bottom) {
            trt.anchoredPosition = new Vector2(0, 0);
        }
    }
 }