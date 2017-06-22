using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AAN0aHRtcPLOAcsyLXRhv4ILXiVBkKYHt.DW9FrsSPSLXYPOxgUEYYJyYRdWGzgdKwLacaykHdbpM");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("594b666ef43b1501ec7f8129");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
