using System;

namespace DarkTonic.MasterAudio;

[Serializable]
public class CustomEventCategory
{
	public string CatName = "[Uncategorized]";

	public bool IsExpanded = true;

	public bool IsEditing;

	public bool IsTemporary;

	public string ProspectiveName = "[Uncategorized]";
}
