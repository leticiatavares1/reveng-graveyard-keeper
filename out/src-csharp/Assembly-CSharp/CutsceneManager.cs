public static class CutsceneManager
{
	private const string EXTRA_PATH = "DLC_stories/";

	public static void ExecuteCutscene(string flowscript_name, CustomFlowScript.OnFinishedDelegate onFinishedDelegate = null)
	{
		GS.RunFlowScript("DLC_stories/" + flowscript_name, onFinishedDelegate);
	}
}
