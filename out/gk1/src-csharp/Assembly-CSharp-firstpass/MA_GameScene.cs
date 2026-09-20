using UnityEngine;

public class MA_GameScene : MonoBehaviour
{
	private void OnGUI()
	{
		GUI.Label(new Rect(20f, 40f, 640f, 190f), "This is the Game Scene. We have used a Dynamic Sound Group Creator prefab to populate temporary Sound Groups into Master Audio as soon as that prefab becomes enabled (on Scene change for us). If we were to load a different Scene now, those sounds would vanish from the mixer.");
	}
}
