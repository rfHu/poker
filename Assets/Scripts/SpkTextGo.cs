using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class SpkTextGo: MonoBehaviour {
    public Text MessageText;
    public GameObject Arrow; 

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