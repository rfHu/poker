using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

public class Expression: MonoBehaviour {
    public Transform Face;
    public Sprite FaceImage;

    public void SetTrigger(string name, Action cb = null) {
        var animator = Face.GetComponent<Animator>();
        animator.Play(name);

        Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe((__) => {
            PoolMan.Despawn(transform);

            if (cb != null) {
                cb();
            }
        }).AddTo(this);
    }

    void OnDespawned() {
        foreach(Transform child in Face) {
            child.gameObject.SetActive(false);
        } 

        Face.GetComponent<Image>().sprite = FaceImage;
    }
}