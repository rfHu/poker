using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3Ag-PernViMnEhMLxn_ABeopjlkQQbsvN_.NUgBmKTWXirsmG9NDsGOYgekG7LbHsODi46TTYK8MCY");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5908668e18b8563ecc8f502d");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
