using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR 
        external.SetSid("s%3AR5F2GohvrLCmsoRbmd0Ic5p5XrrjtNzW.fRzkkSxNHXb%2Fs69zff8Yk8E3dvhn2sOatybazCyUvVQ");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f731905630a67fe7d26124");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
