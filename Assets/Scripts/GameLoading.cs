using UnityEngine;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;
		ObjectsPool.Setup();

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.poker.top");
			external.SetProxy("http://localhost:8888");
			// external.SetSid("s%3ASuOxhyXR0wmfu1xIZZUmFFdAq8Cd5xsD.h6RldH4l7niXxoZG0QyYnlt36QayIW0rtY0Z8GmorV0");
            // external.SetRoomID("5950a958cfb1ed3a3860c871");
			external.SetSid("s%3A5l6J7CJukVfYMoYqSrZPZxi3RONaUN4s.bHF7kw63%2FXniGXRUJF4zFufB5twJnr%2FaoRU6C6ZFZZM");
			external.SetRoomID("5951e3e1d6a8664f00b68304");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
