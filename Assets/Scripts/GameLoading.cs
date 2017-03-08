using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AoEh7aRMgGuGAAfDN176WltQoaMlfLS9i.omLzTR4KADkx7yNOzNdniVotdgjeC2koSSuJpBesfms");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58bfaf65b8c30f840fcabad2"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
