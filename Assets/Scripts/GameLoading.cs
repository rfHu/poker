using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3Aah8qRasykc-URM3W9Y6NH23L2ANPcm4u.wPIp9T%2FOq%2FYA6m0rPYXCxIg%2B40wu%2BbsDfwVg01ZBv68");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58c159a5675b5a4abf137005"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
