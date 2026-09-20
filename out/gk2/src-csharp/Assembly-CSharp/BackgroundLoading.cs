using System.Threading;
using Cysharp.Threading.Tasks;

public static class BackgroundLoading
{
	public static bool IsActive;

	public static int YieldEveryMultiplier = 1;

	public static bool ShouldYield(int index, int yieldEveryBase)
	{
		if (IsActive)
		{
			return index % (yieldEveryBase * YieldEveryMultiplier) == 0;
		}
		return false;
	}

	public static UniTask YieldIfNeeded(int index, int yieldEveryBase)
	{
		CancellationToken token = GameShutdown.Token;
		token.ThrowIfCancellationRequested();
		if (!ShouldYield(index, yieldEveryBase))
		{
			return UniTask.CompletedTask;
		}
		return UniTask.Yield(PlayerLoopTiming.Update, token);
	}
}
