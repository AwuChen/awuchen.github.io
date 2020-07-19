using UnityEngine;
using System.Collections;

public class IsometricCameraFollow: MonoBehaviour
{
    public Transform target;            // The position that that camera will be following.
    
	public float smoothing = 5f;        // The speed with which the camera will be following.


    public Vector3 offset;             // The initial offset from the target.


     
		// Use this for initialization
        private void Start(){}


		
	  /**
	    * set up the target
      */
		public void SetTarget(Transform _target)
		{
		  target = _target;
		}


        void FixedUpdate ()
        {
		    if(target!=null)
			{
             // Create a postion the camera is aiming for based on the offset from the target.
             Vector3 targetCamPos = target.position + offset;

             // Smoothly interpolate between the camera's current position and it's target position.
             transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
			 
			}
        }
    
}
