using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class Pots : MonoBehaviour {
	public Text TotalPot;
	public GameObject OnePot;
	public GameObject PotList;

	// public GameObject Grp;
	private CompositeDisposable disposables = new CompositeDisposable();

	void Awake()
	{
		registerRx();	
	}

	void OnDestroy()
	{
		disposables.Clear();
	}

	private void registerRx() {
		GameData.Shared.Pot.AsObservable().Subscribe((value) => {
			var go = TotalPot.gameObject;

			if (value > 0) {
				go.SetActive(true);
			} else {
				go.SetActive(false);
			}

			TotalPot.text =  "底池:" + _.Num2CnDigit<int>(value);	
		}).AddTo(this);

		GameData.Shared.Pots.AsObservable().Subscribe((list) => {
			disposables.Clear();

			if (list == null) {
				list = new List<Dictionary<string, object>>();
			}

			var transform = PotList.transform;
			
			if (list.Count == 0) {
				transform.Clear();
				return ;
			}	

			var diff = transform.childCount - list.Count;

			// 多出来的要删除
			while(diff > 0) {
				var child = transform.GetChild(transform.childCount - 1);
				Destroy(child.gameObject);
				
				diff -= 1;
			}

			for(var i = 0; i < list.Count; i++) {
				var chips = list[i].Int("chips");
				
				if (transform.childCount <= i) {
					var go = Instantiate(OnePot, transform, false);
					setPot(go, chips);
					go.SetActive(true);

					var cvg = go.GetComponent<CanvasGroup>();
					cvg.alpha = 0;
					cvg.DOFade(1, 0.3f);
				} else {
					var child = transform.GetChild(i);
					setPot(child.gameObject, chips);
				}
			}

			for(var i = 0; i < list.Count; i++) {
				var winners = list[i].SL("win_uids");

				if (winners.Count <= 0) {
					continue;
				}

				var child = transform.GetChild(i).gameObject;
				
				Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe((_) => {
					if (child == null) {
						return ;
					}
					
					foreach(var uid in winners) {
						if (GameData.Shared.FindPlayerIndex(uid) == -1) {
							return ;
						}

						var go = Instantiate(child, G.UICvs.transform, true);
						go.SetActive(true);
						
						var grp = go.GetComponent<ChipsGrp>();
						grp.OnlyChips();

						var gainChip = new GainChip(grp, uid);
						RxSubjects.GainChip.OnNext(gainChip);
					}

					child.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => {
						Destroy(child);
						TotalPot.gameObject.SetActive(false);
					});
				}).AddTo(disposables);	
			}
		}).AddTo(this);
	}

	private void setPot(GameObject go, int chips) {
		go.GetComponent<ChipsGrp>().PotText.text = _.Num2CnDigit(chips);
	}
}
