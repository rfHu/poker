using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3AlutQV_3tSj2d5g9mmC2OcMdkTOvF1c3P.t09KigflZ3zWDVA7v63BvltJEptJ32%2FZQkROnH64MLg");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58ee0a83571df77f32b40f9b");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
