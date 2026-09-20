using UnityEngine;

public class MA_LocalizationUI : MonoBehaviour
{
	private void OnGUI()
	{
		GUI.Label(new Rect(20f, 40f, 640f, 200f), "This scene shows the automatic Localization of Resource files. Preview the 'hello' sound from the mixer, which will be in Spanish first. Then press stop, and change the 'Use Specific Language' language to another language up in the top section of the Master Audio prefab's Inspector, hit play and hear the difference! The correct folder's audio file will be loaded automatically according to your language settings.");
	}
}
