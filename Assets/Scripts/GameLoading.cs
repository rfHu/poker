using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AObNf0oAGsstOzb6lJV5Ox01KqciJ2yZD.vAJRZu9bxFdFT0DZ54Hj7yGRANrerASp62oJ69RlzKo");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58c7bc537ade6b0dcb175d8c"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
