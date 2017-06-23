using UnityEngine;

public class Owner : MonoBehaviour {
	public void OnClick() {
		var transform = G.Spawn("RoomMessage");
		transform.GetComponent<DOPopup>().Show();
	}
}
