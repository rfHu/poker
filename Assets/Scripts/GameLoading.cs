using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3A99zI6Vp-jR8zKTdASjp7eVkE5j2m1sIZ.5%2Bm4ebLnSS1c9LGld4R87mlCTmT%2Bv%2FROhWY4XGIeqwc");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58bcea936faeb43d18b66e8b"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
