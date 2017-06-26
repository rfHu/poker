using UnityEngine;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		var external = External.Instance;
		ObjectsPool.Setup();

		#if UNITY_EDITOR  
			external.SetSocket("https://socket.dev.poker.top");
			external.SetProxy("http://localhost:8888");
			external.SetSid("s%3ASuOxhyXR0wmfu1xIZZUmFFdAq8Cd5xsD.h6RldH4l7niXxoZG0QyYnlt36QayIW0rtY0Z8GmorV0");
            // external.SetRoomID("5950a958cfb1ed3a3860c871");

			// external.SetSid("s%3AtlWwZxu4UCVIFOeNzUTOoL7njX52JALS.QkHG%2B7VsbQBBrsgnRSv0FeFO2eXRAQQpLGJeP5AIXNI");
			external.SetRoomID("59514fe7975cbe0c5fa91b8b");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
