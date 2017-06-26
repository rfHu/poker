using UnityEngine;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;
		ObjectsPool.Setup();

		#if UNITY_EDITOR  
        	// external.SetSid("s%3AdOlm2w5hGhqahgt--HMRhxyXjTCSmOLf.3IXxXmGw1R3E0Swr10JEcqYcGvn7AUZT4%2F0d99jOuT8");
			external.SetSocket("https://socket.dev.poker.top");
            external.SetSid("s%3AMlxEue3v7i2rYLs9bKAdOgaOTSXsAAei.xmUcVOEOHLwtUelq64efdBGZ28RAEJhF%2FuDkUE4QZtg");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5950d05ec5ec0f7d422783cd");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
