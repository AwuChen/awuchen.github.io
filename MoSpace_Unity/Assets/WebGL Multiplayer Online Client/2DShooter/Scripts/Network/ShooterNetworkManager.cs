using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ShooterNetworkManager : MonoBehaviour
{
   
	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static ShooterNetworkManager instance;
	
	//Variable that defines comma character as separator
	static private readonly char[] Delimiter = new char[] {':'};

	//flag which is determined the player is logged in the arena
	public bool onLogged = false;

	//local player id
	public string myId = string.Empty;
	
	//local player id
	public string local_player_id;


	public enum PlayerType { NONE,GIRL,BOY}; 

	public PlayerType playerType;
	
	public bool myTurn;
	
	public string myType;
	
	public bool startedGame;
	
	
	//store localPlayer
	public GameObject myPlayer;
	
	//store all players in game
	public Dictionary<string, Player2DManager> networkPlayers = new Dictionary<string, Player2DManager>();
	
	ArrayList playersNames;
	
	//store the local players' models
	public GameObject[] localPlayersPrefabs;
	

	//store the networkplayers' models
	public GameObject[] networkPlayerPrefabs;
	
	public GameObject txtPlayerNamePref;
	
	//stores the spawn points 
	public Transform[] spawnPoints;
	
	public Camera2DFollow cameraFollow;
	
	public bool isGameOver;
	
	
	int index;

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
	
		
		playersNames = new ArrayList();
		
	
		
	 }
	 else
	 {
		//it destroys the class if already other class exists
		Destroy(this.gameObject);
	 }
		
	}
	
	
	

	void Update(){}



	/// <summary>
	///  receives an answer of the server.
	/// from  void OnReceivePing(string [] pack,IPEndPoint anyIP ) in server
	/// </summary>
	public void OnPrintPongMsg()
	{

		/*
		 * data.pack[0]= CALLBACK_NAME: "PONG"
		 * data.pack[1]= "pong!!!!"
		*/

		Debug.Log("receive pong");
		
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
		 data["name"] = ShooterCanvasManager.instance.inputLogin.text;
			  
	     data["avatar"] = ButtonChooseManager.instance.currentAvatar.ToString();
		 
		 //makes the draw of a point for the player to be spawn
		 index = UnityEngine.Random.Range (0, spawnPoints.Length);

		 data["dx"] = spawnPoints[index].position.x.ToString();
			  
			
		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "JOIN_ROOM",new JSONObject(data));

		
		}
		else
		{
			
		   isGameOver = false;
		   
			 //player's name	
		    data["name"] = ShooterCanvasManager.instance.inputLogin.text;
			  
	        data["avatar"] = ButtonChooseManager.instance.currentAvatar.ToString();
		 
		    //makes the draw of a point for the player to be spawn
		    index = UnityEngine.Random.Range (0, spawnPoints.Length);

		    data["dx"] = spawnPoints[index].position.x.ToString();
			
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
		
		ShooterCanvasManager.instance.ShowMessage("use mouse to fire weapon");
		
		var pack = data.Split (Delimiter);
		
		if (!myPlayer) {
		
			// take a look in Player2DManager.cs script
			Player2DManager newPlayer;
			
			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (localPlayersPrefabs [int.Parse(pack[2])],spawnPoints[index].position,
			Quaternion.identity).GetComponent<Player2DManager> ();
			 
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
			
			ShooterCanvasManager.instance.localPlayerImg.sprite =  ShooterCanvasManager.instance.
			spriteFacesPref[int.Parse(pack[2])].GetComponent<SpriteRenderer> ().sprite;
	
		    ShooterCanvasManager.instance.txtLocalPlayerName.text = pack[1];
				
		    ShooterCanvasManager.instance.txtLocalPlayerHealth.text = "100";
			  
			ShooterCanvasManager.instance.OpenScreen(1);
			  
			GameObject txtName = GameObject.Instantiate (txtPlayerNamePref,new Vector3(0f,0f,-0.1f), Quaternion.identity) as GameObject;
			txtName.name = pack[1];
			txtName.GetComponent<PlayerName> ().setName (pack[1]);
			txtName.GetComponent<Follow> ().SetTarget (newPlayer.gameObject.transform);
			
			playersNames.Add(txtName);
			
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
		*/
		
		var pack = data.Split (Delimiter);
		try{
		
		Debug.Log ("\n spawning network player ...\n");
		
		//ShooterCanvasManager.instance.txtLog.text = "\n spawning network player ...\n";
	
		bool alreadyExist = false;

			
			if(networkPlayers.ContainsKey(pack [0]))
			{
			  alreadyExist = true;
			}
			if (!alreadyExist) {
			
			// ShooterCanvasManager.instance.txtLog.text = "creating a new player";

				Debug.Log("creating a new player");

				Player2DManager newPlayer;
		
				newPlayer =  GameObject.Instantiate (networkPlayerPrefabs [int.Parse(pack[2])],new Vector3(float.Parse(pack [3]),-3f,0),
			  Quaternion.identity).GetComponent<Player2DManager> ();
			  
			  
			//  ShooterCanvasManager.instance.txtLog.text = "player statiated";
			  
			 

				//it is not the local player
				newPlayer.isLocalPlayer = false;

				//network player online in the arena
				newPlayer.isOnline = true;

				newPlayer.gameObject.name = pack [0];
				
				newPlayer.name = pack[1];
				
				newPlayer.avatar = pack[2];

				//puts the local player on the list
				networkPlayers [pack [0]] = newPlayer;
					
			
				GameObject txtName = GameObject.Instantiate (txtPlayerNamePref,new Vector3(0f,0f,-0.1f), Quaternion.identity) as GameObject;
				txtName.name = pack[1];
				txtName.GetComponent<PlayerName> ().setName (pack[1]);
				txtName.GetComponent<Follow> ().SetTarget (newPlayer.gameObject.transform);
				
				playersNames.Add(txtName);
				
				// ShooterCanvasManager.instance.txtLog.text = "player in game";
				
			}
			
			}
			catch(Exception e)
			{
			   Debug.Log(e.ToString());
			 //  ShooterCanvasManager.instance.txtLog.text = e.ToString();
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
		 * pack[2] = rotation
		*/
		var pack = data.Split (Delimiter);
	
		
		if (networkPlayers [pack [0]] != null)
		{
		  
			Player2DManager netPlayer = networkPlayers[pack [0]];

			//update with the new position
			netPlayer.UpdatePosition(float.Parse(pack [1]));
	        //update new player rotation
			netPlayer.UpdateRotation(pack[2]);
			

		}
	}
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////PLAYER JUMP UPDATES///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//send local player jump to server
	public void EmitJump()
	{
	 
	   //hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = myPlayer.GetComponent<Player2DManager>().id;

	   //sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "JUMP",new JSONObject(data));
	}
	
	/// <summary>
	///  Update the network player jump to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateJump(string data)
	{
		/*
		 * pack[0] = id
		
		*/
		
		
		var pack = data.Split (Delimiter);
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[pack[0]];

		//updates current animation
		netPlayer.UpdateJump();

	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////ANIMATION UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Emits the local player animation to Server.js.
	/// </summary>
	/// <param name="_animation">Animation.</param>
	public void EmitAnimation(string _animation)
	{
		
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = myPlayer.GetComponent<Player2DManager>().id;

		data ["animation"] = _animation;
	
	    //sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "ANIMATION",new JSONObject(data));

	}

	/// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateAnim(string data)
	{
		/*
		 * data.pack[0] = id
		 * data.pack[1] = animation
		*/
		
		var pack = data.Split (Delimiter);
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[pack[0]];

		//updates current animation
		netPlayer.UpdateAnimator(pack[1]);

	}
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////DAMAGE UPDATES/////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  public void EmitPlayerDamage(string _networkPlayerID)
  {
  
    Debug.Log("emit damage");
    
	//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data ["id"] = _networkPlayerID;
		 
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
		var pack = data.Split (Delimiter);
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[pack[0]];

		if(networkPlayers[pack[0]].isLocalPlayer)
		{
		    ShooterCanvasManager.instance.txtLocalPlayerHealth.text = pack[1];
		}
		else
		{
		    
		    ShooterCanvasManager.instance.networkPlayerImg.sprite =  
			ShooterCanvasManager.instance.spriteFacesPref[int.Parse(netPlayer.avatar)].GetComponent<SpriteRenderer> ().sprite;
	
		    ShooterCanvasManager.instance.txtNetworkPlayerName.text = netPlayer.name;
				
		    ShooterCanvasManager.instance.txtNetworkPlayerHealth.text = pack[1];
		}
		//updates current animation
		netPlayer.UpdateAnimator("OnDamage");

	}
	
	
	  /// <summary>
	///  Update the network player animation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnGameOver(string data)
	{
		 /*
		 * data.data.pack[0] = id
	
		*/
		
		try{
		
		var pack = data.Split (Delimiter);
		
		
		//find network player by your id
		Player2DManager netPlayer = networkPlayers[pack[0]];
		
		 GameObject name = null; 
		 
		 foreach(GameObject pn in playersNames)
		 {
		     if(pn!=null && pn.name.Equals(netPlayer.name))
		     {
			 
			  Destroy(pn);
		     }
		  }
		  
		if(networkPlayers[pack[0]].isLocalPlayer)
		{
		    ResetGame(); 
		}
		else
		{
		    
		  isGameOver = true;
		
		  Destroy (networkPlayers [pack[0]].gameObject);
		  
		  networkPlayers.Remove (pack[0]);
				
		
		}
		}
		catch(Exception e)
		{
		 Debug.Log(e.ToString());
		}
		
	
	}
	
	void ResetGame()
	{
	    Debug.Log("reset game");
		
		myPlayer = null;
		
		
		foreach(GameObject name in playersNames)
		{
		  Destroy(name);
		}
		
		playersNames.Clear();
		
		//send answer in broadcast
		foreach (KeyValuePair<string, Player2DManager> entry in networkPlayers) {

		  Destroy(entry.Value.gameObject);
		  
		}//END_FOREACH
		
		networkPlayers.Clear();
		
		ShooterCanvasManager.instance.txtLocalPlayerHealth.text = "100";
	
	    ShooterCanvasManager.instance.txtNetworkPlayerHealth.text = "100";
		
		ShooterCanvasManager.instance.OpenScreen(0);
		
		
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
	void OnUserDisconnected(string data)
	{

		/*
		 * data.pack[0] = id (network player id)
		*/

		var pack = data.Split (Delimiter);
		
		Debug.Log("user: "+networkPlayers[pack[0]].name+" disconnected");
  
		GameObject name = null; 
	 
		 foreach(GameObject pn in playersNames)
		 {
		     if(pn.name.Equals(networkPlayers[pack[0]].name))
		     {
			 
			  Destroy(pn);
		     }
		  }
		  
		
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
	public string GetPlayerType()
	{
		switch (playerType) {

		case PlayerType.NONE:
			return "none";
		break;
		case PlayerType.GIRL:
			return "girl";
		break;
		case PlayerType.BOY:
			return "boy";
		break;
		
		}
		return string.Empty;
	}


	/// <summary>
	/// Sets the type of the user.
	/// </summary>
	/// <param name="_userType">User type.</param>
	public void SetPlayerType(string _playerType)
	{
		switch (_playerType) {

		case "none":
			playerType = PlayerType.NONE;	
		break;
		case "boy":
			playerType = PlayerType.BOY;	
		break;
		case "girl":
			playerType = PlayerType.GIRL;	
		break;
		}
	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	
		
}
