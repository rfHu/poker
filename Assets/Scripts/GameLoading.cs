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
            external.SetSid("s%3ABRuJfTk2X1Lvx-TO5C378H2ctBThU0dt.jWcJAogoJOOBbr9XS0x7nMrIpwX3KrO5zOw4eflO3Ww");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("594e25dfa697787ff4f7313b");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
