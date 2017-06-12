using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3A8b2dUD6ONK1kPi8WGC1ecyxsncWIKzxW.dOlYVe8sCXZjlx91fnNtFhQ0GWlHskWURr114GbqKeI");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("593e7dbd9bb13c2d82fd2922");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
