using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AXDv4WoHh29phQZxHqaeVjAxN9hvcVTLW.KpPdmJuSslMhDMEBMY2tZZKXYexuhMc%2F1Qbwnr%2BlkiY");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58ca3f03ebcdea470156ac0b"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
