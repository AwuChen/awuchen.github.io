using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Created by Rio 3D Studios.
 *
 * Class to manage the player
 * Manage Network player if isLocalPlayer variable is false
 * or Local player if isLocalPlayer variable is true.
 */

public class VehicleManager : MonoBehaviour
{
     
    public string	id;

	public string name;

	public string avatar;

	public bool isOnline;

	public bool isLocalPlayer;
	
	 Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
	
	public float speed = 6f;            // The speed that the player will move at.
			 
    Vector3 movement;                   // The vector to store the direction of the player's movement.
    
	public float h ;
	
	public float v;
	
	public bool onMobileButton;
 
	#if !MOBILE_INPUT
	int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    float camRayLength = 10000f;          // The length of the ray from the camera into the scene.
    #endif
	
	/****** SYNCHRONIZATION VARIABLES (only for Network Players) ***/
	
	public float lastSynchronizationTime;
	
	public float syncDelay;
	
	public float syncTime;
	
	Vector3 syncStartPosition = Vector3.zero;
	
	Vector3 syncEndPosition = Vector3.zero;

	public bool onReceivedPos;
	
	Vector3 pos;
	
	/*************************************************************/

    void Awake ()
    {
       #if !MOBILE_INPUT
        // Create a layer mask for the floor layer.
        floorMask = LayerMask.GetMask ("Floor");
       #endif

       playerRigidbody = GetComponent <Rigidbody> ();
			
    }

    void FixedUpdate ()
    {
          
	   if(isLocalPlayer)
	   {
	   
	      if(!onMobileButton)
		  {
		   // Store the input axes.
           h = Input.GetAxisRaw("Horizontal");
              
		   v = Input.GetAxisRaw("Vertical");
		  }
		  
	 
		  // Move the player around the scene.
          Move (h, v);
			 
          // rotates the player according to the mouse			 
		  Turning ();
		
          //sends rotation and position of the player to the server		
		  UpdateStatusToServer ();
		       
            
		}
		else //if Network Player
		{
		  SyncedMovement();
		}
         
      }
		
		
    void Move (float h, float v)
    {
            // Set the movement vector based on the axis input.
            movement.Set (h, 0f, v);
            
            // Normalise the movement vector and make it proportional to the speed per second.
            movement = movement.normalized * speed * Time.deltaTime;

            // Move the player to it's current position plus the movement.
            playerRigidbody.MovePosition (transform.position + movement);
    }
	
     	
	void UpdateStatusToServer ()
	{
			
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = id;

		data["dx"] = transform.position.x.ToString();
		
		data["dz"] = transform.position.z.ToString();
		
		data["rotY"] = transform.rotation.y.ToString();

	    //call method Shooter3DNetworkManager for transmit new  player position and rotation to all clients in game
		Shooter3DNetworkManager.instance.EmitPosAndRot(data);

	}


    void Turning ()
    {
		
      #if !MOBILE_INPUT
      // Create a ray from the mouse cursor on screen in the direction of the camera.
      Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

      // Create a RaycastHit variable to store information about what was hit by the ray.
      RaycastHit floorHit;

      // Perform the raycast and if it hits something on the floor layer...
      if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask))
      {	
        // Create a vector from the player to the point on the floor the raycast from the mouse hit.
        Vector3 playerToMouse = floorHit.point - transform.position;
        
		// Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotatation = Quaternion.LookRotation (playerToMouse);
				
        // Set the player's rotation to this new rotation.
        playerRigidbody.MoveRotation (newRotatation);
      }
	  
      #else
      Vector3 turnDir = new Vector3(CrossPlatformInputManager.GetAxisRaw("Mouse X") , 0f , CrossPlatformInputManager.GetAxisRaw("Mouse Y"));

       if (turnDir != Vector3.zero)
       {
          // Create a vector from the player to the point on the floor the raycast from the mouse hit.
          Vector3 playerToMouse = (transform.position + turnDir) - transform.position;

          // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
          Quaternion newRotatation = Quaternion.LookRotation(playerToMouse);
		
          // Set the player's rotation to this new rotation.
          playerRigidbody.MoveRotation(newRotatation);
       }
      #endif
    
	}//END_TURNING
		
	
     public void Set3DName(string name)
     {
		GetComponentInChildren<TextMesh> ().text = name;
     }
	 
	 public void EnableKey(string _key)
	 {
	 
	   onMobileButton = true;
	   switch(_key)
	   {
	   
	     case "up":
		 v = 1;
		 break;
		 case "down":
		 v= -1;
		 break;
		 case "right":
		 h = 1;
		 break;
		 case "left":
		 h = -1;
		 break;
	   }
	 }
	 
	 public void DisableKey(string _key)
	 {
	   onMobileButton = false;
	   switch(_key)
	   {
	    case "up":
		 v = 0;
		 break;
		 case "down":
		 v= 0;
		 break;
		 case "right":
		 h = 0;
		 break;
		 case "left":
		 h = 0;
		 break;
	   }
	 }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////NETWORK PLAYERS METHODS ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  /**
	 * method to synchronize the position received from the server
	 * only for Network Players
   */
	void SyncedMovement()
	{
	  syncTime +=Time.deltaTime;
	  
	  if(onReceivedPos)
	  {
	    transform.position = new Vector3(Vector3.Lerp(syncStartPosition,syncEndPosition,syncTime/syncDelay).x,transform.position.y,
		Vector3.Lerp(syncStartPosition,syncEndPosition,syncTime/syncDelay).z);
		
	  }
	}
	
	
  /**
	 * method that receives and handles the update of the position of the Network Player that arrived from the server
   */
	public void UpdatePosition(float _dx, float _dz) 
	{
	  
	  syncEndPosition = new Vector3(_dx,transform.position.y,_dz);
	  
	  syncStartPosition = transform.position;
	  
	  syncTime = 0f;
	  
	  syncDelay = Time.time - lastSynchronizationTime;
	  
	  lastSynchronizationTime = Time.time;
	  
	  onReceivedPos = true;
	
	  // transform.position = new Vector3 (position.x, position.y, position.z);
	}
	
   /**
	 * method that receives and handles the update of the rotation of the Network Player that arrived from the server
   */
	public void UpdateRotation(string _rotation) 
	{
		if (!isLocalPlayer) 
		{
		
		   transform.rotation = new Quaternion ( transform.rotation.x, float.Parse(_rotation),  transform.rotation.z,  transform.rotation.w);
		   
		}

	}


}
