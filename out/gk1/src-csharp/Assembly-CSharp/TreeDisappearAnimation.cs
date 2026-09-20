using System.Collections.Generic;
using DarkTonic.MasterAudio;

public class TreeDisappearAnimation : DisappearAnimation
{
	private GJCommons.VoidDelegate _on_complete;

	private readonly List<FallingPart> _parts = new List<FallingPart>();

	public override void StartAnimation(GJCommons.VoidDelegate on_complete)
	{
		_parts.Clear();
		_on_complete = on_complete;
		FallingPart[] componentsInChildren = GetComponentsInChildren<FallingPart>();
		foreach (FallingPart fallingPart in componentsInChildren)
		{
			FallingPart p1 = fallingPart;
			_parts.Add(p1);
			fallingPart.StartAnimation(delegate
			{
				_parts.Remove(p1);
			});
		}
		MasterAudio.PlaySound3DAtTransform("tree_fall", base.transform);
	}

	public void Update()
	{
		if (_on_complete != null && _parts.Count == 0)
		{
			GJCommons.VoidDelegate on_complete = _on_complete;
			_on_complete = null;
			on_complete?.Invoke();
		}
	}
}
