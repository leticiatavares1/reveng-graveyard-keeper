using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Override Audio Settings", 0)]
[Category("Game/Audio")]
[Description("Overrides Music and SFX volumes in GameSettings. Can restore them back to original values.")]
[Color("f5da42")]
public class Flow_OverrideAudioSettings : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool restore;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<float> musicVolume;

	private ValueInput<float> sfxVolume;

	private float? originalMusicVolume;

	private float? originalSfxVolume;

	public override string name
	{
		get
		{
			if (!restore)
			{
				return "Override Audio Settings";
			}
			return "Restore Audio Settings";
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Execute);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!restore)
		{
			musicVolume = AddValueInput<float>("musicVolume".CapitalizeFirst());
			sfxVolume = AddValueInput<float>("sfxVolume".CapitalizeFirst());
		}
	}

	private void Execute(Flow flow)
	{
		GameSettings instance = GameSettings.Instance;
		if (restore)
		{
			if (originalMusicVolume.HasValue)
			{
				instance.musicVolume = originalMusicVolume.Value;
				instance.sfxVolume = originalSfxVolume.Value;
				instance.ApplyAudioSettings();
				originalMusicVolume = null;
				originalSfxVolume = null;
			}
		}
		else
		{
			if (!originalMusicVolume.HasValue)
			{
				originalMusicVolume = instance.musicVolume;
				originalSfxVolume = instance.sfxVolume;
			}
			instance.musicVolume = musicVolume.value;
			instance.sfxVolume = sfxVolume.value;
			instance.ApplyAudioSettings();
		}
		@out.Call(flow);
	}
}
