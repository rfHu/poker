using UnityEngine;
using System;
using DG.Tweening;

public class SpkTextGo: MonoBehaviour {
    public ScrollText Text;
    public GameObject Arrow; 
    public GameObject TextCont;
    public GameObject Arrow2;
    public CanvasGroup cvg;

    public String Uid;

    private Tween tween;
    private float duration = 0.2f;

    void Awake()
    {
        cvg = GetComponent<CanvasGroup>();
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
        Arrow2.SetActive(false);

        // 当玩家参与游戏时，调整特殊的位置
        if (Uid == GameData.Shared.Uid)  {
            rt.anchoredPosition = new Vector2(0, -220);
            trt.anchoredPosition = new Vector2(0, 0);

            Arrow.SetActive(false);
            Arrow2.SetActive(true);
        }  else if (pos == SeatPosition.Right || pos == SeatPosition.TopLeft) {
            trt.anchoredPosition = new Vector2(-offsetX, 0);
        } else if (pos == SeatPosition.Left || pos == SeatPosition.TopRight) { 
            trt.anchoredPosition = new Vector2(offsetX, 0);
        } else if (pos == SeatPosition.Top || pos == SeatPosition.Bottom) {
            trt.anchoredPosition = new Vector2(0, 0);
        }
    }
 }