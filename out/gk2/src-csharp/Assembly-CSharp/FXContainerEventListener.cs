using UnityEngine;

public class FXContainerEventListener : MonoBehaviour
{
	[SerializeField]
	private FXContainer fxContainer;

	public void PlayFx(string fxName)
	{
		fxContainer.PlayFx(fxName);
	}
}
