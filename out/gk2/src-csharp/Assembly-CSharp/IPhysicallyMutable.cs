using System;

public interface IPhysicallyMutable
{
	bool IsMuted { get; set; }

	void Mute(Action onUnmuted = null);
}
