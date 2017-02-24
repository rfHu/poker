using UnityEngine;
using System.Collections;

public class LoadingJSONCS : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LeanLoader.load("http://dentedpixel.com/assets/test_json.txt", new LLOptions().setOnLoad(onJSONLoaded) );


	}


	private void onJSONLoaded( string str ){
		LeanJSON j = new LeanJSON(str);

		LeanJSON[] imagesArr = j.Array("images");
		Debug.Log("0:"+imagesArr[0]);

		string tex = imagesArr[0].String("texture");
		Debug.Log("texStr:"+tex);
		LeanLoader.load(tex, new LLOptions().setOnLoad(onCubeImageLoaded));
	}

	private void onCubeImageLoaded( Texture2D tex ){
		Debug.Log("tex:"+tex);
		GameObject cube = GameObject.Find("Cube");
		cube.GetComponent<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture
	}

}
