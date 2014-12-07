using UnityEngine;
using System.Collections;


public class Rotater_Discs : MonoBehaviour {

	public float Rara = 0f;

	void Update () {
		//transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		transform.RotateAround (this.transform.position, this.transform.forward, Rara * 4 * Time.deltaTime);
	}
}
