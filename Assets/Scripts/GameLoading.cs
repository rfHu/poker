using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3Anh2x_w09xdVjU1YUIpcqKjLVXX_HVpnu.lmIVJ6uzs0QFVqaBQZejOEwgUwd8BovXFX44%2FivP%2FDc");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59392581f5e608318d4305ad");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
