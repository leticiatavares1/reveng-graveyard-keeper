using UnityEngine;

public class Trampoline : MonoBehaviour
{
	[SerializeField]
	private float bounceForce = 100f;

	private void OnCollisionEnter(Collision other)
	{
		Rigidbody component = other.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			Vector3 linearVelocity = base.transform.up * bounceForce;
			component.linearVelocity = linearVelocity;
		}
	}
}
