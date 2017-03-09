using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3Ayp7_5gacEZxXlmSv8e0RkPZa38js6VDT.dJohXoXVc9MSqqXxBDyMAUS%2FiNkMUGq60v%2B1hMVPOKo");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58c13b2e837d792e900fae09"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
