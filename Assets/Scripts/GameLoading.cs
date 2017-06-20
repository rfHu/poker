using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;

		#if UNITY_EDITOR  
        	// external.SetSid("s%3AdOlm2w5hGhqahgt--HMRhxyXjTCSmOLf.3IXxXmGw1R3E0Swr10JEcqYcGvn7AUZT4%2F0d99jOuT8");
        	external.SetSid("s%3Aupurq6_6pnPu4vBkDtVorwO0B7cckne9.CzjLFjsyylkUD9HAPU8E9FMZ6P%2Bks93TAFvhLlhCARc");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5948e2c3e5905f118abab4cc");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
