using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3ApoV82Gv9g8achifdq5DtRLaG8v4xpv0e.cqLq%2BDOtV1ijIB1odKQubjxiADaJkfBtk%2B%2FaT%2FlM2Mo");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f5c0465630a67fe7d250c6");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
