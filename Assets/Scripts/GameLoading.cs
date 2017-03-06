using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3A2q6qDztVNeIgQuVHTZ3JulZcHmJVg5c2.N26QPJOf91Z%2BkS1jCa%2B2WBY%2F1wOC0qlCubCRWHPzKVo");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58bd0cf74d4e9087b0557e25"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
