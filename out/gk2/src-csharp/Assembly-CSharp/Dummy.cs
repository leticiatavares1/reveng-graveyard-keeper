using UnityEngine;

public class Dummy : MonoBehaviour
{
	[SerializeField]
	private WorldFX worldFx;

	[SerializeField]
	private Animator animator;

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PikeHitBox>() != null)
		{
			worldFx.Play(null);
			animator.SetTrigger("Hit");
		}
	}
}
