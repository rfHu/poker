using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AipKINdGnF0hbc0fuldjj_-D5_xEg8KJv.S5yH2UQt%2FaGT8tCWKmPSP1h3OAXWpINicDcZmhny3sI");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b93ee6b1896db03cdc9939"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
