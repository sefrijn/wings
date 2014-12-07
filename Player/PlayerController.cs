/*
 *	Main file
 */
// TODO Mapping with depth feed. Dimensions vary

// Mapping van X positie maakt niet uit voor skeleton tracking, want gaat om relatief verschil

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour {
	// RIG VARIABLES
	string[] rigLeftNames = new string[] {"Rig|Bone_L.003", "Rig|Bone_L.005"};
	string[] rigRightNames = new string[] {"Rig|Bone_R.003", "Rig|Bone_R.005"};
	string worldOriginName = "WorldOrigin";
	string mainCameraName = "Main Camera";
	string[] smokeNames = new string[] {"ParticleLeftWing","ParticleCenter","ParticleRightWing"};
	
	// GAMEOBJECTS
	// Player
	public GameObject rigLeft1;
	public GameObject rigLeft2;
	public GameObject rigRight1;
	public GameObject rigRight2;
	// World and camera
	GameObject worldOrigin;
	GameObject mainCamera;
	// Particle trails
	GameObject smokeLeft;
	GameObject smokeRight;
	GameObject smokeCenter;

	// Mapping variables
	public float mapBottom;
	public float mapTop;
	public float mapInverse;

	// Development Mode With Mouse Only
	public bool developmentMode;

	// Basic steering
	public bool basicVersion;
//	float maxAngle;

	// Server config
	public string serverConfigPath = "";
	public int local;

	// Gravity
	Vector3 gravityVector = new Vector3(0,0,0);
	Vector3 rotateVector = new Vector3(0,0,0);
	bool gravityEnabled = true;

	// Collision
	Vector3 collisionPosition = new Vector3(0,0,0);
	Vector3 collisionNormal = new Vector3(0,0,0);
	public float upSpeed;

	// Collision Camera Shake
	public float cameraShakeLength;
	public float cameraShakeTimer;
	Vector3 camOriginalPosition;
	Quaternion camOriginalRotation;

	// Audio
	AudioSource[] sounds;

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
	float shouldercenterX;
	float shouldercenterXzero;
	float lefthandzero;
	float righthandzero;
	public bool visibleSkeleton;
	public bool skeletonEnteredScreen;
	public int skeletonBufferDuration;
	int skeletonBufferTimer;

	// URL Parameters
	public string url;
	public string img;
	public string depth;
	public float depthWidth;
	public float depthHeight;
	public float skeletonWidth;
	public float skeletonHeight;
	bool skeletonReady;

	// Speed variables
	public float forwardSpeed;
	public float pitch_down_speed;
	public float pitch_up_speed;
	public float roll_factor;
	public float roll_speed;
	public float wing_movement_factor;

	// Start Screen
	public int welcomeTimer;
	public int welcomeDuration;
	public bool isStartingUp;
	public float startupHeight;

	// Depth map control
//	Texture2D depthTexture;
//	bool textureDoneLoading = true;
	float depthValue = -1f;
//	float depthValueZero = -1f;
	public float depthNear;
	public float depthFar;

	// Live feed variables ( Size of /color )
	public int colorWidth;
	public int colorHeight;


	void Awake(){
		// Load XML configuration
		Config config = GetComponent<Config> ();
		config.Init ();

		// ASSIGN GAME OBJECTS
		// Set rig GameObjects to correct parts
		rigLeft1 = GameObject.Find(rigLeftNames[0]);
		rigLeft2 = GameObject.Find(rigLeftNames[1]);
		rigRight1 = GameObject.Find(rigRightNames[0]);
		rigRight2 = GameObject.Find(rigRightNames[1]);
		
		smokeLeft = GameObject.Find (smokeNames[0]);
		smokeCenter = GameObject.Find (smokeNames[1]);
		smokeRight = GameObject.Find (smokeNames[2]);
		
		mainCamera = GameObject.Find (mainCameraName);
		worldOrigin = GameObject.Find(worldOriginName);

		InitWings();
	}

	// Use this for initialization
	void Start () {
		// Store initial wing position to recalibrate when entering startup screen
		roll_factor = 0;

		// Set amount of maximum steering
//		maxAngle = 90f; // 90 degrees

		// Set timers to zero
		skeletonBufferTimer = 0;
		welcomeTimer = 0;

		isStartingUp = false;

		smokeLeft.SetActive (false);
		smokeRight.SetActive (false);

		if (developmentMode == false) {
			GetSkeleton ();
			GetDepthMap();
			//skeletonReady = false;
		}
		visibleSkeleton = false;
		skeletonEnteredScreen = true;

		// Init collision camera timer
		cameraShakeTimer = 0;
		camOriginalPosition = mainCamera.transform.localPosition;
		camOriginalRotation = mainCamera.transform.localRotation;

		sounds = GetComponents<AudioSource>();

		// Set variables according to config.xml if present and development mode is false
		if(!developmentMode){
			Debug.Log("Development mode = OFF");
		}else{
			Debug.Log("Development mode = ON");
		}
	}
	
	
	// Update is called once per frame
	void Update () {
		// Check if during startup
		if(!isStartingUp){
			if(basicVersion && developmentMode == false){
				BasicPitch();
				BasicRoll();
				SetBasicWings();
			}else{
				// Set Wing position variables
				if(developmentMode == false && visibleSkeleton == true){
					// Only use Get Skeleton if developmentMode is off and there is a skeleton present
					SetWingsWithSkeleton();
				}else{
					// Set Wing position variables with mouse data
					SetWingsWithMouse();
				}		
				// Up and down
				CharacterPitch ();
				// Left and right
				CharacterRoll ();
			}

			// Shake Camera on Collision
			ShakeCamera ();
		}else{
			// Starting Game Procedure
			// Forward movement parallel with planet
			MoveParallel();

			// Reset wings
			if(skeletonEnteredScreen){
				ResetWings();
				skeletonEnteredScreen = false;
			}
		}

		// While Powered up, slowly remove speed
		if (powerUp > 1) {
			powerUp -= 0.01f;
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

	void OnGUI(){
		if(developmentMode){
			if(GUI.Button(new Rect(0,0,200,25),"Startscreen")){
				skeletonEnteredScreen = true;
				welcomeTimer = welcomeDuration;
			}
			if(GUI.Button(new Rect(0,25,200,25),"Leave DevelopmentMode")){
				developmentMode = false;
			}
			GUI.Label (new Rect(0,100,250,25),"Linkerhand: "+lefthand);
			GUI.Label (new Rect(0,125,250,25),"Rechterhand: "+righthand);
			GUI.Label (new Rect(0,175,250,25),"Basic Version: "+basicVersion);
			GUI.Label (new Rect(0,200,250,25),"Diepte value: "+(depthValue));
			GUI.Label (new Rect(0,150,250,25),"SchouderY: "+shouldercenter); // komt binnen op breedte van hoogte feed
			GUI.Label (new Rect(0,225,250,25),"SchouderX: "+shouldercenterX); // komt binnen op hoogte van depth feed
		}else if(Input.GetButton("Jump")){
			GUI.Label (new Rect(0,200,250,25),"SchouderY: "+shouldercenter); // komt binnen op breedte van hoogte feed
			GUI.Label (new Rect(0,175,250,25),"SchouderX: "+shouldercenterX); // komt binnen op hoogte van depth feed
			GUI.Label (new Rect(0,150,250,25),"Schouder Zero: "+shouldercenterXzero);
			GUI.Label (new Rect(0,225,250,25),"Diepte value: "+(depthValue));
		}

	}
	
	void ShakeCamera(){
		if(cameraShakeTimer > (cameraShakeLength*0.33f)){
			float factor = cameraShakeTimer/cameraShakeLength;
			mainCamera.transform.Rotate (Vector3.forward*Mathf.Cos((cameraShakeLength-cameraShakeTimer)*2*Mathf.PI/10f)*12*factor);
			cameraShakeTimer--;
		}else if(cameraShakeTimer > 1){
			mainCamera.transform.localRotation = Quaternion.RotateTowards(mainCamera.transform.localRotation, camOriginalRotation, 0.2f);
		}else if(cameraShakeTimer <= 1){
			mainCamera.transform.localPosition = camOriginalPosition;
			mainCamera.transform.localRotation = camOriginalRotation;
			cameraShakeTimer--;
		}
	}
	
	// Disable gravity force when collision has happened
	void OnCollisionEnter(Collision col){
		gravityEnabled = false;
		audio.Play();
		collisionNormal = col.contacts [0].normal;
		// Get current position to calculate distance and factor upward speed away from object
		collisionPosition = transform.position;
		cameraShakeTimer = cameraShakeLength;
	}
	void OnCollisionExit(Collision col){
		Invoke ("ResumeGravity", 1);
	}
	void ResumeGravity(){
		gravityEnabled = true;
	}

	// Power up hit
	void OnTriggerEnter(){
		sounds[3].Play();
		powerUp = 3;
		smokeLeft.SetActive (true);
		smokeRight.SetActive (true);
		smokeCenter.GetComponent<ParticleSystem>().startSize = 10f;
		Invoke ("NoPowerUp", 15);
	}

	void NoPowerUp(){
		powerUp = 1;
		smokeLeft.SetActive (false);
		smokeRight.SetActive (false);
		smokeCenter.GetComponent<ParticleSystem>().startSize = 40f;
	}


	void SetWingsWithSkeleton(){
		if (skeletonEnteredScreen) {
			Debug.Log("set wings met skeleton entered true");
			// Initial position from calibration values
			left_wing_current = left_wing_old = wing_movement_factor*mapInverse*((Mathf.Clamp(lefthandzero-shouldercenter,mapBottom,mapTop)-mapBottom)/((mapTop-mapBottom)/2)-1);
			right_wing_current = right_wing_old = wing_movement_factor*mapInverse*((Mathf.Clamp(righthandzero-shouldercenter,mapBottom,mapTop)-mapBottom)/((mapTop-mapBottom)/2)-1);
			skeletonEnteredScreen = false;

		} else {
			left_wing_old = left_wing_current;
			right_wing_old = right_wing_current;
			left_wing_current = wing_movement_factor*mapInverse*((Mathf.Clamp(lefthand-shouldercenter,mapBottom,mapTop)-mapBottom)/((mapTop-mapBottom)/2)-1);
			right_wing_current = wing_movement_factor*mapInverse*((Mathf.Clamp(righthand-shouldercenter,mapBottom,mapTop)-mapBottom)/((mapTop-mapBottom)/2)-1);
		}
	}


	void SetWingsWithMouse(){
		// Set ROLL with mouse X (map mouse X from -4 to 4)
		roll_factor = ((Input.mousePosition.x-700)/-700)*0.8f;
		// Set PITCH with mouse Y (map mouse Y from 0 to 1
		if (skeletonEnteredScreen) {
			Debug.Log("set wings met mouse true");

			left_wing_old = right_wing_old = left_wing_current = right_wing_current = wing_movement_factor * 2 * Input.mousePosition.y / 1200 + 1;
			skeletonEnteredScreen = false;
		} else {
			// Set old variables before assigning new ones
			left_wing_old = left_wing_current;
			right_wing_old = right_wing_current;
			left_wing_current = right_wing_current = wing_movement_factor * 2 * Input.mousePosition.y / 1200 + 1;
		}
	}

	void CharacterPitch(){
		// Rotate character UP only if wings move down
		if ( (left_wing_current - left_wing_old) < -0.0001f) {
			transform.Rotate(Vector3.right * ((left_wing_current - left_wing_old) * pitch_up_speed));
		}
		// Apply gravity and rotate character DOWN 
		// Calculate Vectors
//		GameObject world = GameObject.Find("WorldOrigin");
		gravityVector = (worldOrigin.transform.position - transform.position);
		float distanceToOrigin = 1-(Mathf.Clamp(Vector3.Distance(worldOrigin.transform.position, transform.position),100,400)-100)/300;
		Vector3 forwardVector = transform.forward*10;
		rotateVector = Vector3.Cross(gravityVector,forwardVector).normalized;

		// Rotate down only when Gravity is enabled (= not during collision)
		if (gravityEnabled){
			// Angle between gravity force and forward movement of character
			float angle = Vector3.Angle(gravityVector, forwardVector);
			// If angle is smaller than 30 degrees, reduce rotation with linear factor
			if(angle > 30F){
				transform.Rotate(-rotateVector*pitch_down_speed*distanceToOrigin, Space.World);
			}else{
				transform.Rotate(-rotateVector*pitch_down_speed*distanceToOrigin*(angle/30F), Space.World);
			}
		}
		Debug.DrawLine(transform.position-rotateVector*20, transform.position+rotateVector*20, Color.red);
		Debug.DrawLine(transform.position, transform.position+gravityVector, Color.red);
		Debug.DrawLine(transform.position, transform.position+forwardVector, Color.red);
	}

	void CharacterRoll(){
		// Only if a Skeleton is present use Skeleton to Roll character
		if(developmentMode == false && visibleSkeleton == true){
			roll_factor = -(lefthand-righthand)/(mapTop-mapBottom)*mapInverse;
		}
		transform.Rotate(Vector3.forward * roll_factor * roll_speed);

//		Debug.Log ((left_wing_current-left_wing_old)*100);
		// Rotate wings according to diff variable
		rigLeft1.transform.Rotate (Vector3.right * (left_wing_current-left_wing_old)*100);
		rigRight1.transform.Rotate (Vector3.right * (right_wing_current-right_wing_old)*100);
		rigLeft2.transform.Rotate (Vector3.right * (left_wing_current-left_wing_old)*100);
		rigRight2.transform.Rotate (Vector3.right * (right_wing_current-right_wing_old)*100);
	}


	void MoveParallel(){
		// Calculate Vectors
		gravityVector = (worldOrigin.transform.position - transform.position);
		Vector3 forwardVector = transform.forward*10;
		rotateVector = Vector3.Cross(gravityVector,forwardVector);
		forwardVector = Vector3.Cross(rotateVector,gravityVector).normalized *(pitch_up_speed/4);
		transform.rotation = Quaternion.LookRotation(forwardVector, -gravityVector);
		if(gravityVector.magnitude < startupHeight){
			transform.Translate(Vector3.up*0.2f);
			transform.Rotate(Vector3.left*(startupHeight-gravityVector.magnitude));
		}
		if (gravityVector.magnitude > startupHeight+20f){
			transform.Translate(Vector3.down * 2f);
		}
		Debug.DrawLine(transform.position, transform.position+forwardVector, Color.red);
	}

	void BasicPitch(){
		// Move parallel
		gravityVector = (worldOrigin.transform.position - transform.position);
//		float distanceToOrigin = 1-(Mathf.Clamp(Vector3.Distance(worldOrigin.transform.position, transform.position),100,400)-100)/300;
		Vector3 forwardVector = transform.forward*10;
		rotateVector = Vector3.Cross(gravityVector,forwardVector).normalized;
		forwardVector = Vector3.Cross(rotateVector,gravityVector).normalized *(pitch_up_speed/4);
		if (gravityEnabled){
			transform.rotation = Quaternion.LookRotation(forwardVector, Quaternion.Euler(forwardVector* roll_factor * 10f) * -gravityVector);
		}
		Debug.DrawLine(transform.position, transform.position + Quaternion.Euler(forwardVector* 1f)* -gravityVector, Color.blue);
		Debug.DrawLine(transform.position, transform.position - gravityVector, Color.blue);
		Debug.DrawLine(transform.position, transform.position + forwardVector*10, Color.red);
		Debug.DrawLine(transform.position, transform.position + Quaternion.Euler(forwardVector* -1f)* -gravityVector, Color.blue);

		// Angle between gravity force and forward movement of character
//		float angle = Vector3.Angle(gravityVector, forwardVector);

		// Correct height of bird
		if(gravityVector.magnitude < startupHeight-50f){
			transform.Translate(Vector3.up*0.2f);
		}
		if (gravityVector.magnitude > startupHeight-30f){
			transform.Translate(Vector3.down * 2f);
		}
	}
	
	void SetBasicWings(){
		rigLeft1.transform.localEulerAngles = new Vector3(left3.x,left3.y + (shouldercenter-(skeletonHeight/2-8))*-4, left3.z);
		rigLeft2.transform.localEulerAngles = new Vector3(left5.x,left5.y + (shouldercenter-(skeletonHeight/2-8))*-4, left5.z);
		rigRight1.transform.localEulerAngles = new Vector3(right3.x,right3.y + (shouldercenter-(skeletonHeight/2-8))*4, right3.z);
		rigRight2.transform.localEulerAngles = new Vector3(right5.x,right5.y + (shouldercenter-(skeletonHeight/2-8))*4, right5.z);



		//		rigRight1.transform.Rotate (Vector3.right * (right_wing_current-right_wing_old)*100);
//		rigLeft2.transform.Rotate (Vector3.right * (left_wing_current-left_wing_old)*100);
//		rigRight2.transform.Rotate (Vector3.right * (right_wing_current-right_wing_old)*100);
	}

	void BasicRoll(){
		roll_factor = -(shouldercenterXzero-shouldercenterX)/300f;
		//roll_factor = Mathf.Clamp (roll_factor, -0.4f, 0.4f);
		transform.Rotate(Vector3.up * -roll_factor * roll_speed);
	}

	void InitWings(){
		left3 = rigLeft1.transform.localEulerAngles;
		right3 = rigRight1.transform.localEulerAngles;
		left5 = rigLeft2.transform.localEulerAngles;
		right5 = rigRight2.transform.localEulerAngles;
	}

	void ResetWings(){
		rigLeft1.transform.localEulerAngles = left3;
		rigRight1.transform.localEulerAngles = right3;
		rigLeft2.transform.localEulerAngles = left5;
		rigRight2.transform.localEulerAngles = right5;
	}
		
	void GetDepthMap(){
		WWW www = new WWW(depth);
		StartCoroutine(WaitForRequestDepth(www));
	}

	IEnumerator WaitForRequestDepth(WWW www){
		yield return www;
		if(www.error == null){
//			depthTexture = www.texture;
		}else{
			Debug.Log("Depthmap Error: "+ www.error);
		}
		GetDepthMap();
	}

	// Get Skeleton data
	void GetSkeleton(){
		WWW www = new WWW(url);
		StartCoroutine(WaitForRequest(www));
	}


	// Get Skeleton data only when request is succesful
	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		// Check if request is succesful
		if (www.error == null)
		{
			string skeleton = www.text;
			JSONObject j = new JSONObject(skeleton);
			JSONObject obj = (JSONObject)(j.GetField ("Skeletons"));
			if(obj[local] != null && obj[local].GetField ("Skeleton").GetField("Joints").list[2].GetField ("Ts").n != 0){
				// Store Kinect data in variables. Only Y position is stored.
				shouldercenter = obj[local].GetField("Skeleton").GetField("Joints").list[2].GetField("Y").n;
				shouldercenterX = obj[local].GetField("Skeleton").GetField("Joints").list[2].GetField("X").n;
				lefthand = obj[local].GetField("Skeleton").GetField("Joints").list[7].GetField("Y").n;
				righthand = obj[local].GetField("Skeleton").GetField("Joints").list[11].GetField("Y").n;

				// One or more Skeletons are present
				if(!visibleSkeleton){
					skeletonEnteredScreen = true;
					Debug.Log("Skeleton is entered the screen");
					welcomeTimer = welcomeDuration;
				}
				skeletonBufferTimer = skeletonBufferDuration;
				visibleSkeleton = true;

				// Set initial position of wings from T shape calibration
				if(welcomeTimer > (3.2f/6f)*welcomeDuration){
					lefthandzero = lefthand;
					righthandzero = righthand;
					shouldercenterXzero = skeletonWidth/2;
					SetWingsWithSkeleton();
				}
			} else {
				if(skeletonBufferTimer == 0){
					// No Skeleton detected for 30 times
					if(visibleSkeleton){
						skeletonEnteredScreen = true;
						Debug.Log ("Skeleton left the screen");
						welcomeTimer = 0;
					}
					visibleSkeleton = false;
				}else{
					// Wait until Skeleton is really gone (30 frames)
					skeletonBufferTimer--;
				}
			}
		} else {
			// Log errors if request is unsuccesful and assume there is no valid Skeleton to control the character
			Debug.Log("Skeleton Error: "+ www.error);
			visibleSkeleton = false;
			//GetSkeleton();
		}
		GetSkeleton();
	}
}