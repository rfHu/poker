using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AsId4wHe8mnTSOTlwOhvMIO5rWu7oZ6pc.XDJAoeeFpARjjny%2F8SMFuXpF7AqhBszMlmGsCu7R5Fo");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5902dc29f25c253715f32c41");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
