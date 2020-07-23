using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.UI;
using System.Runtime.InteropServices;

/// <summary>
/// Network Manager class.
/// </summary>
///

public class NetworkManager : MonoBehaviour {

	/*#if UNITY_WEBGL
	[DllImport("__Internal")]
	private static extern void OpenConnection();

	[DllImport("__Internal")]
	private static extern void Emit(string callback_id, string data);
	#endif
   */
	private bool _isListenPortLogged = false;

	//Variable that defines comma character as separator
	static private readonly char[] Delimiter = new char[] {','};

	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static NetworkManager instance;

	//flag which is determined the player is logged in the arena
	public bool onLogged = false;

	//store localPlayer
	public GameObject myPlayer;


	public string local_player_id;

	//store all players in game
	public Dictionary<string, PlayerManager> networkPlayers = new Dictionary<string, PlayerManager>();

	//store the local players' models
	public GameObject[] localPlayersPrefabs;

	//store the networkplayers' models
	public GameObject[] networkPlayerPrefabs;

	//stores the spawn points
	public Transform[] spawnPoints;

	//Standard Assets\Cameras\Prefabs\MultipurposeCameraRig.prefab
	public GameObject camRigPref;

	public GameObject camRig;

	public bool isGameOver;


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


			Debug.Log("start mmo game");



		}
		else
		{
			//it destroys the class if already other class exists
			Destroy(this.gameObject);
		}

	}



	/// <summary>
	/// Prints the pong message which arrived from server.
	/// </summary>
	/// <param name="_msg">Message.</param>
	public void OnPrintPongMsg(string data)
	{

		/*
		 * data.pack[0]= msg
		*/

		var pack = data.Split (Delimiter);
		Debug.Log ("received message: "+pack[0] +" from server by callbackID: PONG");
		CanvasManager.instance.ShowAlertDialog ("received message: "+pack[0] +" from server by callbackID: PONG");
	}

	// <summary>
	/// sends ping message to server.
	/// </summary>
	public void EmitPing() {

		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//store "ping!!!" message in msg field
	    data["msg"] = "ping!!!!";

		JSONObject jo = new JSONObject (data);

		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "PING", new JSONObject(data));


	}





	//call be  OnClickJoinBtn() method from CanvasManager class
	/// <summary>
	/// Emits the player's name to server.
	/// </summary>
	/// <param name="_login">Login.</param>
	public void EmitJoin()
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();


		//player's name
		data["name"] = CanvasManager.instance.inputLogin.text;

		//makes the draw of a point for the player to be spawn
		int index = Random.Range (0, spawnPoints.Length);

		//send the position point to server
		string msg = string.Empty;

		if (!isGameOver) {


			data["name"] = CanvasManager.instance.inputLogin.text;

			data["position"] = spawnPoints[index].position.x+","+spawnPoints[index].position.y+","+spawnPoints[index].position.z;

			//sends to the nodejs server through socket the json package
			Application.ExternalCall("socket.emit", "LOGIN",new JSONObject(data));


		}
		else
		{
			data["callback_name"] = "RESPAW";//preenche com o id da callback receptora que está no servidor


			Debug.Log ("emit respawn");
			data["id"] = local_player_id;

			JSONObject jo = new JSONObject (data);

			//sends to the nodejs server through socket the json package
			Application.ExternalCall("socket.emit", "RESPAW",new JSONObject(data));

		}



		//obs: take a look in server script.
	}

	/// <summary>
	/// Joins the local player in game.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void OnJoinGame(string data)
	{
		Debug.Log("Login successful, joining game");
		var pack = data.Split (Delimiter);
		// the local player now is logged
		onLogged = true;

		/*
		 * data.data.pack[0] = id (local player id)
		 * data.data.pack[1]= name (local player name)
		 * data.data.pack[2] = position.x (local player position x)
		 * data.data.pack[3] = position.y (local player position ...)
		 * data.data.pack[4] = position.z
		 * data. data.pack[5] = rotation.x
		 * data.data.pack[6] = rotation.y
		 * data.data.pack[7] = rotation.z
		 * data.data.pack[8] = rotation.w
		*/


		if (!myPlayer) {

			// take a look in NetworkPlayer.cs script
			PlayerManager newPlayer;

			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (localPlayersPrefabs [0],
				new Vector3(float.Parse(pack[2]), float.Parse(pack[3]),
					float.Parse(pack[4])),Quaternion.identity).GetComponent<PlayerManager> ();


			Debug.Log("player instantiated");
			newPlayer.id = pack [0];
			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;

			//set local player's 3D text with his name
			newPlayer.Set3DName(pack[1]);

			//puts the local player on the list
			networkPlayers [pack [0]] = newPlayer;

			myPlayer = networkPlayers [pack[0]].gameObject;

			local_player_id =  pack [0];



			//spawn camRigPref from Standard Assets\Cameras\Prefabs\MultipurposeCameraRig.prefab
			//camRig = GameObject.Instantiate (camRigPref, new Vector3 (0f, 0f, 0f), Quaternion.identity);

			//set local player how  being MultipurposeCameraRig target to follow him
			//camRig.GetComponent<CameraFollow> ().SetTarget (myPlayer.transform, newPlayer.cameraTotarget);

			//CanvasManager.instance.healthSlider.value = newPlayer.gameObject.GetComponent<PlayerHealth>().health;

			//CanvasManager.instance.txtHealth.text = "HP " + newPlayer.gameObject.GetComponent<PlayerHealth>().health + " / " +
			//	newPlayer.gameObject.GetComponent<PlayerHealth>().maxHealth;
			//hide the lobby menu (the input field and join buton)
			CanvasManager.instance.OpenScreen(1);
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
		 * data.pack[0] = id (network player id)
		 * data.pack[1]= name
		 * data.pack[2] = position.x
		 * data.pack[3] = position.y
		 * data.pack[4] = position.z
		 * data.pack[5] = rotation.x
		 * data.pack[6] = rotation.y
		 * data.pack[7] = rotation.z
		 * data.pack[8] = rotation.w
		*/

		var pack = data.Split (Delimiter);

		if (onLogged ) {

			bool alreadyExist = false;

			//verify all players to  prevents copies
			foreach(KeyValuePair<string, PlayerManager> entry in networkPlayers)
			{
				// same id found ,already exist!!!
				if (entry.Value.id == pack [0])
				{
					alreadyExist = true;
				}
			}
			if (!alreadyExist) {


				PlayerManager newPlayer;

				// newPlayer = GameObject.Instantiate( network player avatar or model, spawn position, spawn rotation)
				newPlayer = GameObject.Instantiate (networkPlayerPrefabs [0],
					new Vector3(float.Parse(pack[2]), float.Parse(pack[3]),
						float.Parse(pack[4])),Quaternion.identity).GetComponent<PlayerManager> ();




				newPlayer.id = pack [0];


				//it is not the local player
				newPlayer.isLocalPlayer = false;

				//network player online in the arena
				newPlayer.isOnline = true;

				//set the network player 3D text with his name
				newPlayer.Set3DName(pack[1]);
				newPlayer.gameObject.name = pack [0];
				//puts the local player on the list
				networkPlayers [pack [0]] = newPlayer;

			}

		}

	}

	//method to respawn  player called from client.js
	void OnRespawPlayer(string data)
	{
		/*
		 * data.pack[0] = id
		 * data.pack[1]= name
		 * data.pack[2] = position.x
		 * data.pack[3] = position.y
		 * data.pack[4] = position.z
		 * data.pack[5] = rotation.x
		 * data.pack[6] = rotation.y
		 * data.pack[7] = rotation.z
		 * data.pack[8] = rotation.w
		*/

		var pack = data.Split (Delimiter);

		Debug.Log("Respaw successful, joining game");
		CanvasManager.instance.OpenScreen(1);
		onLogged = true;
		isGameOver = false;

		if (myPlayer == null) {

			// take a look in PlayerManager.cs script
			PlayerManager newPlayer;

			// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			newPlayer = GameObject.Instantiate (localPlayersPrefabs [0],
				new Vector3(float.Parse(pack[2]), float.Parse(pack[3]),
					float.Parse(pack[4])),Quaternion.identity).GetComponent<PlayerManager> ();


			Debug.Log("player instantiated");
			newPlayer.id = pack [0];
			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;

			//set local player's 3D text with his name
			newPlayer.Set3DName(pack[1]);

			//puts the local player on the list
			networkPlayers [pack [0]] = newPlayer;

			myPlayer = networkPlayers [pack[0]].gameObject;

			local_player_id = pack [0];



			//spawn camRigPref from Standard Assets\Cameras\Prefabs\MultipurposeCameraRig.prefab
			//camRig = GameObject.Instantiate (camRigPref, new Vector3 (0f, 0f, 0f), Quaternion.identity);

			//set local player how  being MultipurposeCameraRig target to follow him
			//camRig.GetComponent<CameraFollow> ().SetTarget (myPlayer.transform, newPlayer.cameraTotarget);

			CanvasManager.instance.healthSlider.value = newPlayer.gameObject.GetComponent<PlayerHealth>().health;

			CanvasManager.instance.txtHealth.text = "HP " + newPlayer.gameObject.GetComponent<PlayerHealth>().health + " / " +
				newPlayer.gameObject.GetComponent<PlayerHealth>().maxHealth;
			//hide the lobby menu (the input field and join buton)
			CanvasManager.instance.OpenScreen(1);
			Debug.Log("player in game");

		}

	}

	public void EmitMoveAndRotate( Dictionary<string, string> data)
	{

		JSONObject jo = new JSONObject (data);

		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "MOVE_AND_ROTATE",new JSONObject(data));
    }



	/// <summary>
	/// Update the network player position and rotation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateMoveAndRotate(string data)
	{
		/*
		 * data.pack[0] = id (network player id)
		 * data.pack[1] = position.x
		 * data.pack[2] = position.y
		 * data.pack[3] = position.z
		 * data.pack[4] = rotation.x
		 * data.pack[5] = rotation.y
		 * data.pack[6] = rotation.z
		 * data.pack[7] = rotation.w
		*/

		var pack = data.Split (Delimiter);

		if (networkPlayers [pack [0]] != null)
		{
			PlayerManager netPlayer = networkPlayers[pack[0]];

			//update with the new position
			netPlayer.UpdatePosition(new Vector3(
				float.Parse(pack[1]), float.Parse(pack[2]), float.Parse(pack[3])));

			Vector4 rot = new Vector4(
				float.Parse(pack[4]), float.Parse(pack[5]), float.Parse(pack[6]),
				float.Parse(pack[7]));// atualiza a posicao

			//update new player rotation
			netPlayer.UpdateRotation(new Quaternion (rot.x,rot.y,rot.z,rot.w));

			//IsAtack?
			if (bool.Parse (pack [8]))
			{
				netPlayer.UpdateAnimator ("IsAtack");
			}

            Debug.Log("Update Player Pos & Rot");
        }


	}


	/// <summary>
	/// Emits the local player animation to Server.js.
	/// </summary>
	/// <param name="_animation">Animation.</param>
	public void EmitAnimation(string _animation)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = myPlayer.GetComponent<PlayerManager>().id;

		data ["animation"] = _animation;

		JSONObject jo = new JSONObject (data);

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
		 * data.pack[0] = id (network player id)
		 * data.pack[1] = animation (network player animation)
		*/

		var pack = data.Split (Delimiter);

		//find network player by your id
		PlayerManager netPlayer = networkPlayers[pack[0]];

		//updates current animation
		netPlayer.UpdateAnimator(pack[1]);

	}
	public void EmitAttack(string _id)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data ["id"] = _id;

		JSONObject jo = new JSONObject (data);

		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "ATACK",new JSONObject(data));


	}

	/// <summary>
	/// Update the network player rotation to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdateAtack(string data)
	{
		/*
		 * data.pack[0] = id (network player id)

		*/

		var pack = data.Split (Delimiter);

		if (networkPlayers [pack [0]] != null)
		{
			PlayerManager netPlayer = networkPlayers[pack[0]];

			netPlayer.UpdateAnimator ("IsAtack");

		}


	}




	//sends to server player damage
	public void EmitPhisicstDamage(string _shooterId, string _targetId)
	{

		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data ["shooterId"] = _shooterId;

		data ["targetId"] = _targetId;

		JSONObject jo = new JSONObject (data);

		//sends to the nodejs server through socket the json package
		Application.ExternalCall("socket.emit", "PHISICS_DAMAGE",new JSONObject(data));

	}


	//updates player damage
	void OnUpdatePlayerPhisicsDamage (string data)
	{

		/*
		 * data.pack[0] = attacker.id or shooter.id (network player id)
		 * data.pack[1] = target.id (network player id)
		 * data.pack[2] = target.health
		 */


		var pack = data.Split (Delimiter);


		if (networkPlayers [pack [1]] != null)
		{

			PlayerManager PlayerTarget = networkPlayers[pack [1]];
			PlayerTarget. GetComponent<PlayerHealth> ().TakeDamage ();


			if (PlayerTarget.isLocalPlayer)// if i'm a target
			{
				CanvasManager.instance.healthSlider.value = float.Parse(pack [2]);
				CanvasManager.instance.txtHealth.text = "HP " + pack [2] + " / "
					+ PlayerTarget. GetComponent<PlayerHealth> ().maxHealth;

			}



		}

		if (networkPlayers [pack [0]] != null)
		{

			PlayerManager PlayerShooter = networkPlayers[pack [0]];



			if (!PlayerShooter.isLocalPlayer)//if i'm a target
			{
				//PlayerShooter.UpdateAnimator("IsAtack");

			}



		}


	}


	void OnPlayerDeath (string data)
	{

	 /*
     *  data.pack[0] = target.id (network player id)
     */


		var pack = data.Split (Delimiter);

		if (networkPlayers [pack [0]] != null)
		{
			PlayerManager PlayerTarget = networkPlayers[pack [0]];
			if (PlayerTarget.isLocalPlayer) {// se eu sou o alvo


				StartCoroutine (deathCutScene (PlayerTarget));

			}
			else
			{

				StartCoroutine (NetworkPlayerDeathCutScene(PlayerTarget));

			}

		}

	}


	IEnumerator deathCutScene(PlayerManager PlayerTarget )
	{

		PlayerTarget.UpdateAnimator ("IsDead");

		CanvasManager.instance.healthSlider.value = 0f;
		CanvasManager.instance.txtHealth.text = "HP  0" + " / "
			+ PlayerTarget. GetComponent<PlayerHealth> ().maxHealth;

		yield return new WaitForSeconds(3f); // wait for set reload time
		//myPlayer.GetComponent<FirstPersonController>().enabled = false;


		Destroy( networkPlayers[ PlayerTarget.id].gameObject);
		networkPlayers.Remove(PlayerTarget.id);
		myPlayer = null;
		GameOver ();// volta para a tela inicial do game
	}


	IEnumerator NetworkPlayerDeathCutScene(PlayerManager PlayerTarget )
	{
		PlayerTarget.UpdateAnimator ("IsDead");

		yield return new WaitForSeconds(3f); // wait for set reload time
		Destroy( networkPlayers[ PlayerTarget.id].gameObject);
		networkPlayers.Remove(PlayerTarget.id);

	}



	void GameOver()
	{
		if (onLogged) {
			isGameOver = true;
			CanvasManager.instance.ShowGameOverDialog ();
		}
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

		if (networkPlayers [pack [0]] != null)
		{


			//destroy network player by your id
			Destroy( networkPlayers[pack[0]].gameObject);


			//remove from the dictionary
			networkPlayers.Remove(pack[0]);

		}

	}


	private void OnDestroy() {

	}


	void OnApplicationQuit() {


		//Debug.Log("Application ending after " + Time.time + " seconds");
	}



}
