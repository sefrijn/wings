using UnityEngine;
using System.Collections;

public class ServerController : MonoBehaviour {
	// Network
	GameObject player;
	GameObject other;
	PlayerController playerController;
	OtherController otherController;

	public bool isMaster;
	public string ServerIP = "";
	public int Port = 25000;

	void Start () {
		// Network
		player = GameObject.Find("Player");
		playerController = player.GetComponent<PlayerController>();
		other = GameObject.Find ("Other");
		otherController = other.GetComponent<OtherController>();

		if(!playerController.developmentMode){
			if(!isMaster){
				Network.Connect(ServerIP,Port);
				Debug.Log ("Client started");
			}else{
				bool useNat = !Network.HavePublicAddress();
				Network.InitializeServer(32, Port,useNat);
				Debug.Log ("Server started");
			}
		}
	}

	void OnFailedToConnect(NetworkConnectionError error) {
		Debug.Log("Could not connect to server: " + error);
	}

	void FixedUpdate(){
		if(Network.peerType != NetworkPeerType.Disconnected){
			Vector3 pos = player.transform.position;
			Quaternion rot = player.transform.rotation;
			Vector3 rigLeft1 = playerController.rigLeft1.transform.localEulerAngles;
			Vector3 rigLeft2 = playerController.rigLeft2.transform.localEulerAngles;
			Vector3 rigRight1 = playerController.rigRight1.transform.localEulerAngles;
			Vector3 rigRight2 = playerController.rigRight2.transform.localEulerAngles;
			networkView.RPC ("PrintText", RPCMode.Others, pos, rot, rigLeft1, rigLeft2, rigRight1, rigRight2);
		}
	}

	void OnGUI(){
		GUI.depth = 10;
		if(Input.GetButton("Jump")){
			if(Network.peerType == NetworkPeerType.Client){
				GUI.Label(new Rect(200,50,100,25),"Is Client");
				if(GUI.Button(new Rect(200,125,100,25),"Logout")){
					Network.Disconnect(250);
				}
			}
			if(Network.peerType == NetworkPeerType.Server){
				GUI.Label(new Rect(200,75,100,25),"Is Server");
				GUI.Label(new Rect(200,125,100,25),"Connections: " + Network.connections.Length);
				if(GUI.Button(new Rect(0,159,100,25),"Logout")){
					Network.Disconnect(250);
				}
			}
		}
	}

	[RPC]
	void PrintText (Vector3 pos, Quaternion rot, Vector3 l1, Vector3 l2, Vector3 r1, Vector3 r2){
		other.transform.position = pos;
		other.transform.rotation = rot;
		otherController.rigLeft1.transform.localEulerAngles = l1;
		otherController.rigLeft2.transform.localEulerAngles = l2;
		otherController.rigRight1.transform.localEulerAngles = r1;
		otherController.rigRight2.transform.localEulerAngles = r2;
	}
}
