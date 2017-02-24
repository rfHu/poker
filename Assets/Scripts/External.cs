using UnityEngine;

public class External : MonoBehaviour{
	void Awake() {
		this.name = "External";
		Object.DontDestroyOnLoad(gameObject);
	}

	public void Exit() {
		Connect.Shared.Close(() => {
			Application.Quit();		
		});
	}

	public void Enter(string sid, string roomID) {
		GameData.Shared.Sid = sid;
		GameData.Shared.Room = roomID;
		Connect.Shared.Setup();	
	}
}
