using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;


public class Shooter3DNetworkManager : MonoBehaviour
{
   
	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static Shooter3DNetworkManager instance;
	
	//Variable that defines comma character as separator
	static private readonly char[] Delimiter = new char[] {':'};

	//store localPlayer
	public GameObject myPlayer;
	
	//store all players in game
	public Dictionary<string, VehicleManager> networkPlayers = new Dictionary<string, VehicleManager>();
	
	//store the local players' models
	public GameObject[] localPlayersPrefabs;
	
	//store the networkplayers' models
	public GameObject[] networkPlayerPrefabs;

	//stores the spawn points 
	public Transform[] spawnPoints;
	
	public IsometricCameraFollow cameraFollow;
	
	public  AudioClip screamSound;
	
	public bool isGameOver;
	
	int index;
	
	float startTime;
	
	public float endTime;
	
	bool waitingSearch;
	

	void Awake()
	{
		Application.ExternalEval("socket.isReady = true;");
		
	}
	
	// Use this for initialization
	void Start () {
	
	 // if don't exist an instance of this class
	 if (instance == null) {

		//it doesn't destroy the object, if other scene be loaded
		DontDestroyOnLoad (this.gameObject);

		instance = this;// define the class as a static variable
		
		
	 }
	 else
	 {
		//it destroys the class if already other class exists
		Destroy(this.gameObject);
	 }
		
	}
	
	
	

	void Update(){
	
	
	  //tries to obtain a "pong" of some local server
	  StartCoroutine ("PingPong");
	
	}
	
	/// <summary>
	/// corroutine called  of times in times to send a ping to the server
	/// </summary>
	/// <returns>The pong.</returns>
	private IEnumerator PingPong()
	{

		if (waitingSearch)
		{
			yield break;
		}

		waitingSearch = true;

		//sends a ping to server
		EmitPing ();

		

		// wait 1 seconds and continue
		yield return new WaitForSeconds(4);

		waitingSearch = false;

	}




	/// <summary>
	///  receives an answer of the server.
	/// from  void OnReceivePing(string [] pack,IPEndPoint anyIP ) in server
	/// </summary>
	public void OnPrintPongMsg(string data)
	{

		/*
		 * pack[0]= totalPlayers
		*/

	    var pack = data.Split (Delimiter);
		
		float latency = Time.time - startTime;// latency in seconds
		
		latency = latency*1000; //latency in ms
		
		
		Shooter3DCanvasManager.instance.txtPing.text = latency.ToString("N2")+" ms";
		
		Shooter3DCanvasManager.instance.txtTotalPlayers.text = pack[0];
		
		
	}

