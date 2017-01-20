using UnityEngine;
using UnityEngine.UI;

public class Pots : MonoBehaviour {
	public Text DC;
	public Text PrevPot;

	public void UpdatePot() {
		gameObject.SetActive(true);
		GetComponent<Pots>().PrevPot.text = (GConf.Pot - GConf.PrPot).ToString();
		GetComponent<Pots>().DC.text =  "底池:" + GConf.Pot.ToString();	

		var pgo = PrevPot.transform.parent.gameObject;

		if (GConf.PrPot == 0) {
			pgo.SetActive(false);
		} else {
			pgo.SetActive(true);
		}
	}
}
