/*
 *	Main file
 */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class OtherController : MonoBehaviour {
	// RIG VARIABLES
	string[] rigLeftNames = new string[] {"ORig|Bone_L.003", "ORig|Bone_L.005"};
	string[] rigRightNames = new string[] {"ORig|Bone_R.003", "ORig|Bone_R.005"};
//	string worldOriginName = "WorldOrigin";
//	string[] smokeNames = new string[] {"OParticleLeftWing","OParticleCenter","OParticleRightWing"};
	
	// GAMEOBJECTS
	// Other Player
	public GameObject rigLeft1;
	public GameObject rigLeft2;
	public GameObject rigRight1;
	public GameObject rigRight2;

	// Development Mode With Mouse Only
	public bool developmentMode; 
	
	// Gravity
	Vector3 gravityVector = new Vector3(0,0,0);
	Vector3 rotateVector = new Vector3(0,0,0);
	bool gravityEnabled = true;
	
	// Collision
	Vector3 collisionPosition = new Vector3(0,0,0);
	Vector3 collisionNormal = new Vector3(0,0,0);
	public float upSpeed;
	
	// Powerups
	float powerUp = 1;
	
	// Store wing positions, current and previous to calculate bird movement
	float left_wing_current;
	float right_wing_current;
	float left_wing_old;
	float right_wing_old;
	
	// Reset Wings variables
	Vector3 left3;
	Vector3 left5;
	Vector3 right3;
	Vector3 right5;
	
	// Kinect skeleton variables
	float lefthand;
	float shouldercenter;
	float righthand;
	float lefthandzero;
	float righthandzero;
	public bool visibleSkeleton;
	public bool skeletonEnteredScreen;
	int skeletonBufferTimer;
	
	// Speed variables
	public float forwardSpeed;
	public float pitch_down_speed;
	public float pitch_up_speed;
	public float roll_factor;
	public float roll_speed;
	public float wing_movement_factor;
	
	// Start Screen
	public int welcomeDuration;
	public bool isStartingUp;
	public float startupHeight;
	
	// Particle trails
	GameObject smokeLeft;
	GameObject smokeRight;
//	GameObject smokeCenter;

	
	// Use this for initialization
	void Start () {
		// ASSIGN GAME OBJECTS
		// Set rig GameObjects to correct parts
		rigLeft1 = GameObject.Find(rigLeftNames[0]);
		rigLeft2 = GameObject.Find(rigLeftNames[1]);
		rigRight1 = GameObject.Find(rigRightNames[0]);
		rigRight2 = GameObject.Find(rigRightNames[1]);
		
//		smokeLeft = GameObject.Find (smokeNames[0]);
//		smokeCenter = GameObject.Find (smokeNames[1]);
//		smokeRight = GameObject.Find (smokeNames[2]);

		GameObject playerObject = GameObject.Find("Player");
		PlayerController player = playerObject.GetComponent<PlayerController> ();
		developmentMode = player.developmentMode;

//		smokeLeft.SetActive (false);
//		smokeRight.SetActive (false);

		visibleSkeleton = false;
		skeletonEnteredScreen = true;
	}
	
	
	// Update is called once per frame
	void Update () {
		// If disconnected from network or no other players are connected, move other player in default trajectory around world
		if(Network.peerType == NetworkPeerType.Disconnected || (Network.isServer && Network.connections.Length < 1)){
			MoveParallel();
		}
	}
	
	void FixedUpdate(){
		rigidbody.velocity = new Vector3(0, 0, 0);
		
		if(!gravityEnabled){
			// Calculate amount of up speed based on distance to collision position
			float factor = (20-Vector3.Distance(collisionPosition,transform.position))/20;
			factor = Mathf.Pow(Mathf.Clamp(factor,0,1),3);
			rigidbody.AddForce((collisionNormal) * upSpeed * factor);
			// Slow player down when hitting an object
			rigidbody.AddRelativeForce(Vector3.forward * forwardSpeed * (1-factor));
			
		}else{
			rigidbody.AddRelativeForce(Vector3.forward * forwardSpeed * powerUp);
		}
	}

	
	void MoveParallel(){
		// Calculate Vectors
		GameObject world = GameObject.Find("WorldOrigin");
		gravityVector = (world.transform.position - transform.position);
		Vector3 forwardVector = transform.forward*10;
		rotateVector = Vector3.Cross(gravityVector,forwardVector);
		forwardVector = Vector3.Cross(rotateVector,gravityVector).normalized *20f;
		transform.rotation = Quaternion.LookRotation(forwardVector, -gravityVector);
		if(gravityVector.magnitude < startupHeight){
			transform.Translate(Vector3.up*0.2f);
			transform.Rotate(Vector3.left*(startupHeight-gravityVector.magnitude));
		}
		Debug.DrawLine(transform.position, transform.position+forwardVector, Color.red);
	}
}