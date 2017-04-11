using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3AQtdgRLhJs9dhziIXSxBactPvwU2RZGo0.7sUi1ryTUn21h11plOAnBz1TGCyuJywK3pL9VWoAfKU");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58eb6cce02732507d150742b");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
