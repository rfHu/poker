using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class SpkTextGo: MonoBehaviour {
    public Text MessageText;
    public GameObject Arrow; 
    public GameObject TextCont;
    public GameObject Arrow2;

    public String Uid;

    private IDisposable disposable;
    
    public void ShowMessage(string text) {
        gameObject.SetActive(true);
        
        MessageText.text = text;

        if (disposable != null) {
            disposable.Dispose();
        }

        // 3s后自动消失
        disposable = Observable.Timer(TimeSpan.FromSeconds(3)).AsObservable().Subscribe(
            (_) => {
                gameObject.SetActive(false);
            }
        ).AddTo(this);
    }

    public void ChangePos(SeatPosition pos) {
        var rt = GetComponent<RectTransform>();
        var trt = TextCont.GetComponent<RectTransform>(); 
        var offsetX = 120;
        rt.anchoredPosition = new Vector2(0, 68);

        Arrow.SetActive(true);
        Arrow2.SetActive(false);

        if (pos == SeatPosition.Bottom) {
            rt.anchoredPosition = new Vector2(-270, -110);
            trt.anchoredPosition = new Vector2(0, 0);

            Arrow.SetActive(false);
            Arrow2.SetActive(true);
        }  else if (pos == SeatPosition.Right || pos == SeatPosition.TopLeft) {
            trt.anchoredPosition = new Vector2(-offsetX, 0);
        } else if (pos == SeatPosition.Left || pos == SeatPosition.TopRight) { 
            trt.anchoredPosition = new Vector2(offsetX, 0);
        } else if (pos == SeatPosition.Top) {
            trt.anchoredPosition = new Vector2(0, 0);
        }
    }

    public void Hide() {
        disposable.Dispose();
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (disposable != null) {
            disposable.Dispose();
        }
    }
}