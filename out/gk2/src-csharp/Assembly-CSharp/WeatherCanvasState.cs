using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Name("Weather State", 0)]
[Category("Game")]
public class WeatherCanvasState : ActionTask<WeatherComponent>
{
	protected override string info => $"Weather: {base.agentInfo}";

	protected override void OnExecute()
	{
		base.agent.FadeIn();
	}

	protected override void OnStop()
	{
		base.agent.FadeOut();
	}
}
