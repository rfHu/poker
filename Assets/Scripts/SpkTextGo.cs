using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using SimpleJSON;

public class SpkTextGo: MonoBehaviour {
    public Text MessageText;
    public GameObject Arrow; 

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

    public void Hide() {
        gameObject.SetActive(false);
    }

    void Start()
    {
        RxSubjects.SendChat.Subscribe((jsonStr) => {
            var N = JSON.Parse(jsonStr);
            var text = N["text"].ToString();
            var uid = N["uid"].ToString();

            if (uid != Uid) {
                return ;
            }

            ShowMessage(text);
        }).AddTo(this); 
    }

    void OnDestroy()
    {
        if (disposable != null) {
            disposable.Dispose();
        }
    }
}