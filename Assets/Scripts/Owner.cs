using UnityEngine;

public class Owner : MonoBehaviour {
	public void OnClick() {
		var transform = PoolMan.Spawn("RoomMessage");
		transform.GetComponent<DOPopup>().Show();
        transform.GetComponent<RoomMessage>().Init();
	}
}
