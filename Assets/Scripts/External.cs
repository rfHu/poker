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

	public void SetSid(string sid) {
		GameData.Shared.Sid = sid;
		checkSetup();		
	}

	public void SetRoomID(string roomID) {
		GameData.Shared.Room = roomID;
		checkSetup();
	}

	private void checkSetup() {
		if (string.IsNullOrEmpty(GameData.Shared.Sid) || string.IsNullOrEmpty(GameData.Shared.Room)) {
			return ;
		}

		Connect.Shared.Setup();	
	}
}
