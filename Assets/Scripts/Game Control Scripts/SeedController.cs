using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedController : MonoBehaviour {

	public string priorSeed;
	public string seed = "I wandered lonely as a cloud";
	public bool changeWorld = false;

	public void TryChangeWorld()
	{
		GameObject inputFieldWrapper = GameObject.Find ("InputField");
		InputField inputField = inputFieldWrapper.GetComponent<InputField>();

		string proposedSeed = inputField.text;
		if (proposedSeed != "") {
			Debug.Log ("proposedSeed is " + proposedSeed.ToString () + "; changing world");
			priorSeed = seed;
			seed = proposedSeed;
			changeWorld = true;
		}
	}
}
