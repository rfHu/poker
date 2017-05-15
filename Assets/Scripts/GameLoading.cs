using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3Aa7ZFCi9ZbtYq-1CN6F2qaZKoW-40Px0P.k%2FRmBAC9XCM37pHjRXsEJp3gTLWrtvtIVKfM21eUh1E");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59191d988b6d267fa4b92a2d");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
