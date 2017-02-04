using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class Pots : MonoBehaviour {
	public Text DC;
	public Text PrevPot;
	public GameObject PrPotGo;

	void Start()
	{
		registerRx();	
	}

	private void registerRx() {
		GameData.Shared.PrPot.AsObservable().Subscribe((value) => {
			toggleElement();
			DC.text =  "底池:" + GameData.Shared.Pot.Value.ToString();	
		}).AddTo(this);

		GameData.Shared.Pot.AsObservable().Subscribe((value) => {
			toggleElement();

			var prv = GameData.Shared.PrPot.Value;
			PrevPot.text = (GameData.Shared.Pot.Value - prv).ToString();

			if (prv > 0) {
				PrPotGo.SetActive(true);
			} else {
				PrPotGo.SetActive(false);
			} 

		}).AddTo(this);
	}

	private void toggleElement() {
		if (GameData.Shared.PrPot.Value > 0 || GameData.Shared.Pot.Value > 0) {
			PrPotGo.SetActive(true);
 		} else {
			PrPotGo.SetActive(false);
		 }
	}
}
