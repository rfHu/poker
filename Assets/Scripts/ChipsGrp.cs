using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System;
using UniRx;

public class ChipsGrp : MonoBehaviour {
	public List<GameObject> Chips;
	public Text PotText;

	private Transform target;

	void Awake()
	{
		RxSubjects.GameExit.Subscribe((_) => {
			Destroy(gameObject);
		}).AddTo(this);
	}

	public void OnlyChips() {
		GetComponent<ProceduralImage>().enabled = false;
		PotText.enabled = false;
	}

	public void ToParent(Transform target, Action onComplete) {
		this.target = target;

		doAnim(Chips[0], 0);	
		doAnim(Chips[1], 0.05f);	
		doAnim(Chips[2], 0.1f);	
		doAnim(Chips[3], 0.15f);	
		doAnim(Chips[4], 0.2f, onComplete);	

		G.PlaySound("chipfly");

		Destroy(gameObject);
	} 

	private void doAnim(GameObject go, float delay, Action cb = null) {
		go.transform.SetParent(target, true);
		go.GetComponent<Image>().DOFade(0.3f, 0.3f).SetDelay(delay);
		go.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 0), 0.3f).SetDelay(delay).OnComplete(() => {
			Destroy(go);
			if (cb != null) {
				cb();
			}
		});	
	}
}
