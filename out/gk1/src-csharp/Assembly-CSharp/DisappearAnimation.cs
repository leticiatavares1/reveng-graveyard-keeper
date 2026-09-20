using UnityEngine;

public class DisappearAnimation : MonoBehaviour
{
	public virtual void StartAnimation(GJCommons.VoidDelegate on_done)
	{
		on_done?.Invoke();
	}
}
