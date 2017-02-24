// Copyright (c) 2015 Russell Savage - Dented Pixel
// 
// LeanLoader version 0.30 - http://dentedpixel.com/developer-diary/
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


/**
* JSON object for parsing a string into a useable JSON object
*
* @class LeanJSON
* @constructor
* @param {String} str:String String of representing the JSON object (will be parsed and made into an easily readable Dictionary<string,string> object)
*/
public class LeanJSON : Dictionary<string,string> {
	private static char begJson = "{"[0];
	private static char endJson = "}"[0];
	private static char begArr = "["[0];
	private static char endArr = "]"[0];
	private static char quoteDouble = "\""[0];
	private static char quoteSingle = "'"[0];
	private static char colon = ":"[0];
	private static char comma = ","[0];
	private static char escape = "\\"[0];

	public override string ToString(){
		return DictToString(this, null);
	}

	public static string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format)
	{
	    format = string.IsNullOrEmpty(format) ? "'{0}'='{1}', " : format; 

	    StringBuilder itemString = new StringBuilder();
	    itemString.Append("{");
	    foreach(var item in items)
	        itemString.AppendFormat(format, item.Key, item.Value);

	    itemString.Append("}");
	    return itemString.ToString(); 
	}

	public LeanJSON( string str ){
		int beg = str.IndexOf(begJson);
		int end = str.LastIndexOf(endJson);
		str = str.Substring(beg+1,end-beg-1);
		// Debug.Log("short:"+str);

		bool isDoubleQuoted = false;
		bool hasStartedQuote = false;
		int hasStartedEscape = -1;
		int closureLevel = 0;
		int lastComma = 0;
		int endPoint = str.Length-1;
		List<string> commaSplit = new List<string>();
		for(int i = 0; i <= endPoint; i++){
			/*if(str[i]==comma){
				Debug.Log("char["+i+"]:"+str[i] + " closureLevel:"+closureLevel+" hasStartedEscape:"+hasStartedEscape+" hasStartedQuote:"+hasStartedQuote+" lastComma:"+lastComma+" part:"+str.Substring(lastComma,i-lastComma));
			}*/
			// split on comma
			if(hasStartedEscape<=0){
				if(closureLevel==0){ // only parse if it is on the first level
					if(hasStartedQuote){ // Check if the quote area has ended
						if(isDoubleQuoted){
							if(str[i]==quoteDouble){
								hasStartedQuote = false;
							}
						}else{
							if(str[i]==quoteSingle){
								hasStartedQuote = false;
							}
						}
					}else{
						if(str[i]==comma){ // Push to array
							string split = str.Substring(lastComma,i-lastComma);
							// Debug.Log("split:"+split);
							commaSplit.Add( split );
							lastComma = i + 1;
						}else if(str[i]==escape){
							hasStartedEscape = 2;
						}else if(hasStartedEscape<=0){
							if(str[i]==quoteDouble){
								isDoubleQuoted = true;
								hasStartedQuote = true;
							}else if(str[i]==quoteSingle){
								isDoubleQuoted = false;
								hasStartedQuote = true;
							}	
						}
					}
				}
				if(i==endPoint){ // at end of string, push remaining
					string split = str.Substring(lastComma,i-lastComma+1);
					split = split.Replace(@"\", string.Empty); // strip off any escape characters from the strings
					commaSplit.Add( split );
					// Debug.Log("split:"+split);
				}

				if(hasStartedQuote==false){ // increment and decrement closure level
					if(str[i]==begJson){
						closureLevel++;
					}else if(str[i]==endJson){
						// Debug.Log("decrementing part:"+str.Substring(lastComma,i-lastComma));
						closureLevel--;
					}else if(str[i]==begArr){
						closureLevel++;
					}else if(str[i]==endArr){
						// Debug.Log("decrementing part:"+str.Substring(lastComma,i-lastComma));
						closureLevel--;
					}
				}
			}

			if(hasStartedEscape>=0){
				// Debug.LogError("Escapes exist! part:"+str.Substring(lastComma,i));
				hasStartedEscape--;
			}
		}
		
		for(int i = 0; i < commaSplit.Count; i++){
			// NEED to fix, colons can be hidden inside strings
			if(commaSplit[i].Length>1){
				int colonPoint = commaSplit[i].IndexOf(colon);
				string[] colonSplit = new string[]{ commaSplit[i].Substring(0,colonPoint), commaSplit[i].Substring(colonPoint+1) } ;
				// Debug.Log("colon 0:"+colonSplit[0] + " 1:"+ colonSplit[1]);
				// Trim quotes
				for(int j = 0; j < colonSplit.Length; j++){
					// Trim white-space
					colonSplit[j] = colonSplit[j].Trim();
					// Trim double quotes
					char firstChar = colonSplit[j][0];
					
					if(firstChar==quoteDouble){
						end = colonSplit[j].LastIndexOf(quoteDouble);
						// Debug.Log("colonSplit[j]:"+colonSplit[j]+" beg:"+beg+" end:"+end);
						colonSplit[j] = colonSplit[j].Substring(1,end-1);
					}else{
						// Trim single quotes
						if(firstChar==quoteSingle){
							end = colonSplit[j].LastIndexOf(quoteSingle);
							colonSplit[j] = colonSplit[j].Substring(1,end-1);
						}
					}
				}
				// Debug.Log("trimmed colon 0:"+colonSplit[0] + " 1:"+ colonSplit[1]);
				// Debug.Log("key:"+colonSplit[0]+" val:"+colonSplit[1]);
				this[ colonSplit[0] ] = colonSplit[1];
			}
		}
	}

	/**
	* Retrieve the object on the associated key (this is used when the JSON has nested JSON objects)
	* 
	* @method Object
	* @param {String} key:String String The key of the associated object you wish to retrieve
	* @return {LeanJSON} Value for key
	*/
	public LeanJSON Object( string key ){
		return new LeanJSON( this[key] );
	}

	/**
	* Retrieve the an array for the associated key (returns a LeanJSON[] Array). 
	* 
	* @method Array
	* @param {String} key:String String The key of the associated array you wish to retrieve
	* @return {Array} LeanJSON[] Array for key
	*/
	public LeanJSON[] Array( string key){
		string str = this[key];
		int beg = str.IndexOf(begArr);
		int end = str.LastIndexOf(endArr);
		str = str.Substring(beg+1,end-beg-1);

		bool isDoubleQuoted = false;
		bool hasStartedQuote = false;
		int hasStartedEscape = -1;
		int closureLevel = 0;
		int lastComma = 0;
		int endPoint = str.Length-1;
		List<string> commaSplit = new List<string>();
		for(int i = 0; i <= endPoint; i++){
			/*if(str[i]==comma){
				Debug.Log("char["+i+"]:"+str[i] + " closureLevel:"+closureLevel+" hasStartedEscape:"+hasStartedEscape+" hasStartedQuote:"+hasStartedQuote+" lastComma:"+lastComma+" part:"+str.Substring(lastComma,i-lastComma));
			}*/
			// split on comma
			if(hasStartedEscape<=0){
				if(closureLevel==0){ // only parse if it is on the first level
					if(hasStartedQuote){ // Check if the quote area has ended
						if(isDoubleQuoted){
							if(str[i]==quoteDouble){
								hasStartedQuote = false;
							}
						}else{
							if(str[i]==quoteSingle){
								hasStartedQuote = false;
							}
						}
					}else{
						if(str[i]==comma){ // Push to array
							string split = str.Substring(lastComma,i-lastComma);
							// Debug.Log("split:"+split);
							commaSplit.Add( split );
							lastComma = i + 1;
						}else if(str[i]==escape){
							hasStartedEscape = 2;
						}else if(hasStartedEscape<=0){
							if(str[i]==quoteDouble){
								isDoubleQuoted = true;
								hasStartedQuote = true;
							}else if(str[i]==quoteSingle){
								isDoubleQuoted = false;
								hasStartedQuote = true;
							}	
						}
					}
				}
				if(i==endPoint){ // at end of string, push remaining
					string split = str.Substring(lastComma,i-lastComma+1);
					split = split.Replace(@"\", string.Empty); // strip off any escape characters from the strings
					commaSplit.Add( split );
					// Debug.Log("split:"+split);
				}

				if(hasStartedQuote==false){ // increment and decrement closure level
					if(str[i]==begJson){
						closureLevel++;
					}else if(str[i]==endJson){
						// Debug.Log("decrementing part:"+str.Substring(lastComma,i-lastComma));
						closureLevel--;
					}
				}
			}

			if(hasStartedEscape>=0){
				// Debug.LogError("Escapes exist! part:"+str.Substring(lastComma,i));
				hasStartedEscape--;
			}
		}
		LeanJSON[] jsonArr = new LeanJSON[ commaSplit.Count ];
		for(int i = 0; i < jsonArr.Length; i++){
			if(commaSplit[i].Length>2)
				jsonArr[i] = new LeanJSON( commaSplit[i] );
		}

		return jsonArr;
	}

	/**
	* Retrieve the string the associated key 
	* 
	* @method String
	* @param {String} key:String String The key of the associated string you wish to retrieve
	* @return {String} Value for key
	*/
	public string String( string key ){
		return this[key];
	}

	/**
	* Retrieve the double value for the associated key 
	* 
	* @method Double
	* @param {String} key:String String The key of the associated string you wish to retrieve
	* @return {double} Value for key
	*/
	public double Double( string key ){
		return double.Parse( this[key] );
	}

	/**
	* Retrieve the float value for the associated key 
	* 
	* @method Float
	* @param {String} key:String String The key of the associated string you wish to retrieve
	* @return {float} Value for key
	*/
	public float Float( string key ){
		return float.Parse( this[key] );
	}

	/**
	* Retrieve the int value for the associated key 
	* 
	* @method Int
	* @param {String} key:String String The key of the associated string you wish to retrieve
	* @return {int} Value for key
	*/
	public int Int( string key ){
		return int.Parse( this[key] );
	}

	/**
	* Retrieve the boolean value for the associated key 
	* 
	* @method Boolean
	* @param {String} key:String String The key of the associated string you wish to retrieve
	* @return {boolean} Value for key
	*/
	public bool Boolean( string key ){
		return bool.Parse( this[key] );
	}

	public bool Bool( string key ){
		return bool.Parse( this[key] );
	}

}

public class LLOptions : object {

	public System.Action<Texture2D> onImageLoad;
	public System.Action<Texture2D,Dictionary<string,object>> onImageLoadP;
	public System.Action<string> onTextLoad;
	public System.Action<LeanJSON> onJSONLoad;
	public System.Action<string,Dictionary<string,object>> onTextLoadP;
	public System.Action<LeanJSON,Dictionary<string,object>> onJSONLoadP;
	public System.Action<AudioClip> onAudioLoad;
	public System.Action<AudioClip,Dictionary<string,object>> onAudioLoadP;
	public System.Action<string> onError;
	public Dictionary<string, object> postParams;
	public bool useCache;
	public bool useCacheAsBackup;
	public bool isImage;
	public bool isText;
	public bool isJSON;
	public bool isAudio;
	public bool useFile;
	public int cacheLife;
	public string cacheName;
	public LeanLoading ll;
	public Dictionary<string,object> onLoadParam;

	/**
	* Set options for the LeanLoader
	*
	* @class LLOptions
	* @constructor
	*/
	public LLOptions(){
		cacheLife = System.Int32.MaxValue;
	}

	/**
	* Set the callback for the asset once it is loaded (image loading)
	* 
	* @method LLOptions().setOnLoad
	* @param {System.Action<Texture2D>} onLoad:System.Action<Texture2D> System.Action<Texture2D> The method that is called once the image is loaded
	*/
	public LLOptions setOnLoad( System.Action<Texture2D> onLoad ){
		this.onImageLoad = onLoad;
		return this;
	}
	public LLOptions setOnLoad( System.Action<Texture2D,Dictionary<string,object>> onLoad ){
		this.onImageLoadP = onLoad;
		return this;
	}

	/**
	* Set the callback for the asset once it is loaded (data loading)
	* 
	* @method LLOptions().setOnLoad
	* @param {System.Action<string>} onLoad:System.Action<string> System.Action<string> The method that is called once the data is loaded
	*/
	public LLOptions setOnLoad( System.Action<string> onLoad ){
		this.onTextLoad = onLoad;
		return this;
	}
	public LLOptions setOnLoad( System.Action<string,Dictionary<string,object>> onLoad ){
		this.onTextLoadP = onLoad;
		return this;
	}

	/**
	* Set the callback for the asset once it is loaded (data loading JSON specific)
	* 
	* @method LLOptions().setOnLoad
	* @param {System.Action<LeanJSON>} onLoad:System.Action<LeanJSON> System.Action<LeanJSON> The method that is called once the data is loaded
	*/
	public LLOptions setOnLoad( System.Action<LeanJSON> onLoad ){
		this.onJSONLoad = onLoad;
		return this;
	}
	public LLOptions setOnLoad( System.Action<LeanJSON,Dictionary<string,object>> onLoad ){
		this.onJSONLoadP = onLoad;
		return this;
	}

	/**
	* Set the callback for the asset once it is loaded (audio loading)
	* 
	* @method LLOptions().setOnLoad
	* @param {System.Action<AudioClip>} onLoad:System.Action<AudioClip> System.Action<AudioClip> The method that is called once the audio is loaded
	*/
	public LLOptions setOnLoad( System.Action<AudioClip> onLoad ){
		this.onAudioLoad = onLoad;
		return this;
	}
	public LLOptions setOnLoad( System.Action<AudioClip,Dictionary<string,object>> onLoad ){
		this.onAudioLoadP = onLoad;
		return this;
	}

	/**
	* Set the callback for error method that is called if it encounters any issues while loading
	* 
	* @method LLOptions().setOnError
	* @param {System.Action<string>} onError:System.Action<string> System.Action<string> The method that is called when if it encounters an error
	*/
	public LLOptions setOnError( System.Action<string> onError ){
		this.onError = onError;
		return this;
	}

	/**
	* Set Post parameters in the server call
	* 
	* @method LLOptions().setPostParams
	* @param {Dictionary<string, string>} dictionary:Dictionary<string, string> Dictionary<string, string> Add a dictionary of string values that will be posted to the server call
	*/
	public LLOptions setPostParams( Dictionary<string, object> dict ){
		postParams = dict;
		return this;
	}

	/**
	* Set Post parameters in the server call
	* 
	* @method LLOptions().setPostParams
	* @param {Hashtable} hashtable:Hashtable Hashtable Add a hashtable of values that will be posted to the server call
	*/
	#if !UNITY_METRO
	public LLOptions setPostParams( Hashtable hash ){
		postParams = new Dictionary<string, object>();
		foreach (DictionaryEntry pair in hash){
			// Debug.Log("pair.Value:"+pair.Value);
			string valS;
			var val = pair.Value;
			if(val.GetType()==typeof(float)){
				valS = ""+val;
			}else{
				valS = val as string;
			}
			postParams.Add((string)pair.Key, valS);
		}
		return this;
	}

	public LLOptions setOnLoadParam( Hashtable hash ){
		this.onLoadParam = new Dictionary<string, object>();
		foreach (DictionaryEntry pair in hash){
			this.onLoadParam.Add((string)pair.Key, (object)pair.Value);
		}
		return this;
	}
	#endif

	/**
	* Set the use of caching. On subsequent calls if the cache-life is still valid, it will retrieve it from the cache first
	* 
	* @method LLOptions().setUseCache
	* @param {boolean} useCache:boolean boolean determining whether or not to cache this item (and retrieve from cache in subsequent calls)
	*/
	public LLOptions setUseCache( bool useCache ){
		this.useCache = useCache;
		return this;
	}

	public LLOptions setUseFile( bool useFile ){
		this.useFile = useFile;
		return this;
	}

	/**
	* This mode of caching only pulls up the cache as a backup (if internet is not available, or it has an issue reaching the asset)
	* 
	* @method LLOptions().setUseCacheAsBackup
	* @param {boolean} useCacheAsBackup:boolean boolean determining whether or not to retrieve a cached version as a backup to getting the most up-to-date version
	*/
	public LLOptions setUseCacheAsBackup( bool useCacheAsBackup ){
		this.useCacheAsBackup = useCacheAsBackup;
		return this;
	}

	/**
	* Set the cache life of the asset (in seconds)
	* 
	* @method LLOptions().setCacheLife
	* @param {int} cacheLife:int int value in seconds specifying the life-time of the cached asset. Once this value is exceded a new version will be retrieved.
	*/
	public LLOptions setCacheLife( int cacheLife ){
		this.cacheLife = cacheLife;
		return this;
	}

	public LLOptions setOnLoadParam( Dictionary<string,object> hash ){
		this.onLoadParam = hash;
		return this;
	}

	public void savePostValues(){
		if(postParams!=null){
			StringBuilder strBuild = new StringBuilder(100);

			foreach (var key in postParams.Keys){
				strBuild.Append("&");
				strBuild.Append((string)key);
				strBuild.Append("=");
				strBuild.Append((string)postParams[key]);
			}

			cacheName = strBuild.ToString();
		}
	}

	public static Dictionary<string,object> dictionary( Dictionary<string,object> table ){
		Dictionary<string, object> d = new Dictionary<string, object>();
		foreach (var key in table.Keys){
			string valS;
			var val = table[key];
			if(val.GetType()==typeof(float)){
				valS = ""+val;
			}else{
				valS = val as string;
			}
			// Debug.Log("key:"+key+" value:"+valS);
			d.Add((string)key, valS);
		}
		return d;
	}
}

/**
* Helper class for saving large files over a period of time, to avoid frame-rate hiccups
*
* @class LLStreamingSave
* @constructor
*/

public class LLStreamingSave : MonoBehaviour{

	private static char strSplitChar = "|"[0];
	private StringBuilder strBuild;

	/**
	* The amount of loops that are spent on every update decoding audio data (decrease if you are having performance issues)
	* 
	* @property AUDIO_DECODE_EVERY
	* @type {int}
	* @default 40000
	*/
	public static int AUDIO_DECODE_EVERY = 40000;
	/**
	* The amount of loops that are spent on every update encoding audio date (decrease if you are having performance issues)
	* 
	* @property AUDIO_ENCODE_EVERY
	* @type {int}
	* @default 5000
	*/
	public static int AUDIO_ENCODE_EVERY = 5000;

	public void save( AudioClip audio, LLOptions options ){
		StartCoroutine( serializeAudio( audio, options ) );
	}

	public void retrieve( string str, LLOptions options ){
		StartCoroutine( deSerializeAudio( str, options ) );
	}

	IEnumerator serializeAudio( AudioClip audio, LLOptions options ) {
		int lengthSamples = audio.samples * audio.channels;

		strBuild = new StringBuilder(lengthSamples*10+100);
		string str;
		float[] samples = new float[lengthSamples];
		audio.GetData(samples, 0);
		int i = 0;
		int j = 1;
		while (i < samples.Length) {
        	if(j%AUDIO_ENCODE_EVERY==0){
        		// Debug.Log("processing");
        		yield return true;
        	}
			if(samples[i]<0.0f){
				str = samples[i].ToString("F7");
			}else{
				str = samples[i].ToString("F8");
			}
			strBuild.Append( str );
			++i;
			j++;
		}
		
		strBuild.Append("|" + audio.name);
		strBuild.Append("|" + lengthSamples);
		strBuild.Append("|" + audio.channels);
		strBuild.Append("|" + audio.frequency);
		strBuild.Append("|" + samples.Length);

		try {
			PlayerPrefs.SetString(options.cacheName, strBuild.ToString());	
		}catch (PlayerPrefsException err) {
			Debug.Log("Exceded Storage Limit (only usually issue on the Web Player) error: " + err);
			if(options.onError!=null)
		        options.onError( err.ToString() );
		}

		Destroy( this );
	}

	IEnumerator deSerializeAudio( string str, LLOptions options ){
		string[] splitGroup = str.Split( strSplitChar );

		string aName = splitGroup[1];
		int lengthSamples = int.Parse( splitGroup[2] );
		int channels = int.Parse( splitGroup[3] );
		int frequency = int.Parse( splitGroup[4] );
		//Debug.Log("name:"+aName+" len:"+lengthSamples+" channels:"+channels+" freq:"+frequency+" samplesLength:"+samplesLength);
		
		#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6
		AudioClip audio = AudioClip.Create(aName, lengthSamples/channels, channels, frequency, false);
		#else
		AudioClip audio = AudioClip.Create(aName, lengthSamples/channels, channels, frequency, true, false);
		#endif

		string audioStr = splitGroup[0];
		
		float[] data = new float[lengthSamples];
		
		int i = 0;
		int k = 0;
		int j = 1;
        while (i < audioStr.Length) {
        	if(j%AUDIO_DECODE_EVERY==0){
        		// Debug.Log("processing");
        		options.ll.fakeProgress = i*1.0f/audioStr.Length;
        		yield return true;
        	}
            data[k] = float.Parse( audioStr.Substring(i, 10) );
            i+=10;
            k++;
            j++;
        }
        audio.SetData( data, 0 );

        if(options.onAudioLoadP!=null){
			options.onAudioLoadP( audio, options.onLoadParam );
		}else{
			options.onAudioLoad( audio );
		}
		options.ll.fakeProgress = 1.0f;
		
		Destroy( this );
	}
}

/**
* Keep track of the loading progress of the asset, as well as other attributes
*
* @class LeanLoading
* @constructor
*/
public class LeanLoading : object {

	/**
	* The name the asset is stored as (useful for deleting later)
	* 
	* @property cacheName
	* @type {string}
	*/
	public string cacheName;
	/**
	* Whether the cache being used for this asset 
	* 
	* @property cacheUsed
	* @default false
	* @type {boolean}
	*/
	public bool cacheUsed;
	/**
	* The WWW object that is used to load the object (in case you need to access anything specific) 
	* 
	* @property www
	* @type {WWW}
	*/
	public WWW www;
	public float fakeProgress;

	public LeanLoading(){
		fakeProgress = -1.0f;
	}

	/**
	* How much of the asset has loaded (from 0.0f to 1.0f)
	* 
	* @property progress
	* @type {float}
	* @default 0.0f
	*/
	public float progress{
		get{
			if(www!=null){
				return www.progress;
			}else if(fakeProgress>=0.0f){
				return fakeProgress;
			}else{
				return 0.0f;
			}
		}
	}

}

/**
* LeanLoader v1.2 - Load assets from the web and cache them for faster performance and offline use
*
* @class LeanLoader
* @constructor
*/
public class LeanLoader : MonoBehaviour {

	private const string CACHE_KEYS = "LLCacheKeys";

	private static Dictionary<string, int> allKeys;
	private static char strSplitChar = "|"[0];
	private static int cleanupIter = 0;
	public static int CLEANUP_BLOCK_SIZE = 5;

	private static LeanLoader reference;
	private static GameObject staticGo;
	private const string uniqueClassName = "~LeanLoader";

	private static List<IEnumerator> coroutines;

	public void Awake() {
        if (allKeys == null) {
            allKeys = retrieveHashtable(CACHE_KEYS);
        }
    }

	public LeanLoader(){
		
	}

	private static void init(){
		if(staticGo==null){
			staticGo = GameObject.Find(uniqueClassName);
			if(staticGo==null){
				staticGo = new GameObject( uniqueClassName );
				reference = staticGo.AddComponent(typeof(LeanLoader)) as LeanLoader;
				staticGo.isStatic = true;
				DontDestroyOnLoad( staticGo );
				//staticGo.hideFlags = HideFlags.HideInHierarchy;
			}else{
				reference = staticGo.GetComponent(typeof(LeanLoader)) as LeanLoader;
			}

			coroutines = new List<IEnumerator>();
		}
	}

	public static int count{
		get{ 
			return allKeys.Count;
		}
	}

	private static void saveHashtable( Dictionary<string, int> dict, string name ){
		int count = dict.Keys.Count;
		StringBuilder strBuild = new StringBuilder(count*20);
		int i = 0;
		foreach(KeyValuePair<string, int> entry in dict){
	        strBuild.Append( entry.Key );
	        if(i<count-1)
				strBuild.Append( strSplitChar );

			i++;
		}
		PlayerPrefs.SetString(name, strBuild.ToString()); 
	}

	private static Dictionary<string, int> retrieveHashtable( string name ){
		string str = PlayerPrefs.GetString(name);
		// Debug.Log("saved keys:"+str);
		if(str==null)
			return new Dictionary<string, int>();

		string[] splitGroup = str.Split( strSplitChar );
		Dictionary<string, int> dict = new Dictionary<string, int>();
		for(int i = 0; i < splitGroup.Length; i++){
			dict[ splitGroup[i] ] = 1;
		}

		return dict;
	}

	private static System.DateTime getLastSavedTime( string dataURL ){
		string keyDate = dataURL+"_d";
		if(PlayerPrefs.HasKey( keyDate )){
			string date = PlayerPrefs.GetString(keyDate);
			string[] dateSplit = date.Split(":"[0]);
			if(dateSplit.Length>6){
				System.DateTime dateTime = new System.DateTime(System.Int32.Parse(dateSplit[0]), System.Int32.Parse(dateSplit[1]), System.Int32.Parse(dateSplit[2]), System.Int32.Parse(dateSplit[3]), System.Int32.Parse(dateSplit[4]), System.Int32.Parse(dateSplit[5]), System.Int32.Parse(dateSplit[6]) );
				return dateTime;
			}
		}
		return System.DateTime.Now;
	}

	/**
	* Remove a specific cached item
	* 
	* @method LeanLoader.deleteCache
	* @param {string} dataURL:String The path of the asset you want to delete from the cache
	*/
	public static void deleteCache( string dataURL ){
		PlayerPrefs.DeleteKey(dataURL); 
		PlayerPrefs.DeleteKey(dataURL+"_w");
		PlayerPrefs.DeleteKey(dataURL+"_h");
		PlayerPrefs.DeleteKey(dataURL+"_d");
		allKeys.Remove(dataURL);
	}

	/**
	* Removes all of the LeanLoader cached items from the PlayerPrefs
	* 
	* @method LeanLoader.deleteCacheAll
	*/
	public static void deleteCacheAll(){
		init();

		Dictionary<string, int> copyKeys = new Dictionary<string, int>( allKeys );
		foreach(KeyValuePair<string, int> entry in copyKeys ){
	        deleteCache( entry.Key );
		}

		PlayerPrefs.DeleteKey( CACHE_KEYS ); 
		allKeys.Clear();
	}

	public static string escapedName( string url ){
		string escName = WWW.EscapeURL( url );
		return Application.persistentDataPath + "/" + escName;
	}

	/**
	* Load an asset from the internet
	* 
	* @method LeanLoader.load
	* @param {String} dataURL:String URL of the data you wish to load
	* @param {LLOptions} options:LLOptions The optional parameters you wish to specify for the loading of the asset (use caching, onComplete handlers, etc...)
	* @return {LeanLoading} Returns a LeanLoading object where you can track the assets progress, and other attributes
	* @example <i>Javascript:</i><br>
	* LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", LLOptions().setOnComplete( imageLoaded ).setUseCache( true ));
	* function imageLoaded( tex:Texture2D ){<br>
	* 	Debug.Log("image loaded:"+tex);<br>
	* }<br>
	* <br><br>
	* <i>C#: </i> <br>
	* LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", new LLOptions().setOnComplete( imageLoaded ).setUseCache( true ));
	* void imageLoaded( Texture2D tex ){<br>
	* 	Debug.Log("image loaded:"+tex);<br>
	* }<br>
	* <br><br>
	*/
	public static LeanLoading load( string dataURL, LLOptions options ){ 
		init();

		// Sanitize string
		dataURL = dataURL.Replace(" ", "%20");

		if(options.useCache || options.useCacheAsBackup){
			options.savePostValues();
		}
		if(!string.IsNullOrEmpty(options.cacheName) ){
			options.cacheName = dataURL+options.cacheName;
		}else{
			options.cacheName = dataURL;
		}
		// Debug.Log("options.cacheName:"+options.cacheName);
		
		bool cacheValid = options.useCache;
		if(options.useCacheAsBackup==false && options.useCache && PlayerPrefs.HasKey(options.cacheName+"_d")){
			System.DateTime dateWhenExpired = getLastSavedTime( options.cacheName );
			int hasExpired = System.DateTime.Now.CompareTo( dateWhenExpired );
			// Debug.Log("hasExpired:"+hasExpired + " dateWhenExpired:"+dateWhenExpired);
			cacheValid = hasExpired < 0;
		}
		options.isImage = options.onImageLoad!=null || options.onImageLoadP!=null;
		options.isText = options.onTextLoad!=null || options.onTextLoadP!=null;
		options.isJSON = options.onJSONLoad!=null || options.onJSONLoadP!=null;
		options.isAudio = options.onAudioLoad!=null || options.onAudioLoadP!=null;

		LeanLoading ll = new LeanLoading();
		ll.cacheUsed = cacheValid;
		ll.cacheName = options.cacheName;
		options.ll = ll;

		bool doesExist = false;
	#if !UNITY_WEBPLAYER
		doesExist = options.useFile ? File.Exists( escapedName( options.cacheName ) ) : PlayerPrefs.HasKey(options.cacheName);
	#else
		doesExist = PlayerPrefs.HasKey(options.cacheName);
	#endif
		if(options.useCacheAsBackup==false && cacheValid && doesExist){
			loadFromCache( options );
		}else{
			reference.startSendData( dataURL, options );
		}

		return ll;
	}

	private void cancelAllInternal(){
		for (int i = 0; i < coroutines.Count; i++){
		    StopCoroutine( coroutines[i] );
		}

		coroutines.Clear();
	}

	public static void cancelAll(){
		reference.cancelAllInternal();
	}

	public static void loadFromCache( string dataURL, LLOptions options ){
		options.cacheName = dataURL;
		loadFromCache( options );
	}

	private static void loadFromCache( LLOptions options ){
		if(options.isImage){
			string temp = PlayerPrefs.GetString(options.cacheName);
			int width = PlayerPrefs.GetInt(options.cacheName+"_w");
			int height = PlayerPrefs.GetInt(options.cacheName+"_h");
			Texture2D tex = new Texture2D(width,height,TextureFormat.RGB24, false);

			byte[] byteArray;
		#if !UNITY_WEBPLAYER
			if(options.useFile){
				byteArray = File.ReadAllBytes( escapedName( options.cacheName ) );
			}else{
				byteArray = System.Convert.FromBase64String(temp);
			}
		#else
			byteArray = System.Convert.FromBase64String(temp);
		#endif
			tex.LoadRawTextureData(byteArray);

			if(options.onImageLoadP!=null){
				options.onImageLoadP( tex, options.onLoadParam );
			}else{
				options.onImageLoad( tex );
			}
			options.ll.fakeProgress = 1.0f;
	   }else if(options.isText){
		   	string temp = PlayerPrefs.GetString(options.cacheName);
		   	if (temp == null || temp == ""){
                options.onError("cache is empty");
            }else{
            	if(options.onTextLoadP!=null){
					options.onTextLoadP( temp, options.onLoadParam );
				}else{
					options.onTextLoad( temp );
				}
				options.ll.fakeProgress = 1.0f;
            }
		   
	   }else if(options.isJSON){
	   		string temp = PlayerPrefs.GetString(options.cacheName);
		   	if(options.onJSONLoadP!=null){
				options.onJSONLoadP( new LeanJSON( temp ), options.onLoadParam );
			}else{
				options.onJSONLoad( new LeanJSON( temp ) );
			}
			options.ll.fakeProgress = 1.0f;
	   }else if(options.isAudio){
	   		string temp = PlayerPrefs.GetString(options.cacheName);
	   		LLStreamingSave save = staticGo.AddComponent( typeof(LLStreamingSave) ) as LLStreamingSave;
			save.retrieve( temp, options );
	   }
	}

	private void startSendData( string dataURL, LLOptions options ){
		IEnumerator newCoroutine = sendData( dataURL, options );
		coroutines.Add( newCoroutine );
		StartCoroutine( newCoroutine );
	}

	IEnumerator sendData( string dataURL, LLOptions options ){
		WWWForm sendForm = null;

		if(options.postParams!=null){
			sendForm = new WWWForm();
		    foreach(KeyValuePair<string, object> entry in options.postParams){
		    	sendForm.AddField((string)entry.Key, ""+entry.Value );
			}
		}

		WWW www;
		if(sendForm!=null){
			www = new WWW(dataURL, sendForm); 
		}else{
			www = new WWW(dataURL);
		}
		
		options.ll.www = www;
		yield return www;

		if(!string.IsNullOrEmpty(www.error)) {
	        Debug.Log("Error: " + www.error + " while loading:"+dataURL);
	        if(options.onError!=null)
		        options.onError( www.error );

		    if(options.useCacheAsBackup || options.useCache)
		    	loadFromCache( options );
	    } else {
	        // Debug.Log("Receive response: " + www.text + " isImage:"+options.isImage);
	        if(options.isImage){
	        	if(www.text[0] == "{"[0]){ // Work around for Facebook profile images, that provide a redirect
					LeanJSON j = new LeanJSON( www.text );
	        		LeanJSON d = j.Object("data");
					string picUrl = d["url"];
					startSendData(picUrl, options);
	        	}else{
					Texture2D tex = www.texture;

					if(options.useCache||options.useCacheAsBackup){
						yield return new WaitForEndOfFrame();

						byte[] byteArray = tex.GetRawTextureData();
						yield return new WaitForEndOfFrame();
						string imgRepStr = System.Convert.ToBase64String(byteArray);
						yield return new WaitForEndOfFrame();

						try {
						#if !UNITY_WEBPLAYER
							if(options.useFile){
								File.WriteAllBytes( escapedName( options.cacheName ), byteArray);

//								string fullPath = escapedName( options.cacheName );
//
//						        using (FileStream fs = File.Create(fullPath, FileMode.OpenOrCreate))
//						        {
//						            fs.Seek(0, SeekOrigin.End);
//						            await fs.WriteAsync(byteArray, 0, 512);
//						        }
							}else{
								PlayerPrefs.SetString(options.cacheName, imgRepStr); 
							}
						#else
							PlayerPrefs.SetString(options.cacheName, imgRepStr);
						#endif
							
							PlayerPrefs.SetInt(options.cacheName+"_w", tex.width);
							PlayerPrefs.SetInt(options.cacheName+"_h", tex.height);
							saveTime( options.cacheName, options.cacheLife );

							allKeys[ options.cacheName ] = 1;
							saveHashtable( allKeys, CACHE_KEYS );
						}catch (PlayerPrefsException err) {
							Debug.Log("Exceded Storage Limit (only usually issue on the Web Player) error: " + err);
							if(options.onError!=null)
						        options.onError( err.ToString() );
						}
					}
					yield return new WaitForEndOfFrame();
					if(options.onImageLoadP!=null){
						options.onImageLoadP( tex, options.onLoadParam );
					}else{
						options.onImageLoad( tex );
					}
				}
	        }else if(options.isText || options.isJSON){
	        	string response = www.text;

	        	if(options.useCache||options.useCacheAsBackup){
		        	try {
						PlayerPrefs.SetString(options.cacheName, response);
						saveTime( options.cacheName, options.cacheLife ); 

						allKeys[ options.cacheName ] = 1;
						saveHashtable( allKeys, CACHE_KEYS );
					}catch (PlayerPrefsException err) {
						Debug.Log("Exceded Storage Limit (only usually issue on the Web Player) error: " + err);
						if(options.onError!=null)
					        options.onError( err.ToString() );
					}
				}
				if(options.isText){
					if(options.onTextLoadP!=null){
						options.onTextLoadP( response, options.onLoadParam );
					}else{
						options.onTextLoad( response );
					}
				}else{
					if(options.onJSONLoadP!=null){
						options.onJSONLoadP( new LeanJSON( response ), options.onLoadParam );
					}else{
						options.onJSONLoad( new LeanJSON( response ) );
					}
				}
	        }else if(options.isAudio){
	        	AudioClip response = www.audioClip;

	        	if(options.useCache||options.useCacheAsBackup){
		        	try {
						//PlayerPrefs.SetString(options.cacheName, serializeAudio(response));
						LLStreamingSave save = gameObject.AddComponent( typeof(LLStreamingSave) ) as LLStreamingSave;
						save.save( response, options );

						saveTime( options.cacheName, options.cacheLife ); 

						allKeys[ options.cacheName ] = 1;
						saveHashtable( allKeys, CACHE_KEYS );
					}catch (PlayerPrefsException err) {
						Debug.Log("Exceded Storage Limit (only usually issue on the Web Player) error: " + err);
						if(options.onError!=null)
					        options.onError( err.ToString() );
					}
				}

				if(options.onAudioLoadP!=null){
					options.onAudioLoadP( response, options.onLoadParam );
				}else{
					options.onAudioLoad( response );
				}
	        }

	        if(options.useCache || options.useCacheAsBackup){
				// Cleanup unused items
				int i = 0;
				int iMax = i + CLEANUP_BLOCK_SIZE;
				var enumerator = allKeys.GetEnumerator();
				List<string> itemsToDelete = new List<string>();
				// Debug.Log("System.DateTime.Now:"+System.DateTime.Now+" count:"+allKeys.Count);
				while (enumerator.MoveNext() && i < iMax){
					if(i >= cleanupIter){
						var pair = enumerator.Current;
						string key = pair.Key;
				   		System.DateTime dateWhenExpired = getLastSavedTime( key );
				   		// Debug.Log("dateWhenExpired:"+dateWhenExpired);
						int hasExpired = System.DateTime.Now.CompareTo( dateWhenExpired );
						// Debug.Log("key:"+key+" hasExpired:"+hasExpired);
						if(hasExpired > 0){
							itemsToDelete.Add(key);
						}
					}
					i++;
				}
				// actually delete
				for(i = 0; i < itemsToDelete.Count; i++){
					deleteCache( itemsToDelete[i] ); 
				}
				cleanupIter += CLEANUP_BLOCK_SIZE;
				if(cleanupIter>allKeys.Count+CLEANUP_BLOCK_SIZE)
					cleanupIter = 0;
			}
	    }
	}

	private void saveTime( string dataURL, int lifeTime ){
		System.DateTime future = System.DateTime.Now.Add( new System.TimeSpan(0, 0, lifeTime ) );

		PlayerPrefs.SetString(dataURL+"_d", ""+future.Year +":"+ future.Month +":"+ future.Day +":"+ future.Hour +":"+ future.Minute +":"+ future.Second +":"+ future.Millisecond); 
		PlayerPrefs.Save();
	}
}
	


