using System.Collections;
using UnityEngine;

public class InstantiatePlayer : MonoBehaviour {

	public Vector3 startingPosition;
	public Transform player;

	// Use this for initialization
	void Start () {
		Debug.Log ("Instantiate Character Controller");
		Instantiate (player, startingPosition, Quaternion.identity);
	}
}