	// <summary>
	/// sends ping message to UDPServer.
	///     case "PING":
	///     OnReceivePing(pack,anyIP);
	///     break;
	/// take a look in TicTacttoeServer.cs script
	/// </summary>
	public void EmitPing() {

			//hash table <key, value>	
		Dictionary<string, string> data = new Dictionary<string, string>();

		//store "ping!!!" message in msg field
	    data["msg"] = "ping!!!!";
		
		startTime = Time.time;

		JSONObject jo = new JSONObject (data);

		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "PING",new JSONObject(data));

		

	}
	

	/// <summary>
	/// Emits the join game to Server.
	/// case "JOIN_GAME":
	///   OnReceiveJoinGame(pack,anyIP);
	///  break;
	/// take a look in Server.cs script
	/// </summary>
	public void EmitJoinGame()
	{
	 
	  //hash table <key, value>	
	  Dictionary<string, string> data = new Dictionary<string, string>();

		
		//send the position point to server
		string msg = string.Empty;

		if (!isGameOver) {


		
		 //player's name	
		 data["name"] = Shooter3DCanvasManager.instance.inputLogin.text;
			  
	     data["avatar"] = VehicleChooseManager.instance.currentVehicle.ToString();
		 
		 //makes the draw of a point for the player to be spawn
		 index = UnityEngine.Random.Range (0, spawnPoints.Length);

		 data["dx"] = spawnPoints[index].position.x.ToString();
		 
		  data["dz"] = spawnPoints[index].position.z.ToString();
			  
			
		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "JOIN_ROOM",new JSONObject(data));

		
		}
		else
		{
			
		   isGameOver = false;
		   
			 //player's name	
		    data["name"] = Shooter3DCanvasManager.instance.inputLogin.text;
			  
	        data["avatar"] = VehicleChooseManager.instance.currentVehicle.ToString();
		 
		    //makes the draw of a point for the player to be spawn
		    index = UnityEngine.Random.Range (0, spawnPoints.Length);

		    data["dx"] = spawnPoints[index].position.x.ToString();
			
			data["dz"] = spawnPoints[index].position.z.ToString();
			
			
			//sends to the nodejs server through socket the json package
		    Application.ExternalCall("socket.emit", "RESPAW",new JSONObject(data));

		}


	}


	/// <summary>
	/// Raises the join game event from Server.
	/// only the first player to connect gets this feedback from the server
	/// </summary>
	/// <param name="data">Data.</param>
	void OnJoinGame(string data)
	{
			/*
		 * pack[0] = id (local player id)
		 * pack[1]= name (local player name)
		 * pack[2]= avatar 
		
		 
		*/
		
		Debug.Log ("\n joining ...\n");
		
		var pack = data.Split (Delimiter);

		
		Debug.Log("Login successful, joining game");


		if (!myPlayer) {
		
		
		  

			// take a look in VehicleManager.cs script
			VehicleManager newPlayer;
			
			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (localPlayersPrefabs [int.Parse(pack [2])],spawnPoints[index].localPosition,
			localPlayersPrefabs [int.Parse(pack [2])].gameObject.transform.rotation).GetComponent<VehicleManager> ();
			 
			newPlayer.id = pack[0];
			
			newPlayer.name = pack[1];
			
			newPlayer.avatar = pack[2];

			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;
			
			newPlayer.gameObject.name = pack[0];
			  
			//puts the local player on the list
			networkPlayers [ pack[0]] = newPlayer;

			myPlayer = networkPlayers [ pack[0]].gameObject;
			
			cameraFollow.SetTarget(newPlayer.gameObject.transform);
		
		    Shooter3DCanvasManager.instance.txtLocalPlayerHealth.text = "100";
			  
			Shooter3DCanvasManager.instance.OpenScreen(1);
			  
			Debug.Log("player instantiated");

			
			Debug.Log("player in game");
		}

	}
	
	/// <summary>
	/// Raises the spawn player event.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnSpawnPlayer(string data)
	{

		/*
		 * pack[0] = id
		 * pack[1] = name
		 * pack[2]= avatar
		 * pack[3] = dx
		 * pack[4] = dz
		*/
		
		var pack = data.Split (Delimiter);
		
		try{
		
		Debug.Log ("\n spawning network player ...\n");
		

	    bool alreadyExist = false;

			
	    if(networkPlayers.ContainsKey(pack [0]))
	    {
			alreadyExist = true;
		}
		if (!alreadyExist) {
			
		  VehicleManager newPlayer;

		  newPlayer =  GameObject.Instantiate (networkPlayerPrefabs [int.Parse(pack[2])],new Vector3(float.Parse(pack [3]),
				spawnPoints[0].position.y,float.Parse(pack [4])),
			  Quaternion.identity).GetComponent<VehicleManager> ();
			  
		  
		  //it is not the local player
		  newPlayer.isLocalPlayer = false;

		  //network player online in the arena
		  newPlayer.isOnline = true;

		  newPlayer.gameObject.name = pack [0];
				
		  newPlayer.name = pack[1];
				
		  newPlayer.avatar = pack[2];
				
		  newPlayer.Set3DName(pack[1]);

		  //puts the local player on the list
		  networkPlayers [pack [0]] = newPlayer;
			
		}//END_IF
			
	  }//END_TRY
	  catch(Exception e)
	  {
		//Debug.Log(e.ToString());
		//Shooter3DCanvasManager.instance.txtLog.text = e.ToString();
	  }

	}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////PLAYER POSITION AND ROTATION UPDATES///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//send local player position and rotation to server
	public void EmitPosAndRot(Dictionary<string, string> data)
	{
	    JSONObject jo = new JSONObject (data);

	    //sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "POS_AND_ROT",new JSONObject(data));

	}

	/// <summary>
	/// Update the network player position and rotation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdatePosAndRot(string data)
	{
		
	    /*
		 * pack[0] = id
		 * pack[1] = dx
		 * pack[2] = dz
		 * pack[3] = rotation
		*/
		if(networkPlayers.Count > 0)
	  {
		try{
		var pack = data.Split (Delimiter);

	   if (networkPlayers [pack [0]] != null)
		{
		  
		  VehicleManager netPlayer = networkPlayers[pack [0]];
		 
		  //update with the new position
		  netPlayer.UpdatePosition(float.Parse(pack [1]),float.Parse(pack [2]));
	      //update new player rotation
		  netPlayer.UpdateRotation(pack[3]);
			
		}
		}
		catch(Exception e)
		{
		  // Shooter3DCanvasManager.instance.txtLog.text = e.ToString();
		}
	 }
	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////SHOOT UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



	public void EmitShoot()
	{
		
		 //sends to the nodejs server through socket the json package
		 Application.ExternalCall("socket.emit", "SHOOT");
		
	}

    void OnUpdateShoot(string data)
	{
		
		/*
		 * data.pack[0] = id
		
		*/
		
		var pack = data.Split (Delimiter);

		if( networkPlayers[pack[0]]!=null)
		{

		   VehicleManager netPlayer = networkPlayers[pack[0]];
		   netPlayer.gameObject.GetComponentInChildren<Gun3D> ().NetworkShoot();
	

		}
	}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////BEST KILLERS UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


public void EmitGetBestKillers()
{
			
		 //sends to the nodejs server through socket the json package
		 Application.ExternalCall("socket.emit", "GET_BEST_KILLERS");
	
}
	
void OnUpdateBestKillers(string data)
{
	    
	    /*
		 * pack[0] = name
		 * pack[1] = ranking
		 * pack[2] = kills
		*/
		var pack = data.Split (Delimiter);

		Debug.Log("received best players from server ...");
	
		int ranking = int.Parse (pack[1]) + 1;
		
		Shooter3DCanvasManager.instance.SetUpBestKiller(pack[0], pack[2], ranking.ToString());

}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////DAMAGE UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  public void EmitPlayerDamage(string _shooter_id, string _target_id)
  {
  
	 //hash table <key, value>
	 Dictionary<string, string> data = new Dictionary<string, string>();

	 data ["shooter_id"] = _shooter_id;
		
	 data ["target_id"] = _target_id;
	 
	  //sends to the nodejs server through socket the json package
	  Application.ExternalCall("socket.emit", "DAMAGE",new JSONObject(data));
	 

  }
  
  /// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdatePlayerDamage(string data)
	{
		
	 /*
		 * data.pack[0] = id
		 * data.pack[1] = health
		
		*/
		
	  if(networkPlayers.Count > 0)
	  {
		var pack = data.Split (Delimiter);
		
	   //find network player by your id
	   VehicleManager netPlayer = networkPlayers[pack[0]];

	   if(networkPlayers[pack[0]].isLocalPlayer)
	   {
		    Shooter3DCanvasManager.instance.txtLocalPlayerHealth.text = pack[1];
			
			Shooter3DCanvasManager.instance.damaged = true;
	   }
	  }
		
	}
	
	
	  /// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnGameOver(string data)
	{
		
		/*
		 * pack[0] = target_id,
		 * pack[1] = shooter_id
		 * pack[2] = shooter_kills
	
		*/
	if(networkPlayers.Count > 0)
	{
	  var pack = data.Split (Delimiter);
	  
	  try
	  {
		
		
		//find target by your id
		VehicleManager target = networkPlayers[pack[0]];
		
		VehicleManager shooter = networkPlayers[pack[1]];
		
		
		if(networkPlayers[pack[0]].isLocalPlayer)
		{
		    ResetGame(); 
		}
		else
		{
		   
		  if(networkPlayers[pack[1]].isLocalPlayer)
		  {
		    Shooter3DCanvasManager.instance.txtKills.text = pack[2];
		   
		     PlayScreamSound();
		  }
		  
		  Destroy (networkPlayers [pack[0]].gameObject);
		  
		  networkPlayers.Remove (pack[0]);
				
		}
		
		
		
	  }//END_TRY
	  catch(Exception e)
	  {
		 Debug.Log(e.ToString());
	  }
	}
		
	}
	
	void ResetGame()
	{
	    Debug.Log("reset game");
		
		isGameOver = true;
		
		Shooter3DCanvasManager.instance.ShowMessage("You Lose :(");
		
		myPlayer = null;
		
		//send answer in broadcast
		foreach (KeyValuePair<string, VehicleManager> entry in networkPlayers) {

		  Destroy(entry.Value.gameObject);
		  
		}//END_FOREACH
		
		networkPlayers.Clear();
		
		Shooter3DCanvasManager.instance.txtLocalPlayerHealth.text = "100";
	
		
		Shooter3DCanvasManager.instance.OpenScreen(0);
		
		
	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////




	void OnApplicationQuit() {

		Debug.Log("Application ending after " + Time.time + " seconds");

	}
	
	/// <summary>
	/// inform the local player to destroy offline network player
	/// </summary>
	/// <param name="_msg">Message.</param>
	//desconnect network player
	void OnUserDisconnected(string data )
	{

		/*
		 * data.pack[0] = id (network player id)
		*/

		var pack = data.Split (Delimiter);
		
		Debug.Log("user: "+networkPlayers[pack[0]].name+" disconnected");
 
		if (networkPlayers [pack [0]] != null)
		{


			//destroy network player by your id
			Destroy( networkPlayers[pack[0]].gameObject);


			//remove from the dictionary
			networkPlayers.Remove(pack[0]);

		}
		
		


	}

	/// <summary>
	/// inform the local player to destroy offline network player
	/// </summary>
	/// <param name="_msg">Message.</param>
	

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////HELPERS////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	Vector3 StringToVector3(string target ){

		Vector3 newVector;
		string[] newString = Regex.Split(target,";");
		newVector = new Vector3( float.Parse(newString[0]), float.Parse(newString[1]),float.Parse(newString[2]));

		return newVector;

	}
	
	Vector4 StringToVector4(string target ){

		Vector4 newVector;
		string[] newString = Regex.Split(target,";");
		newVector = new Vector4( float.Parse(newString[0]), float.Parse(newString[1]),float.Parse(newString[2]),float.Parse(newString[3]));

		return newVector;

	}
	
	string Vector3ToString(Vector3 vet ){

		return  vet.x+";"+vet.y+";"+vet.z;

	}
	
	string Vector4ToString(Vector4 vet ){

		return  vet.x+";"+vet.y+";"+vet.z+";"+vet.w;

	}
	
	
	Vector3 JsonToVector3(string target ){

	   
		Vector3 newVector = new Vector3(0,0,0);
		string[] newString = Regex.Split(target,";");
		//#if UNITY_EDITOR 
		newVector = new Vector3( float.Parse(newString[0]), float.Parse(newString[1]),float.Parse(newString[2]));
		
        /*#else
		
		CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
         ci.NumberFormat.CurrencyDecimalSeparator = ",";
		 new Vector3( float.Parse(newString[0],NumberStyles.Any,ci), float.Parse(newString[1],NumberStyles.Any,ci),float.Parse(newString[2],NumberStyles.Any,ci));
	
        #endif
 */
		return newVector;

	}

	Vector4 JsonToVector4(string target ){

		Vector4 newVector = new Vector4(0,0,0,0);
		string[] newString = Regex.Split(target,";");
	//	#if UNITY_EDITOR 
		newVector = new Vector4( float.Parse(newString[0]), float.Parse(newString[1]), float.Parse(newString[2]),float.Parse(newString[3]));
	
     /*   #else
		
		CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
         ci.NumberFormat.CurrencyDecimalSeparator = ",";
		newVector = new Vector4( float.Parse(newString[0],NumberStyles.Any,ci), float.Parse(newString[1],NumberStyles.Any,ci),float.Parse(newString[2],NumberStyles.Any,ci
		),float.Parse(newString[3],NumberStyles.Any,ci));
        #endif
		*/
		return newVector;

	}
	
	public void PlayScreamSound()
	{
		GetComponent<AudioSource>().PlayOneShot(screamSound);
	}
	

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	
		
}
