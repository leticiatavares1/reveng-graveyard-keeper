using UnityEngine;

[DisallowMultipleComponent]
public class UITutorialWindowPageHeaderOverride : MonoBehaviour
{
	[SerializeField]
	private string headerLocaleId;

	public string HeaderLocaleId => headerLocaleId;
}
