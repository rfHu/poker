using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
		        external.SetSid("s%3ABaQWmg7b231y1musGS2N4ersKcwuLU6r.E2%2F0J3U%2BpamXQSo3u0L0ysZrhgBikuJQyleMq57Ye2E");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("591ab3ecd174e864a1f58a6f");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
