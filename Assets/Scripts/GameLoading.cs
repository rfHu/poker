using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AorUXV0VEXnhDNlbAcyzWvrYbE41I8Rr6.LgAqtCdLTbVSnmalb2SCn%2FYX%2FCBEuYET8UC%2F8P0bPik");
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
