using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class SpkTextGo: MonoBehaviour {
    public Text MessageText;
    public GameObject Arrow; 
    public GameObject TextCont;

    public String Uid;

    private IDisposable disposable;

    public void ShowMessage(string text) {
        _.Log("调用ShowMessage");

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
        Vector2 vector = new Vector2(0, 0);
        
        if (pos == SeatPosition.Right) {
            vector = new Vector2(-80, 0);
        } else if (pos == SeatPosition.Left) {
            vector = new Vector2(80, 0);
        } 

        GetComponent<RectTransform>().anchoredPosition = vector;
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