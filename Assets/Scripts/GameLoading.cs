using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AnhuEXQBohbzBhlMnEBk3OKCcUU1wMVfK.wenNSA3oWooNB2O%2FsDTj3xoAPxIhtJhrgpXbY0IWeSg");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58d36d096adbcf362fc7a138");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
