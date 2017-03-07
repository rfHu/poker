using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AcIOSTH-bTkqdESTC1AmMAPgImhenKYCD.mOk73D4vXs1P1KByceBGRTfcjCRptqrVPFUpTNctHmE");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58bea132e4dd0d1022155d74"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
