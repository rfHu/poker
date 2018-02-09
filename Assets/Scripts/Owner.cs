using UnityEngine;

public class Owner : MonoBehaviour {
	public GameObject RoomMessagePrefab;

	public void OnClick() {
		var transform = PoolMan.Spawn(RoomMessagePrefab);
		transform.GetComponent<DOPopup>().Show();
        transform.GetComponent<RoomMessage>().Init();
	}
}
