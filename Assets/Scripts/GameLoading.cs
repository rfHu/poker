using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AC6A9TYGjM2xB8hg4uHbSSYa3DH3Jm_OQ.vCRCNxlMNkyK9Mx1Y88324Wr%2B90noHLubnhgTXBAqWo");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59376d14b5e6c40a6467cd3f");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
