using UnityEngine;
using UnityEngine.SceneManagement;

public class External : MonoBehaviour{
	private static External instance;

	public static External Instance {
		get {
			if (instance == null) {
				GameObject go = new GameObject();
				Object.DontDestroyOnLoad(go);
				
				instance = go.AddComponent<External>();
			}

			return instance;
		}
	}

	void Awake() {
		this.name = "External";
	}

	public void Exit() {
		Connect.Shared.Close();

		// 清空两个关键数据
		GameData.Shared.Sid = "";
		GameData.Shared.Room = "";

		// 返回上级界面
		SceneManager.LoadScene("GameLoading");
		Commander.Shared.Exit();
	}

	public void SetSid(string sid) {
		GameData.Shared.Sid = sid;
		checkSetup();		
	}

	public void SetRoomID(string roomID) {
		GameData.Shared.Room = roomID;
		checkSetup();
	}

	public void SetProxy(string proxy) {
		GameData.Shared.Proxy = proxy;		
	}

	private void checkSetup() {
		if (string.IsNullOrEmpty(GameData.Shared.Sid) || string.IsNullOrEmpty(GameData.Shared.Room)) {
			return ;
		}

		Time.timeScale = 1;
		Connect.Setup();	
	}
}
