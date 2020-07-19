using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Created by Rio 3D Studios.
 *
 * class to manage all the game's UI's and HUDs
 *
 */
public class Shooter3DCanvasManager : MonoBehaviour
{
    
	public static Shooter3DCanvasManager  instance; // //useful for any gameObject to access this class without the need of instances her or you declare her

	public Canvas  HUDLobby; //variable that stores the HUDLobby panel
	
	public Canvas  gameCanvas; //variable that stores the Game Canvas panel
	
	public Canvas alertgameDialog; //variable that stores the AlertGameDialog panel
	
	public Canvas leaderBoard;
	
	public Canvas mobileButtons;
	
	public int currentMenu; //store current HUD screen
	
	public GameObject lobbyCamera;  // variable that stores the Lobby Camera
	
	public GameObject mainCamera;   // variable that stores the Main Camera
	 
	public InputField inputLogin;   // variable that stores the Input Filed
	
	public GameObject contentBestUser;
	
	public GameObject  bestUserPrefab;

	public ArrayList bestUsers;
	
	
	
 /*********************** TEXT VARIABLES ***************************/
	 
	public Text txtLocalPlayerHealth;
	
	public Text alertDialogText;
	
	public Text messageText;
	
	public Text txtLog;
	
	public Text txtPing;
	
	public Text txtTotalPlayers;
	
	public Text txtKills;
	
	public  AudioClip buttonSound;
	
	public float delay = 0f;
	
	public bool enabledMobileBtns;
	
	
	
	/***********************PANEL GUN ***************************/
	
	public Image currentGunImage;
	
	public Text txtCurrentAmmo;

	public Slider currentBulletSlider;
	
	/***********************************************************/
	
	
	/***********************DAMAGE SKIN***************************/
	public Image damageImage;
	public float flashSpeed = 5f;
	public Color flashColour = new Color(1f, 0f, 0f, 0.1f);
	public bool damaged;
	/***********************************************************/
	
	
	
	// Use this for initialization
	void Start () {

		if (instance == null) {

			DontDestroyOnLoad (this.gameObject);

			instance = this;
			
			OpenScreen(0);

			CloseAlertDialog ();
			
			bestUsers = new ArrayList ();


		}
		else
		{
			Destroy(this.gameObject);
		}



	}

	void Update()
	{
		delay += Time.deltaTime;

		if (Input.GetKey ("escape") && delay > 1f) {

		  switch (currentMenu) {

			case 0:
			 Application.Quit ();
			break;

			case 1:
			Application.Quit ();
			 delay = 0f;
			break;
			
			case 2:
			Application.Quit ();
			 delay = 0f;
			break;
			
			case 3:
			 Application.Quit ();
			break;

		 }//END_SWITCH

	 }//END_IF
	 
	 	/***********************DAMAGE SKIN***************************/
		if(damaged)
		{
			damageImage.color = flashColour;
		}
		else
		{
			damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
		}
		damaged = false;
      /*************************************************************************/
}
	/// <summary>
	/// Opens the screen.
	/// </summary>
	/// <param name="_current">Current.</param>
	public void  OpenScreen(int _current)
	{
		switch (_current)
		{
		    //lobby menu
		    case 0:
			currentMenu = _current;
			HUDLobby.enabled = true;
			gameCanvas.enabled = false;
			lobbyCamera.SetActive(true);
			mainCamera.SetActive(false);
			break;


		    case 1:
			currentMenu = _current;
			HUDLobby.enabled = false;
			gameCanvas.enabled = true;
			mainCamera.SetActive(true);
			lobbyCamera.SetActive(false);
		
			break;

	
		}

	}


	/// <summary>
	/// Shows the alert dialog.
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowAlertDialog(string _message)
	{
		alertDialogText.text = _message;
		alertgameDialog.enabled = true;
	}

	
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	public void CloseAlertDialog()
	{
		alertgameDialog.enabled = false;
	}
	
	/// <summary>
	/// Shows the alert dialog.Debug.Log
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowMessage(string _message)
	{
		messageText.text = _message;
		messageText.enabled = true;
		StartCoroutine (CloseMessage() );
	}
	
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	IEnumerator CloseMessage() 
	{

		yield return new WaitForSeconds(4);
		messageText.text = "";
		messageText.enabled = false;
	} 
	
	
	/// <summary>
	/// configure the weapon's HUD
	/// </summary>
	public void SetUpPanelGun(int maxBulets,  int currentAmmo)
	{
	   txtCurrentAmmo.text = currentAmmo.ToString();
	   
	   currentBulletSlider.maxValue = currentAmmo;

	   currentBulletSlider.value = currentAmmo;
	}
	
	/// <summary>
	/// update the weapon's HUD
	/// </summary>
	public void UpdatePanelGun( int currentAmmo)
	{
	   txtCurrentAmmo.text = currentAmmo.ToString();
	   
	   currentBulletSlider.value = currentAmmo;
	   
	}


	public void PlayAudio(AudioClip _audioclip)
	{
	   GetComponent<AudioSource> ().PlayOneShot (_audioclip);
	}
	
	
	public void SetUpBestKiller(string _name, string _kills, string _ranking)
	{
	    	  
	  	GameObject newUser = Instantiate (bestUserPrefab) as GameObject;

		
		newUser.GetComponent<User>().name.text = _name;
		newUser.GetComponent<User>().kills.text = _kills;
		newUser.GetComponent<User>().ranking.text = _ranking;
		newUser.transform.parent = contentBestUser.transform;
		newUser.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
		bestUsers.Add (newUser);
	}
	
		/// <summary>
	/// Clears the leader board.
	/// </summary>
	public void ClearLeaderBoard()
	{
		foreach (GameObject user in bestUsers)
		{

			Destroy (user.gameObject);
		}

		bestUsers.Clear ();
	}
	
	public void OpenLeaderBoard()
	{
	  Shooter3DNetworkManager.instance.EmitGetBestKillers();
	  leaderBoard.enabled = true;
	}
	
	public void CloseLeaderBoard()
	{
	  ClearLeaderBoard();
	  leaderBoard.enabled = false;
	  
	}
	
	public void SetMobileButtons()
	{
	  enabledMobileBtns=!enabledMobileBtns;
	  if(enabledMobileBtns)
	  {
	    mobileButtons.enabled = true;
	  }
	  else
	  {
	    mobileButtons.enabled = false;
	  }
	}
	
}
