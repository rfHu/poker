using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AAN0aHRtcPLOAcsyLXRhv4ILXiVBkKYHt.DW9FrsSPSLXYPOxgUEYYJyYRdWGzgdKwLacaykHdbpM");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("594a42bff43b1501ec7f6dba");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
