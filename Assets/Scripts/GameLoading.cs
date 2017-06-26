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
			// external.SetSid("s%3AtlWwZxu4UCVIFOeNzUTOoL7njX52JALS.QkHG%2B7VsbQBBrsgnRSv0FeFO2eXRAQQpLGJeP5AIXNI");
            // external.SetRoomID("5950a958cfb1ed3a3860c871");

			external.SetSid("s%3AIqwKeQYx5U76gnwrJKDxJZjWL9n-CZFr.JPiC%2F4z6CkhI2HkJ5uZlttQaA874IWe8v1tQhFm8ECk");
			external.SetRoomID("5950f94e7b6c2a0349a48ce9");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
