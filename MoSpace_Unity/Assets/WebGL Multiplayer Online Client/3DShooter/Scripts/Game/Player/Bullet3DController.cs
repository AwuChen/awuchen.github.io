using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet3DController : MonoBehaviour {

	  public ParticleSystem particleSystem;

	    public AudioClip explosionAudio;    

	    Rigidbody myRigidybody;

	    public bool canMove;

		Vector3 target;

	    Vector3 target_position;
		
		public float bulletSpeed;

	    // name of player that launched missle
        public string ownersName = "";   

	public bool isLocalBullet;
	public string shooterID;
	public int ammotype;
	public Transform direction;
	
	
	public GameObject explosionPref;

	public GameObject enemyExplosionPref;		


        private void Start ()
        {
		    myRigidybody = GetComponent<Rigidbody> ();
      
        }

		// Update is called once per frame
	    void FixedUpdate () {

		    if (canMove) {
			   myRigidybody.AddForce (bulletSpeed * target);
		    }
	

	    }
		

	    public void Shoot(Vector3 _target , string playerName)
	    {
		   canMove = true;
		   target = _target;
		   target_position = new Vector3(target.x,target.y,target.z);
		   ownersName = playerName;

	     }

		
	void OnTriggerEnter(Collider colisor)
     {
		
		if (colisor.gameObject.name != shooterID 
		&& colisor.gameObject.tag.Equals("NetworkPlayer") && isLocalBullet)
		{
		  //Debug.Log("colider: "+colisor.gameObject.tag);
		  //damage on network player
		  //ShooterNetworkManager.instance.EmitPlayerDamage (colisor.gameObject.name);
		  Instantiate (explosionPref, transform.position, transform.rotation);
		  Shooter3DNetworkManager.instance.EmitPlayerDamage (shooterID,colisor.gameObject.name);
		  Destroy (gameObject);
		  
		  

		}
		if(!isLocalBullet)
		{
		  if (colisor.gameObject.tag.Equals("Player") )
		{
		  
		  Instantiate (explosionPref, transform.position, transform.rotation);
		  Destroy (gameObject);
		}
		}
		
		
		if (colisor.gameObject.name == "wall") {



			
		}
			
	 }


	public void PlayExplisionSound()
	{
	//	GetComponent<AudioSource>().PlayOneShot(explosionAudio);
	}
   

}
