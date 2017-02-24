
private var imageCached:Texture2D;

function Start () {
	LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", LLOptions().setOnLoad(onImageLoaded).setUseFile(true).setUseCache(true).setCacheLife(60*5));
}

function OnGUI(){
	if(imageCached)
		GUI.DrawTexture( new Rect(0.0f,0.0f,Screen.width*0.2f,Screen.width*0.2f), imageCached);
}

private function onImageLoaded( tex:Texture2D ){
	Debug.Log("Your image texture ready to use! :"+tex);
	imageCached = tex;
}
