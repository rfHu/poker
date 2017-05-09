using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3A7d1azZBKnZ_IhBZKTGL1fzBdFTm-Cnfq.xUqmY3tvqD2DH7Fq2qZAV6j9eYVYHYd2kvIUPOniN3M");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59113d88c4eda3558fab29b5");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
