using UnityEngine;

public class SceneDescription : MonoBehaviour
{
	public enum SceneType
	{
		WorldMap,
		MainScene
	}

	public SceneType scene_type;

	public MainGame main_camera;

	public UIRoot ui_root;

	public SmartAudioEngine smart_audio_engine;
}
