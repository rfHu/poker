using UnityEngine;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;
		ObjectsPool.Setup();

		#if UNITY_EDITOR  
        	// external.SetSid("s%3AdOlm2w5hGhqahgt--HMRhxyXjTCSmOLf.3IXxXmGw1R3E0Swr10JEcqYcGvn7AUZT4%2F0d99jOuT8");
			external.SetSocket("https://socket.poker.top");
            // external.SetSid("s%3AMlxEue3v7i2rYLs9bKAdOgaOTSXsAAei.xmUcVOEOHLwtUelq64efdBGZ28RAEJhF%2FuDkUE4QZtg");
			external.SetSid("s%3AtlWwZxu4UCVIFOeNzUTOoL7njX52JALS.QkHG%2B7VsbQBBrsgnRSv0FeFO2eXRAQQpLGJeP5AIXNI");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5950cf2ccfb1ed3a38615e61");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
