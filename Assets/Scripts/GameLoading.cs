using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	// external.SetSid("s%3AdOlm2w5hGhqahgt--HMRhxyXjTCSmOLf.3IXxXmGw1R3E0Swr10JEcqYcGvn7AUZT4%2F0d99jOuT8");
        	external.SetSid("s%3A7-GuaM24kDOvBxAnGXTz0pMia1kQYXKM.e9c6rSmHZyEWQuTRe5mWuSd%2FrHKo6K5SxgJ2BBv8QZY");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59474636493fc701bb6b0c89");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
