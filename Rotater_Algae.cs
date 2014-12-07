using UnityEngine;
using System.Collections;




public class Rotater_Algae : MonoBehaviour {

	private GameObject Centre;
	private float ranfac;

	void Start(){

		ranfac = (Random.value - 0.5f);

		}

	void Update () {

		Centre = GameObject.FindGameObjectWithTag("AlgaeCentre");

		//transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		transform.RotateAround(this.transform.position, this.transform.right, 20 * Time.deltaTime);

		transform.RotateAround(Centre.transform.position, Centre.transform.up, ranfac * 10 * Time.deltaTime);


	}
}
