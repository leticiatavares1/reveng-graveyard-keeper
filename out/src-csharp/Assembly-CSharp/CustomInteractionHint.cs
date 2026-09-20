using UnityEngine;

public class CustomInteractionHint : MonoBehaviour
{
	public string hint;

	public bool has_hint => !string.IsNullOrEmpty(hint);

	public string GetHint()
	{
		return hint;
	}
}
