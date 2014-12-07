using UnityEngine;
using System.Collections;




public class Rotate_Around_Self : MonoBehaviour {

	public GameObject Object;

	void Update () {
		//transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		transform.RotateAround(Object.transform.position, Object.transform.right, 20 * Time.deltaTime);

	}
}
