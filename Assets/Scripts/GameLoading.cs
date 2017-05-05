using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AXPw5ZeQbqaIJ2nymEqVevIDF3UKwgzpQ.o1VU5nHoOMJuiqQ0%2FJ5mAPdHHhE0RmRpB3Zm1dnooUs");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("590c20c2b1628514ae36f638");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
