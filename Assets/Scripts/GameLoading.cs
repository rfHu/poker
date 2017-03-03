using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AB0Lbhs5EjEep4V2w63qwncplFzNT_T9B.%2B9DqjSvnmTNocjSvWRAoKMrPNdhVaJYy76OVrFOqDNE");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b7f8d08421c9ae02f33943"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		Commander.Shared.Exit();
	}
}
