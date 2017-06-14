using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Pots : MonoBehaviour {
	public Text TotalPot;
	public ChipsGrp OnePot;
	public GameObject PotList;

	// public GameObject Grp;

	void Awake()
	{
		registerRx();	
	}

	// public static ChipsGrp CloneChipsHideSource() {
		// var go = Instantiate(pots.Grp, pots.transform, true);
		// go.SetActive(true);

		// pots.PrPotGo.SetActive(false);
		// pots.DC.gameObject.SetActive(false);

		// return go.GetComponent<ChipsGrp>();
	// }

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
					setPot(go.gameObject, chips);
					go.gameObject.SetActive(true);

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

				foreach(var winner in winners) {
					var child = transform.GetChild(i).gameObject;
					child.SetActive(false);
					
					var go = Instantiate(child, G.UICvs.transform, true);
					go.SetActive(true);
					
					var grp = child.GetComponent<ChipsGrp>();
					grp.OnlyChips();

					var gainChip = new GainChip(grp, winner);
					RxSubjects.GainChip.OnNext(gainChip);
				}
			}

		}).AddTo(this);
	}

	private void setPot(GameObject go, int chips) {
		go.GetComponent<ChipsGrp>().PotText.text = _.Num2CnDigit(chips);
	}
}
