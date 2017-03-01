using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AG9H7k82UJ71fN_ei6h72Av9mWmr8zPs5.yoBHmHOuXcbG27MkRYRpyyaM%2FzoehPdPFOWNJ%2FZSAhg");
			// external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b6a0d4dd31d98b0f12186b"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
