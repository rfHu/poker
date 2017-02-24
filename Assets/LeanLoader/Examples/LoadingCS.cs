using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

public class LoadingCS : MonoBehaviour {

private LeanLoading cubeLoading;
private LeanLoading sphereLoading;
private LeanLoading musicLoading;

private XmlNodeList adList;
private Texture2D[] ads;

	void Start () {
		cubeLoading = LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", new LLOptions().setOnLoad(onCubeImageLoaded).setUseCache(true).setCacheLife(60*60*24));
		sphereLoading = LeanLoader.load("http://dentedpixel.com/assets/PrincessPiano.jpg", new LLOptions().setOnLoad(onSphereImageLoaded));

		Dictionary<string, object> postParams = new Dictionary<string, object>(){ { "adnumber", ""+Random.Range(2,4)} }; // send Post params as a Dictionary of string values 
		// setUseCacheAsBackup makes sure the cached version is only used in the event that the asset is not reachable (usually due to no internet access)
		LeanLoader.load("http://dentedpixel.com/assets/advertising.php", new LLOptions().setOnLoad(onAdvertisingXMLLoaded).setUseCacheAsBackup( true ).setPostParams(postParams) );

		musicLoading = LeanLoader.load("http://dentedpixel.com/assets/Furt_brahms_2.ogg", new LLOptions().setOnLoad(onMusicLoaded).setUseCache(true));
	}

	private void onCubeImageLoaded( Texture2D tex ){
		GameObject cube = GameObject.Find("Cube");
		cube.GetComponent<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture
	}

	private void onSphereImageLoaded( Texture2D tex ){
		GameObject sphere = GameObject.Find("Sphere");
		sphere.GetComponent<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture
	}

	private void onAdvertisingXMLLoaded( string str ){
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml( str ); // convert the plain-text to xml
		adList = xmlDoc.GetElementsByTagName("advert"); // you can traverse the xml through all of the standard methods provided by XmlDocument

		ads = new Texture2D[ adList.Count ];
		for(int i = 0; i < adList.Count;i++){
			Hashtable onLoadParam = new Hashtable();
			onLoadParam.Add("i",i);
			LeanLoader.load( adList[i].Attributes["img"].Value, new LLOptions().setOnLoad( onAdLoadComplete ).setOnLoadParam( onLoadParam ).setUseCache(true) );
		}
	}

	private void onMusicLoaded( AudioClip audioClip ){
		AudioSource audioSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
		audioSource.clip = audioClip;
		audioSource.loop = true;
		audioSource.Play();
	}

	private void onAdLoadComplete( Texture2D tex, Dictionary<string,object> hash ){
		int i = System.Convert.ToInt32(hash["i"]);
		ads[i] = tex; // Texture used as a GUI Texture
	}

	void OnGUI(){
		GUI.Label(new Rect(0.0f,Screen.height*0.0f,Screen.width,Screen.height*0.1f), " Cube Image Progress:"+ cubeLoading.progress*100.0 +"%");
		GUI.Label(new Rect(0.0f,Screen.height*0.1f,Screen.width,Screen.height*0.1f), " Sphere Image Progress:"+ sphereLoading.progress*100.0 +"%");
		GUI.Label(new Rect(0.0f,Screen.height*0.2f,Screen.width,Screen.height*0.1f), " Music Progress:"+ musicLoading.progress*100.0 +"%");

		if(GUI.Button(new Rect(0.8f*Screen.width,Screen.height*0.0f,0.2f*Screen.width,Screen.height*0.2f), "Purge Cache") ){
			LeanLoader.deleteCacheAll(); // Purge the whole cache, the values are saved in the PlayerPrefs, you can also delete specific values with LeanLoader.deleteCache( st )
		}

		// Ads shown from the XML loaded
		for(int i = 0; adList!=null && i < adList.Count;i++){
			if(ads[i] && GUI.Button(new Rect(0.0f*Screen.width,Screen.height*(0.8f-0.2f*i),0.2f*Screen.width,Screen.height*0.2f), ads[i]) ){
				Application.OpenURL (adList[i].Attributes["link"].Value);
			}
		}
	}
	
	
}
