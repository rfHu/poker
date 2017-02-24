#pragma strict
import System.Xml;
import System.Collections.Generic;

private var cubeLoading:LeanLoading;
private var sphereLoading:LeanLoading;
private var musicLoading:LeanLoading;

private var adList:XmlNodeList;
private var ads:Texture2D[];

function Start () {
	cubeLoading = LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", LLOptions().setOnLoad(onCubeImageLoaded).setUseCache(true).setCacheLife(60*60*24));
	sphereLoading = LeanLoader.load("http://dentedpixel.com/assets/PrincessPiano.jpg", LLOptions().setOnLoad(onSphereImageLoaded));

	var postParams:Hashtable = {"adnumber":""+Random.Range(2,4)}; // send Post params as a Hashtable of string values 
	// setUseCacheAsBackup makes sure the cached version is only used in the event that the asset is not reachable (usually due to no internet access)
	LeanLoader.load("http://dentedpixel.com/assets/advertising.php", LLOptions().setOnLoad(onAdvertisingXMLLoaded).setUseCacheAsBackup( true ).setPostParams(postParams) );

	musicLoading = LeanLoader.load("http://dentedpixel.com/assets/Furt_brahms_2.ogg", LLOptions().setOnLoad(onMusicLoaded).setUseCache(true));
}

private function onCubeImageLoaded( tex:Texture2D ){
	var cube:GameObject = GameObject.Find("Cube");
	cube.GetComponent.<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture
}

private function onSphereImageLoaded( tex:Texture2D ){
	var sphere:GameObject = GameObject.Find("Sphere");
	sphere.GetComponent.<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture
}

private function onAdvertisingXMLLoaded( str:String ){
	var xmlDoc:XmlDocument = new XmlDocument();
	xmlDoc.LoadXml( str ); // convert the plain-text to xml
	adList = xmlDoc.GetElementsByTagName("advert"); // you can traverse the xml through all of the standard methods provided by XmlDocument

	ads = new Texture2D[ adList.Count ];
	for(var i:int = 0; i < adList.Count;i++){
		LeanLoader.load( adList[i].Attributes["img"].Value, LLOptions().setOnLoad( onAdLoadComplete ).setOnLoadParam( {"i":i} ).setUseCache(true) );
	}
}

private function onMusicLoaded( audioClip:AudioClip ){
	var audioSource:AudioSource = gameObject.AddComponent(AudioSource);
	audioSource.clip = audioClip;
	audioSource.loop = true;
	audioSource.Play();
}

private function onAdLoadComplete( tex:Texture2D, hash:Dictionary.<String,Object> ){
	var i:int = System.Convert.ToInt32( hash["i"] );
	ads[i] = tex; // Texture used as a GUI Texture
}

function OnGUI(){
	GUI.Label(new Rect(0.0f,Screen.height*0.0f,Screen.width,Screen.height*0.1f), " Cube Image Progress:"+ cubeLoading.progress*100.0 +"%");
	GUI.Label(new Rect(0.0f,Screen.height*0.1f,Screen.width,Screen.height*0.1f), " Sphere Image Progress:"+ sphereLoading.progress*100.0 +"%");
	GUI.Label(new Rect(0.0f,Screen.height*0.2f,Screen.width,Screen.height*0.1f), " Music Progress:"+ musicLoading.progress*100.0 +"%");

	if(GUI.Button(new Rect(0.8f*Screen.width,Screen.height*0.0f,0.2f*Screen.width,Screen.height*0.2f), "Purge Cache") ){
		LeanLoader.deleteCacheAll(); // Purge the whole cache, the values are saved in the PlayerPrefs, you can also delete specific values with LeanLoader.deleteCache( st )
	}

	// Ads shown from the XML loaded
	for(var i:int = 0; adList && i < adList.Count;i++){
		if(ads[i] && GUI.Button(new Rect(0.0f*Screen.width,Screen.height*(0.8f-0.2f*i),0.2f*Screen.width,Screen.height*0.2f), ads[i]) ){
			Application.OpenURL (adList[i].Attributes["link"].Value);
		}
	}
}

private var mat : Material;

function OnPostRender(){
	if( !mat )
        mat = new Material(Shader.Find("GUI/Text Shader") );

    	GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);
		GL.Color(Color.white);
		GL.Vertex3(0.4,0.45,0);
		GL.Vertex3(0.6,0.45,0);
		GL.Vertex3(0.6,0.55,0);
		GL.Vertex3(0.4,0.55,0);

		GL.Color(Color.cyan);
		GL.Vertex3(0.4+0.007,0.45+0.01,0);
		GL.Vertex3(0.4+0.007,0.55-0.01,0);
		GL.Color(Color.blue);
		GL.Vertex3(0.6-0.007,0.55-0.01,0);
		GL.Vertex3(0.6-0.007,0.45+0.01,0);

		GL.End();
		GL.PopMatrix();
}
