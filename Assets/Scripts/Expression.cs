using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class Expression: MonoBehaviour {
    public Transform Face;
    public Sprite FaceImage;

    private IDisposable disposable;


    public void SetTrigger(string name, Transform parent, Action cb = null) {
        var rect = GetComponent<RectTransform>();
        var prect = parent.GetComponent<RectTransform>();
		var canvasGrp = gameObject.GetComponent<CanvasGroup>();

        transform.SetParent(G.UICvs.transform, false);

		if (canvasGrp != null) {
			canvasGrp.alpha = 1;
		}
		
        rect.anchorMax = prect.anchorMax;
        rect.anchorMin = prect.anchorMin;
        rect.pivot = prect.pivot;
        rect.position = parent.position;

        var animator = Face.GetComponent<Animator>();
        animator.Play(name);

        if (disposable != null) {
            disposable.Dispose();
        }

        disposable = Observable.Timer(TimeSpan.FromSeconds(4)).Subscribe((__) => {
			if (canvasGrp == null) {
				canvasGrp = gameObject.AddComponent<CanvasGroup>();
			}

			canvasGrp.DOFade(0, 0.3f).OnComplete(() => {
				PoolMan.Despawn(transform);
				if (cb != null) {
					cb();
				}
			});
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