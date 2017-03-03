using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3APqeOgsQUfgMbihrg-HzKps5aZWKLqm1q.zzR9wHpUY%2B8t8iP9vqMRiUDM9lqRRfUhcgqJ8bX9PcY");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b96235447b32bf42353b39"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
