using UnityEngine;

public class GuestHeader : MonoBehaviour {
	public string Uid;

	public void OnClick() {
		if (Uid == null) {
			return ;
		}

		var userPopup = (GameObject)Instantiate(Resources.Load("Prefab/User"));
		var canvas = GameObject.FindGameObjectWithTag("Canvas2").GetComponent<Canvas>();
		userPopup.GetComponent<DOPopup>().Show(canvas);
	}
}
