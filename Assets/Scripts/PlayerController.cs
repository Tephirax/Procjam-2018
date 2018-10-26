using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 3.0f;
	public float maxMoveSpeed = 3.0f;

	void Update()
	{
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
		var z = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

		//De-emphasise diagonal movement
		if (x + z > maxMoveSpeed) {
			x /= 2;
			z /= 2;
		}

		//transform.Rotate(0, x, 0); //Rotation now handled by Mouselook script
		transform.Translate(x, 0, z);

		if (Input.GetKeyDown(KeyCode.G)) {
			GetComponent<Rigidbody>().useGravity = !GetComponent<Rigidbody>().useGravity;
		}
	}
}