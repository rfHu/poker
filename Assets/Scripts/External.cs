using UnityEngine;

public class External{
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
