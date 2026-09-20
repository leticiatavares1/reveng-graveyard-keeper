using UnityEngine;

public class BodyAreaDestroyer : MonoBehaviour
{
	public AreaBodiesSpawner spawner;

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<Rigidbody>().gameObject.name.Contains("Body_Trailer"))
		{
			spawner.RemoveBody(other.gameObject);
		}
	}
}
