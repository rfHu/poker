using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AkbcBN_xN_4YQdm1rt7h-iqCyRWwcVEFq.VR6h2f2sDfz0%2BmaeuNQG%2BMq%2B7qfrg%2BzuKqX1ZF8eMA8");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58d367d06adbcf362fc7a135");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
