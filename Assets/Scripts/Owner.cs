using UnityEngine;

public class Owner : MonoBehaviour {
	public void OnClick() {
		var canvas  = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		var go = (GameObject)Instantiate(Resources.Load("Prefab/OwnerPanel"));
		go.GetComponent<DOPopup>().Show(canvas);
	}
}
