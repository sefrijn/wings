using UnityEngine;
using System.Collections;




public class Rotater_DNA : MonoBehaviour {

	void Update () {
		//transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		transform.RotateAround(this.transform.position, this.transform.up, 4 * Time.deltaTime);

	}
}
