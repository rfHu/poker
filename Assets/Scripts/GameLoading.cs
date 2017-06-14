using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AvYevqr6AFW5yxm-WKPhDX32qzZyKS8rA.xJqNfn%2FYwDBxqTvxk0fMscqo7fgt2yqVL6vj6KP2tjs");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5940ea95758b865c4bb3890b");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
