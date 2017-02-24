
function Start () {
	LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", LLOptions().setOnLoad(onImageLoaded).setUseFile(true));
}

private function onImageLoaded( tex:Texture2D ){
	Debug.Log("Your image texture ready to use! :"+tex);
}
