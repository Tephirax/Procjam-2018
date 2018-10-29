using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 3.0f;
	public float maxMoveSpeed = 3.0f;
	private bool writingMode = false;

	public GameObject inputFieldWrapper;
	public InputField inputField;
	private GameObject seedControllerWrapper;
	private SeedController seedController;


	void Start ()
	{
		inputFieldWrapper = GameObject.Find ("InputField");
		inputField = inputFieldWrapper.GetComponent<InputField>();
		seedControllerWrapper = GameObject.Find ("SeedController");
		seedController = seedControllerWrapper.GetComponent<SeedController>();
	}

	void Update()
	{
		if (!writingMode) {
			var x = Input.GetAxis ("Horizontal") * Time.deltaTime * moveSpeed;
			var z = Input.GetAxis ("Vertical") * Time.deltaTime * moveSpeed;

			//De-emphasise diagonal movement
			if (x + z > maxMoveSpeed) {
				x /= 2;
				z /= 2;
			}

			//transform.Rotate(0, x, 0); //Rotation now handled by Mouselook script
			transform.Translate (x, 0, z);

			if (Input.GetKeyDown (KeyCode.G)) {
				GetComponent<Rigidbody> ().useGravity = !GetComponent<Rigidbody> ().useGravity;
			}

			if (Input.GetKeyDown (KeyCode.Tab)) {
				writingMode = true;
				inputField.interactable = true;
				inputField.Select();
				inputField.ActivateInputField();
				Debug.Log ("Writing mode activated; controls paused");
			}
		} 
		else 
		{
			if (Input.GetKeyDown (KeyCode.Tab)) {
				writingMode = false;
				inputField.interactable = false;
				inputField.DeactivateInputField();
				Debug.Log ("Writing mode deactivated; controls active");
				//Try to change the world;
				seedController.TryChangeWorld();
			}
		}
	}
}