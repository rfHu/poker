using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3A658SEb3LFrxMlfNQn_XVyb6Rzdy2dyR4.bWOhb18mEA%2BIZhvWjDKjp2Y3e6GSo6%2B51i8iCAjbntU");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58eaf93bdaa2fa739cc01da0");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
