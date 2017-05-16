using UnityEngine;

public class Owner : MonoBehaviour {
	public void OnClick() {
		var go = (GameObject)Instantiate(Resources.Load("Prefab/RoomMessage"));
        go.GetComponent<RectTransform>().localPosition = Vector3.zero;
		go.GetComponent<DOPopup>().Show();
	}
}
