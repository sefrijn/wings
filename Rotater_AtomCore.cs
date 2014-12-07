using UnityEngine;
using System.Collections;




public class Rotater_AtomCore : MonoBehaviour {

	void Update () {
		//transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		transform.RotateAround(this.transform.position, Vector3.up, 20 * Time.deltaTime);
	}
}
