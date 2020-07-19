using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

/**
 * Created by Rio 3D Studios.
 *
 * class to manage the choice of helicopters in the lobby room
 *
 */
public class VehicleChooseManager : MonoBehaviour {


	public static  VehicleChooseManager  instance;  //useful for any gameObject to access this class without the need of instances her or you declare her
	
	public GameObject[] vehicles; //helicopters available for choice
	
	public int maxVehicles = 5; //maximum number of helicopters
	
	[SerializeField] private Canvas nextButton, prevButton; //variables referencing the choice buttons

	public int currentVehicle = 0; //marks the chosen helicopter

	
	// Use this for initialization
	void Start () {
	
		// if don't exist an instance of this class
		if (instance == null) {

			// define the class as a static variable
			instance = this;
			
			currentVehicle = 0; //sets the first choice option
			
			SetVehicle(0); //set up the  vehicles[0] as currentVehicle
			
			CheckButtonStatus(); //configure the display of choice buttons

		}



	}

	/**
	 * method to display or hide the choice buttons
	 *
     */
	private void CheckButtonStatus()
	{
	
	    //verification to prevent errors
		if (nextButton == null || prevButton == null)
			return;
		
		
		if (currentVehicle == 0) 
		{
			prevButton.enabled = false;//hides the "previous" button
			
			nextButton.enabled = true;//displays the "next" button
			
		} 
		else if (currentVehicle >= maxVehicles-1) 
		{
			prevButton.enabled = true;//displays the "next" button
			
			nextButton.enabled = false;//hides the "next" button
			
		} 
		else // otherwise displays both buttons
		{
			prevButton.enabled = true;
			nextButton.enabled = true;
		}
		
	}

	/**
	 * method to defines the helicopter to be displayed according to the player's choice
	 *
     */
	void SetVehicle(int index)
	{
	 
	  for(int i =0;i< vehicles.Length;i++)
	  {
	    if(i.Equals(index))
		{
		   vehicles[index].SetActive(true);  //displays the helicopter chosen by the player
		}
		else
		{
		   vehicles[i].SetActive(false);  //hides the other helicopters
		}
	  }
	}

	/**
	 * method called by the BtnNext button that selects the next helicopter
	 *
     */
	public void NextAvatar()
	{
	  if(currentVehicle+1< maxVehicles)
	  {	   
		currentVehicle++;
		
		SetVehicle(currentVehicle);
		
		if(currentVehicle>=maxVehicles)
		{
			currentVehicle = maxVehicles - 1;
		}
		CheckButtonStatus();
	
	  }
	}
	
	/**
	 * method called by the BtnPrev button that selects the previous helicopter
	 *
     */
	public void PrevAvatar()
	{
	  if(currentVehicle-1 >= 0)
	   {
				    
		  currentVehicle--;
		  
		  SetVehicle(currentVehicle);
		
		  if(currentVehicle<0)
		  {
			 currentVehicle =0;
		  }
		  CheckButtonStatus();
		  
		}
	}
	
}
