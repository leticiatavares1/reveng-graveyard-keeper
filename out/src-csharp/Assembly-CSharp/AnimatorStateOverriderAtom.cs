using System;

[Serializable]
public class AnimatorStateOverriderAtom
{
	public enum AnimatorStates
	{
		FLOAT,
		INT,
		BOOL
	}

	public AnimatorStates animator_source_state_type;

	public string source_state_name = "";

	public string destination_state_name = "";
}
