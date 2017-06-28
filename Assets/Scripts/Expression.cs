using UnityEngine;
using UniRx;
using System;

public class Expression: MonoBehaviour {
    public Transform Face;
    private string _name;

    public void SetTrigger(string name) {
        this._name = name;

        foreach(Transform child in Face) {
            child.gameObject.SetActive(false);
        }

        var animator = Face.GetComponent<Animator>();
        animator.SetTrigger(name);

        Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((_) => {
            animator.ResetTrigger(name);
        }).AddTo(this);
    }

    void OnDespawned() {
        var animator = Face.GetComponent<Animator>();
        animator.ResetTrigger(_name);
    }
}