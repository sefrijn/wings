/*
 * 	Controls Startup Screens and Configuration
 */
// TODO mapping of start screen to size of half width screen so camera wont stretch

using UnityEngine;
using System.Collections;

public class StartScreens : MonoBehaviour {
	// Startup Screens
	public Texture[] screens;

	// Two different Cameras
	public Camera camMain;
	public Camera camStartup;

	// Live Kinect video variables
	Texture2D liveFeed;

	WWW www;
	bool drawReady = false;

	// Reference variable
	PlayerController player;

	void Start(){
		player = GetComponent<PlayerController> ();
		liveFeed = new Texture2D (player.colorWidth, player.colorHeight);
		GetImage ();

		// Default is Main Camera enabled, StartupScreen Camera disabled
		camMain.enabled = true;
		camStartup.enabled = true;
	}


	void FixedUpdate(){
		if (player.welcomeTimer > 0) {
			// Startup
			player.welcomeTimer--;
			camMain.enabled = false;
			camStartup.enabled = true;
			player.isStartingUp = true;
			camStartup.transform.RotateAround(player.transform.position, camStartup.transform.up, 0.6f);
		}else if((!player.developmentMode && !player.visibleSkeleton)){
			// No Skeleton and No development mode means production mode on Idle
			camMain.enabled = false;
			camStartup.enabled = true;
			player.isStartingUp = true;
			camStartup.transform.RotateAround(player.transform.position, camStartup.transform.up, 0.6f);
		}else{
			// Main game
			camMain.enabled = true;
			camStartup.enabled = false;
			player.isStartingUp = false;
		}
	}

	// Draw GUI with info
	void OnGUI () {
		GUI.depth = 11;
//		if (!drawReady && player.isStartingUp){
//			Texture2D b = new Texture2D(1,1);
//			b.SetPixel (1,1,Color.black);
//			b.wrapMode = TextureWrapMode.Repeat;
//			b.Apply();
//			GUI.DrawTexture(new Rect(0, 0, Screen.width/2, Screen.height), b);
//		}
		DrawVideoFeed ();
		DrawStrartScreens();
	}


	void DrawVideoFeed(){
		// Draw live video feed on the left side or black screen
		if(drawReady && player.isStartingUp){
			GUI.DrawTexture (new Rect (0, 0, Screen.width/2, Screen.height), liveFeed, ScaleMode.StretchToFill, true, 0);
		}
	}

	void DrawStrartScreens(){
		// Cycle through different screens
		if(!player.developmentMode && !player.visibleSkeleton){
			// Draw Idle screen
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[0], ScaleMode.StretchToFill, true, 0);
		}
		if(player.basicVersion){
			if (player.welcomeTimer > 5f*player.welcomeDuration/6f) {
				// Skeleton enters screen
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[14], ScaleMode.StretchToFill, true, 0);
			}else if (player.welcomeTimer > 4f*player.welcomeDuration/6f) {
				// Multiple users warning
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[13], ScaleMode.StretchToFill, true, 0);
			}else if (player.welcomeTimer > 3.2f*player.welcomeDuration/6f) {
				// Body in T shape
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[9], ScaleMode.StretchToFill, true, 0);
			}else if (player.welcomeTimer > 3f*player.welcomeDuration/6f) {
				// Body in T shape saved
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[10], ScaleMode.StretchToFill, true, 0);
			}else if (player.welcomeTimer > 2f*player.welcomeDuration/6f) {
				// Flap arms to fly
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[11], ScaleMode.StretchToFill, true, 0);
			}else if (player.welcomeTimer > 1f*player.welcomeDuration/6f) {
				// Tilt arms to steer
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[12], ScaleMode.StretchToFill, true, 0);
			}else if (player.welcomeTimer > 0) {
				// Powerups
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), screens[8], ScaleMode.StretchToFill, true, 0);
			}
		}
	}

	void GetImage(){
		www = new WWW(player.img);
		StartCoroutine(loadTexture(www));
	}

	// Load Live feed image and wait for request to complete
	IEnumerator loadTexture(WWW www2) {
		yield return www2;
		
		// Check if request is succesful
		if (www2.error == null)
		{
			www2.LoadImageIntoTexture(liveFeed);
			drawReady = true;
		} else {
			// Log errors if request is unsuccesful and assume there is no valid Skeleton to control the character
			Debug.Log("Error StartScreen URL call: "+ www2.error);
		} 
		GetImage ();
	}
}