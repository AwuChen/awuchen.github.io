using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * Created by Rio 3D Studios.
 *
 * class to rotate helicopter helices
 *
 */
public class Spin : MonoBehaviour {

	public float rotateSpeed = 900f;
	
	// Use this for initialization
	void Start () {}
	
	// Update is called once per frame
	void Update ()
	{
	  transform.RotateAround (transform.position,Vector3.up,rotateSpeed*Time.deltaTime);		
	}
}
