using UnityEngine;
using UnityEngine.SceneManagement;

public class MA_Bootstrapper : MonoBehaviour
{
	private void Awake()
	{
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(20f, 40f, 640f, 190f), "This is the Bootstrapper Scene. Set it up in BuildSettings as the first Scene. Then add '_AfterBootstrapperScene' as the second Scene. Hit play. Master Audio is configured in 'persist between Scenes' mode. Finally, click 'Load Game Scene' button and notice how the music doesn't get interruped even though we're changing Scenes. Normally a Bootstrapper Scene would not be seen. We are illustrating how to set up though. Notice that no Sound Groups are set up in Master Audio. Sample music provided by Alchemy Studios. This music 'The Epic Trailer' (longer version) is available on the Asset Store!");
		if (GUI.Button(new Rect(100f, 150f, 150f, 100f), "Load Game Scene"))
		{
			SceneManager.LoadScene(1);
		}
	}
}
