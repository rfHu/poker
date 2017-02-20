using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class Pots : MonoBehaviour {
	public Text DC;
	public Text PrevPot;
	public GameObject PrPotGo;

	public GameObject Grp;

	private static Pots pots;

	void Awake()
	{
		registerRx();	
		pots = this;
	}

	public static ChipsGrp CloneChips() {
		var go = Instantiate(pots.Grp, pots.PrPotGo.transform, true);
		go.SetActive(true);
		return go.GetComponent<ChipsGrp>();
	}

	private void registerRx() {
		GameData.Shared.Pot.AsObservable().Subscribe((value) => {
			var go = DC.gameObject;

			if (value > 0) {
				go.SetActive(true);
			} else {
				go.SetActive(false);
			}

			DC.text =  "底池:" + _.Num2Text<int>(value);	
		}).AddTo(this);

		GameData.Shared.PrPot.AsObservable().Subscribe((value) => {
			if (value > 0) {
				PrPotGo.SetActive(true);
			} else {
				PrPotGo.SetActive(false);
			} 

			PrevPot.text = _.Num2Text<int>(value); 
		}).AddTo(this);
	}
}
