using System;

namespace LazyBearTechnology;

public interface IDelayedUIHide
{
	bool ShouldDelayHide { get; }

	void AddHideAfterDelayCallback(Action callback);

	void ForceCancelDelayedHide();
}
