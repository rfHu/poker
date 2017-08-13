using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class Expression: MonoBehaviour {
    public Transform Face;
    public Sprite FaceImage;

    private IDisposable disposable;

    public void SetTrigger(string name, Transform parent) {
        var rect = GetComponent<RectTransform>();
        var prect = parent.GetComponent<RectTransform>();

        transform.SetParent(parent, false);

        var animator = Face.GetComponent<Animator>();
        animator.Play(name.Trim());

        if (disposable != null) {
            disposable.Dispose();
        }

        disposable = Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe((__) => {
            PoolMan.Despawn(transform);
        }).AddTo(this);
    }

    void OnDespawned() {
        foreach(Transform child in Face) {
            child.gameObject.SetActive(false);
        } 

        Face.GetComponent<Image>().sprite = FaceImage;
        disposable.Dispose();
    }
}