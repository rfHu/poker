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
            external.SetSid("s%3Am8wxqOVLDhikUA86T9oRWuzGWvdZKlIn.0M1CR0f4wxnP%2Blli7wCd9UlYVq6XcVM1y%2FOCuROIffc");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5950c061023fd77b24a1abbd");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
