
private var facebookProfileImage:Texture2D;

function Start () {
	LeanLoader.load("https://graph.facebook.com/DentedPixel/picture?type=large&redirect=false", LLOptions().setOnLoad(onImageLoaded));
}

private function onImageLoaded( tex:Texture2D ){
	Debug.Log("Your image texture ready to use! :"+tex);
	facebookProfileImage = tex;
}

function OnGUI(){
	if(facebookProfileImage)
		GUI.DrawTexture( new Rect(0.0f,0.0f,Screen.width*0.2f,Screen.width*0.2f), facebookProfileImage);
}
