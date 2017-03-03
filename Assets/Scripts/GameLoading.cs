using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3A6bHhYVJptTX9PrvKfP_0WWk4yjmpL67N.x4omvaw8UVrWsHaVaVfcwD8yW0MNYlT3ni4%2Bf%2BvazYc11");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b9377bb1896db03cdc9932"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
