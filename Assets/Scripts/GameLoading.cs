using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3AIDh5jfTzeuJ8NbsUMx9Lp9xsYP-doniI.OH%2FmGsBckl2lqSP4%2FFydgVdsJGr9L34emRbrpsfJITg");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58db5203c0b2c164b049dcfa");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
