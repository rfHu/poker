using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AhqfsFP10MULp81hsW8SpfDK38vUZvj31.ZcwdROPumfVrMcQg8upykwf9JiTs0HRcS5KLjWVv%2BgA");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58ce73dbf7b83340e62a3458");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
