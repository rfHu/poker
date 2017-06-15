using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AZgwZBKtm_Bpp8LSFYnUBlr3Zci7rkzdK.DHdcusXO9LrSKHeiIp7rcavHWBBUZ5gJZn4rEEG%2B4Uw");
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
