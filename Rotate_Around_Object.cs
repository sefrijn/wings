using UnityEngine;
using System.Collections;




public class Rotate_Around_Object : MonoBehaviour {

	public GameObject Object;
	public float localRotSpeed = 5;
	public float globalRotSpeed = 1;
	private float ranfac;

	void Start(){

		ranfac = (Random.value - 0.5f);

		}

	void Update () {

		//transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		transform.RotateAround(this.transform.position, this.transform.right, localRotSpeed * Time.deltaTime);

		transform.RotateAround(Object.transform.position, Object.transform.up, ranfac * globalRotSpeed * Time.deltaTime);


	}
}
