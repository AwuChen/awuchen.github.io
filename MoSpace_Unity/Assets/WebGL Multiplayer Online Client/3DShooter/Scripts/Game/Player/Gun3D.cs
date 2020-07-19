using UnityEngine;
using System.Collections;

/**
 * Created by Rio 3D Studios.
 *
 * Class to manage the player's weapon
 *
 */
public class Gun3D : MonoBehaviour {

   /*********************** DEFAULT VARIABLES ***************************/
	public string gunName;
	
	public int ammo = 300; //current bullets
	
	public int maxAmmo = 30; // max bullets
	
	public bool  m_Shoot;  // fire shot trigger
	
	public float timeBetweenBullets = 0.15f;  //delay among the shots
	
	public Transform[] gunsPosition; // guns positions
	
	public GameObject bulletPref; // bullet prefab
	
	float timer; // time counter
	
	/*********************** GUN EFFECTS VARIABLES ***************************/
	public ParticleSystem gunParticles1;
	
	public ParticleSystem gunParticles2;
	
	public Light gunLight1;
	
	public Light gunLight2;
	
	float effectsDisplayTime = 0.2f;
	
	public GameObject explosionPref;   //explosion prefab
	
	int shootableMask;  //target layer mask, for this example, we set up the "Floor" layer in start method
	 
	public float camRayLength = 100f;  //the ray of reach of the mouse click
	
	/*********************** AUDIO VARIABLES ***************************/
	public  AudioClip shootSound;
	
	public AudioClip reloadSound;
	
	public AudioClip outOfAmmoSound;
	
 /*********************** END GUN EFFECTS VARIABLES ***************************/

	
	// Use this for initialization
	void Start () {
	
		shootableMask = LayerMask.GetMask ("Floor");
		
		Shooter3DCanvasManager.instance.SetUpPanelGun(ammo,  ammo);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	  timer += Time.deltaTime;   //increments the timer
	
	  Shoot(); //calls shoot method
	
	  if(timer >= timeBetweenBullets * effectsDisplayTime)
      {
          DisableEffects (); // calls the DisableEffects method to disable effects :D
      }
	 
	}

	/**
	 * method to manage the shots fired by the weapon
	 *
     */
	public void Shoot()
	{

	    if(GetComponentInParent<VehicleManager>().isLocalPlayer 
		&& Input.GetButtonDown ("Fire1") &&!GetComponentInParent<VehicleManager>().onMobileButton)
		{
		  m_Shoot = true;
		}
		  
		if(GetComponentInParent<VehicleManager>().isLocalPlayer 
		   && Input.GetButtonUp("Fire1"))
	    {
	        m_Shoot = false;
	    }
		
	   if (m_Shoot && timer >= timeBetweenBullets
			&& Time.timeScale != 0) {
			
		  timer = 0f;  //reset the timer

		  if (GetAmmo () > 0) {
			
			 CameraShake.Shake (0.25f, 0.7f);
			
			 Shooter3DNetworkManager.instance.EmitShoot(); //update the server
			 
			 SetCurrentAmmo (GetAmmo () - 1); //updates the number of bullets
			 
			 SpawnBullet(); //spawn bullets
			 
			 EnableEffects(); // enable gun effects (lights, particles and sounds)
			 
			 Shooter3DCanvasManager.instance.UpdatePanelGun(GetComponentInChildren<Gun3D>().ammo); //updates the gun's HUD
			 
			 EnableParticlesEffects();
					
		  }
		  else
		  {
		    //PlayOutOfAmmoSound();  // no bullets :(
			 SetCurrentAmmo ( maxAmmo); //updates the number of bullets
		  }

	   }
	  
	}
	
	/*
	* method called if the weapon belongs to a network player
	*/
	public void NetworkShoot()
	{
	  timer = 0f;
	  
	  SpawnNetworkBullet();
	  
	  EnableEffects();
		
	}
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////HELPERS ;) ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	
	public void SpawnBullet()
	{
	    GameObject bullet = GameObject.Instantiate (bulletPref, gunsPosition[0].position, Quaternion.identity) as GameObject;
			 
	    bullet.GetComponent<Bullet3DController> ().isLocalBullet = true;
			 
	    bullet.GetComponent<Bullet3DController> ().shooterID = GetComponentInParent<VehicleManager>().id; 
			
	    bullet.GetComponent<Bullet3DController> ().Shoot (transform.forward,GetComponentInParent<VehicleManager> ().gameObject.name);
			 
	    GameObject bullet2 = GameObject.Instantiate (bulletPref, gunsPosition[1].position, Quaternion.identity) as GameObject;
			 
	    bullet.GetComponent<Bullet3DController> ().isLocalBullet = true;
			 
		bullet.GetComponent<Bullet3DController> ().shooterID = GetComponentInParent<VehicleManager>().id; 
			
		bullet2.GetComponent<Bullet3DController> ().Shoot (transform.forward,GetComponentInParent<VehicleManager> ().gameObject.name);
	}
	
	
	public void SpawnNetworkBullet()
	{
	  GameObject bullet = GameObject.Instantiate (bulletPref, gunsPosition[0].position, Quaternion.identity) as GameObject;
			 
	  bullet.GetComponent<Bullet3DController> ().shooterID = GetComponentInParent<VehicleManager>().id; 
			
	  bullet.GetComponent<Bullet3DController> ().Shoot (transform.forward,GetComponentInParent<VehicleManager> ().gameObject.name);
			 
	  GameObject bullet2 = GameObject.Instantiate (bulletPref, gunsPosition[1].position, Quaternion.identity) as GameObject;
			 
	  bullet.GetComponent<Bullet3DController> ().shooterID = GetComponentInParent<VehicleManager>().id; 
			
	  bullet2.GetComponent<Bullet3DController> ().Shoot (transform.forward,GetComponentInParent<VehicleManager> ().gameObject.name);
	}
   
   public void  EnableParticlesEffects()
   {
       // Create a ray from the mouse cursor on screen in the direction of the camera.
       Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

       // Create a RaycastHit variable to store information about what was hit by the ray.
       RaycastHit obstacleHit;

       // Perform the raycast and if it hits something on the floor layer...
       if(Physics.Raycast (camRay, out obstacleHit, camRayLength, shootableMask))
       {
	     Instantiate(explosionPref, obstacleHit.point,Quaternion.identity);
	   }
   }
	
	public void EnableEffects()
	{
	   gunLight1.enabled = true;
			 
	   gunLight2.enabled = true;

       gunParticles1.Stop ();
             
	   gunParticles1.Play ();
			 
	   gunParticles2.Stop ();
             
	   gunParticles2.Play ();
				
	   PlayShootSound ();
	}
	
	public void DisableEffects ()
    {
     
        gunLight1.enabled = false;
			 
		gunLight2.enabled = false;
    }
	
	public int GetAmmo(){ return ammo;}
	
	public void SetCurrentAmmo(int _ammo){ ammo = _ammo;}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// END HELPERS  ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// AUDIO METHODS ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public void PlayOutOfAmmoSound()
	{
		GetComponent<AudioSource>().PlayOneShot(outOfAmmoSound);
	}

	public void PlayReloadSound()
	{
		GetComponent<AudioSource>().PlayOneShot(reloadSound);
	}

	public void PlayShootSound()
	{
		GetComponent<AudioSource>().PlayOneShot(shootSound);
	}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// END AUDIO METHODS  ///////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
