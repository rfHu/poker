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
		GameData.Shared.Pot.AsObservable().DistinctUntilChanged().Subscribe((value) => {
			var go = DC.gameObject;

			if (value > 0) {
				go.SetActive(true);
			} else {
				go.SetActive(false);
			}

			DC.text =  "底池:" + value.ToString();	
		}).AddTo(this);

		GameData.Shared.PrPot.AsObservable().DistinctUntilChanged().Subscribe((value) => {
			if (value > 0) {
				PrPotGo.SetActive(true);
			} else {
				PrPotGo.SetActive(false);
			} 

			PrevPot.text = value.ToString();
		}).AddTo(this);
	}
}
