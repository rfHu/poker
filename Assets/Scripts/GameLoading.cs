using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AnD42tMbXHBrivUKqCbEqBS17_tNodcsg.oKVxLGzBUhUFSxoDw%2FejwR9n4PwDzhL0%2FlMcXl%2FUu40");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58dc7c00c898205e6e43e11f");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
