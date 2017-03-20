using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3A0pYdFFAuzxoir5qSG4DQ-E05xT4hNXXY.5qB%2FJu%2BCRHx7VU7Y4%2BmUNzOG7tYifYz7sl4OH5z%2BG3g");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58cf37bff7b83340e62a3464");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
