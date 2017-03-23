using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AGCwwBDdmOrPPsNHR7SHttlkk4DO3cDh-.DlggMK1f4aCqYqn7QvhX%2FUbTQc41Ogs%2B1ts9I1wg%2Fgo");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58d33f9ac312402c0cf7d058");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
