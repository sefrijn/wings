/*
 *	Loads production settings from external config.xml file, if present.
 */

using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour {
	public string configPath;
	public void Init(){
		// Set variables according to config.xml file if it exists
		if(System.IO.File.Exists (configPath)){
			Debug.Log("config.xml loaded");
			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(configPath);
			PlayerController player = GetComponent<PlayerController> ();
			GameObject otherObject = GameObject.Find("Other");
			OtherController other = otherObject.GetComponent<OtherController>();
			GameObject game = GameObject.Find ("GameController");
			ServerController serverController = game.GetComponent<ServerController>();

			// Path to server config
			reader.ReadToFollowing("isLocal");
			if(reader.ReadElementContentAsBoolean("isLocal","")){
				reader.ReadToFollowing("localpath");
				player.serverConfigPath = reader.ReadElementContentAsString("localpath", "");
			}else{
				reader.ReadToFollowing("path");
				player.serverConfigPath = reader.ReadElementContentAsString("path", "");
			}

			// Collision
			reader.ReadToFollowing("upSpeed");
			player.upSpeed = reader.ReadElementContentAsFloat("upSpeed", "");
			other.upSpeed = player.upSpeed;
			
			// Collision Camera Shake
			reader.ReadToFollowing("cameraShakeLength");
			player.cameraShakeLength = reader.ReadElementContentAsFloat("cameraShakeLength", "");

			// Frames to keep moving until state is changed to no skeleton present
			reader.ReadToFollowing("skeletonBufferDuration");
			player.skeletonBufferDuration = reader.ReadElementContentAsInt("skeletonBufferDuration","");

			// Speed Variables
			reader.ReadToFollowing("forwardSpeed");
			player.forwardSpeed = reader.ReadElementContentAsFloat("forwardSpeed", "");
			other.forwardSpeed = player.forwardSpeed;
			reader.ReadToFollowing("pitchDownSpeed");
			player.pitch_down_speed = reader.ReadElementContentAsFloat("pitchDownSpeed", "");
			other.pitch_down_speed = player.pitch_down_speed;
			reader.ReadToFollowing("pitchUpSpeed");
			player.pitch_up_speed = reader.ReadElementContentAsFloat("pitchUpSpeed", "");
			other.pitch_up_speed = player.pitch_up_speed;
			reader.ReadToFollowing("rollSpeed");
			player.roll_speed = reader.ReadElementContentAsFloat("rollSpeed", "");
			other.roll_speed = player.roll_speed;
			reader.ReadToFollowing("wingMovementFactor");
			player.wing_movement_factor =  reader.ReadElementContentAsFloat("wingMovementFactor", "");
			other.wing_movement_factor = player.wing_movement_factor;

			// Start Screen Duration
			reader.ReadToFollowing("welcomeDuration");
			player.welcomeDuration = reader.ReadElementContentAsInt("welcomeDuration", "");
			other.welcomeDuration = player.welcomeDuration;

			// Startup Height
			reader.ReadToFollowing("startupHeight");
			player.startupHeight = reader.ReadElementContentAsFloat("startupHeight", "");
			other.startupHeight = player.startupHeight;

			// Mapping variables
			reader.ReadToFollowing("mapBottom");
			player.mapBottom = reader.ReadElementContentAsFloat ("mapBottom","");
			reader.ReadToFollowing("mapTop");
			player.mapTop = reader.ReadElementContentAsFloat ("mapTop","");
			reader.ReadToFollowing("mapInverse");
			player.mapInverse = reader.ReadElementContentAsFloat ("mapInverse","");

			// Skeletong mapping variables
			reader.ReadToFollowing("skeletonWidth");
			player.skeletonWidth = reader.ReadElementContentAsFloat ("skeletonWidth","");
			reader.ReadToFollowing("skeletonHeight");
			player.skeletonHeight = reader.ReadElementContentAsFloat ("skeletonHeight","");

			// Skeleton Depth mapping variables
			reader.ReadToFollowing("depthNear");
			player.depthNear = reader.ReadElementContentAsFloat ("depthNear","");
			reader.ReadToFollowing("depthFar");
			player.depthFar = reader.ReadElementContentAsFloat ("depthFar","");

			// Basic version or Full version selection
			reader.ReadToFollowing("basicVersion");
			player.basicVersion = reader.ReadElementContentAsBoolean("basicVersion","");

			// Development mode on or off by config.xml file
			reader.ReadToFollowing("developmentMode");
			player.developmentMode = reader.ReadElementContentAsBoolean("developmentMode","");

			if(player.serverConfigPath != ""){
				// Read JSON file with config
				string file = System.IO.File.ReadAllText(player.serverConfigPath);
				JSONObject j = new JSONObject(file);

				// Define local and remote
				player.local = int.Parse(j.GetField ("local_idx").ToString ());
				int remote = int.Parse(j.GetField ("remote_idx").ToString ());

				// Set config variables
				player.url = j.GetField("locations")[player.local].GetField("server").str + "/activeskeletonsprojected";
				Debug.Log (player.url);
				player.img = j.GetField("locations")[player.local].GetField("server").str + "/color";
				player.depth = j.GetField("locations")[player.local].GetField("server").str + "/depth";
				player.depthWidth = j.GetField("specs").GetField ("depth_res").GetField("width").n;
				player.depthHeight = j.GetField("specs").GetField ("depth_res").GetField("height").n;
				player.colorWidth = (int) j.GetField("specs").GetField ("color_res").GetField("width").n;
				player.colorHeight = (int) j.GetField("specs").GetField ("color_res").GetField("height").n;
				serverController.ServerIP = j.GetField("locations")[remote].GetField("game").str;
				serverController.isMaster = j.GetField("is_master").b;
			}else{
				Debug.Log ("No server path set");
			}
		
		}else{
			Debug.Log("Config.xml not found");
			Debug.Log ("Current directory is: "+System.IO.Directory.GetCurrentDirectory());
		}
	}
}
