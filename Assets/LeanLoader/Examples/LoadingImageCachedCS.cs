using UnityEngine;
using System.Collections;

public class LoadingImageCachedCS : MonoBehaviour {

	private Texture2D imageCached;

void Start () {
	LeanLoader.load("https://s.yimg.com/uy/build/images/sohp/hero/wildsee-pizol3.jpg?test2=no", new LLOptions().setOnLoad(onImageLoaded).setUseFile(true).setUseCache(true).setCacheLife(1));
}

void OnGUI(){
	if(imageCached)
		GUI.DrawTexture( new Rect(0.0f,0.0f,Screen.width*0.2f,Screen.width*0.2f), imageCached);
}

private void onImageLoaded( Texture2D tex ){
	Debug.Log("Your image texture ready to use! :"+tex);
	imageCached = tex;
}
}
