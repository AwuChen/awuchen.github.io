using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///Manage Network player if isLocalPlayer variable is false
/// or Local player if isLocalPlayer variable is true.
/// </summary>
public class PlayerManager : MonoBehaviour {

	public string	id;

	public string name;

	public string avatar;

	public bool isOnline;

	public bool isLocalPlayer;

	Animator myAnim;

	Rigidbody myRigidbody;

	public enum state : int {idle,walk,attack,damage,dead};//cada estado representa um estado do inimigo

	public state currentState;

	public float verticalSpeed = 3f;

	public float rotateSpeed = 60f;

	//distances low to arrive close to the player
	[Range(1f, 200f)][SerializeField] float minDistanceToPlayer = 10f ;

	public bool onGrounded;

	[SerializeField] float m_GroundCheckDistance = 1f;

	public float jumpPower = 12f;

	public float jumpTime=0.4f;

	public float jumpdelay=0.4f;

	public bool m_jump;

	public bool isJumping;

	public float lastVelocityX =0f;

	public Transform cameraTotarget;

	public bool isAtack;

	// Use this for initialization
	void Awake () {

		myAnim = GetComponent<Animator>();
		myRigidbody = GetComponent<Rigidbody> ();
		lastVelocityX = myRigidbody.velocity.x;

	}

	public void Set3DName(string name)
	{
		GetComponentInChildren<TextMesh> ().text = name;

	}

	// Update is called once per frame
	void FixedUpdate () {
		//Turning ();

		if (isLocalPlayer) {

			Atack ();
			Move ();
		}
		else
		{
			if (myRigidbody.velocity.x != lastVelocityX) {
				lastVelocityX = myRigidbody.velocity.x;
				//UpdateAnimator("idle");
			}
			else
			{
				UpdateIdle ();
			}
		}



		Jump ();



	}

	void Atack()
	{
		if (isLocalPlayer)
		{
			//user press A keyboard button or not
			isAtack = Input.GetKey (KeyCode.A);


			if (isAtack)
			{
				currentState = state.attack;
				UpdateAnimator ("IsAtack");
				string msg = id;
				NetworkManager.instance.EmitAttack(msg);//call method NetworkSocketIO.EmitPosition for transmit new  player position to all clients in game

				foreach(KeyValuePair<string, PlayerManager> enemy in NetworkManager.instance.networkPlayers)
				{

					if ( enemy.Key != id)
					{
						//calcula o vetor distancia de mim até o player
						Vector3 meToEnemy = transform.position - enemy.Value.transform.position;
						Debug.Log ("meToEnemy.sqrMagnitude: "+meToEnemy.sqrMagnitude);
						//if i am close to player
						if (meToEnemy.sqrMagnitude < minDistanceToPlayer)
						{


							NetworkManager.instance.EmitPhisicstDamage (id, enemy.Key);
						}
					}
				}
			}

		}
	}

	void Move( )
	{

		// read inputs
		//float  h = CrossPlatformInputManager.GetAxis ("Horizontal");
		//float  v = CrossPlatformInputManager.GetAxis ("Vertical");

		bool move = false;
		if (Input.GetKey("up"))//up button or joystick
		{
		  move = true;
		  transform.Translate (new Vector3 (0, 0, 1 * verticalSpeed * Time.deltaTime));
		//  UpdateAnimator("run");
		}
		if (Input.GetKey("down"))//down button or joystick
		{
			move = true;
			transform.Translate (new Vector3 (0, 0, -1 * verticalSpeed * Time.deltaTime));
			//UpdateAnimator("run");
		}


		if (Input.GetKey ("right")) {//right button or joystick
			move = true;
			this.transform.Rotate (Vector3.up, rotateSpeed * Time.deltaTime);
		}
		if (Input.GetKey ("left")) {//left button or joystick
			move = true;
			this.transform.Rotate (Vector3.up, -rotateSpeed * Time.deltaTime);
		}


		if (move || isJumping) {
			currentState = state.walk;
			UpdateAnimator ("IsWalk");
			UpdateStatusToServer ();
		}
		else
		{
			currentState = state.idle;
			UpdateAnimator ("IsIdle");
		}
	}

	void UpdateStatusToServer ()
	{


		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = id;

		data["position"] = transform.position.x+","+transform.position.y+","+transform.position.z;

		data["rotation"] = transform.rotation.x+","+transform.rotation.y+","+transform.rotation.z+","+transform.rotation.w;



		NetworkManager.instance.EmitMoveAndRotate(data);//call method NetworkSocketIO.EmitPosition for transmit new  player position to all clients in game



	}


	public void UpdateIdle()
	{

		currentState = state.idle;
		UpdateAnimator ("IsIdle");

	}

	public void UpdatePosition(Vector3 position)
	{
		if (!isLocalPlayer) {

			if (!isJumping)
			{
				currentState = state.walk;
				UpdateAnimator ("IsWalk");
				//Debug.Log ("player move to:" + position);
				transform.position = new Vector3 (position.x, position.y, position.z);
			}
		}

	}

	public void UpdateRotation(Quaternion _rotation)
	{
		if (!isLocalPlayer)
		{
			transform.rotation = _rotation;

		}

	}



	public void UpdateAnimator(string _animation)
	{


		switch (_animation) {


		case "IsWalk":
			if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Walk"))
			{
				myAnim.SetTrigger ("IsWalk");

			}
			break;

		case "IsIdle":

			if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				myAnim.SetTrigger ("IsIdle");

			}
			break;

		case "IsDamage":
			if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Damage") )
			{
				myAnim.SetTrigger ("IsDamage");
			}
			break;

		case "IsAtack":
			if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Atack"))
			{
				myAnim.SetTrigger ("IsAtack");
			}
			break;


		case "IsDead":
			if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
			{
				myAnim.SetTrigger ("IsDead");
			}
			break;

		}//END_SWITCH


	}


	public void UpdateJump()
	{
		m_jump = true;
	}

	public void Jump()
	{
		RaycastHit hitInfo;

		onGrounded = Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance);

		jumpTime -= Time.deltaTime;

		if (isLocalPlayer)
		{
			m_jump = Input.GetKey("space");
		}

		// se ja deu o tempo de pulo e o player esta colidindo com o chão e ele  estava pulando
		if (jumpTime <= 0 && isJumping && onGrounded)
		{

			m_jump = false;
			isJumping = false;//marca que o player não esta pulando
		}


		//verifica se o usuario apertou espaco e ele ja não esta pulando ou se o player esta na lona e ja não esta pulando
		if (m_jump && !isJumping)
		{

			//efeito do pulo
			myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
			//calcula o tempo de pulo
			jumpTime = jumpdelay;
			//marca que o player esta pulando
			isJumping = true;
			//UpdateAnimator("jump");

		}

	}
}
