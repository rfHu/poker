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
            external.SetSid("s%3AAN0aHRtcPLOAcsyLXRhv4ILXiVBkKYHt.DW9FrsSPSLXYPOxgUEYYJyYRdWGzgdKwLacaykHdbpM");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("594e14dea697787ff4f72dfc");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
