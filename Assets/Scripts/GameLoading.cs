using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3As6Bw-LtmaMSjOd98sZzW_4vR2psAm3Kt.GvrVdrX%2BxlFrG59c96wwVTA0uZMi9G6YBUl3AWeQMiY");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59294c9c01670d1792c089c7");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
