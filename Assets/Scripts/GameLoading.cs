using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3Aa1z-MGOIOMqkv6QypBWswj1fMMMF_GYG.oS2IAT%2FV1t2jiEbxzLkbMpQSYUP%2BEC%2BFmC38Ut5eS0s");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b7b5129b05ca2534c4f2a3"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
