using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class GameShutdown
{
	private const float QUIT_DEFER_TIMEOUT_SECONDS = 15f;

	private static CancellationTokenSource cts = new CancellationTokenSource();

	private static bool hooksRegistered;

	private static Func<bool> quitBlocker;

	private static bool quitDeferred;

	private static bool quitForced;

	public static bool IsRequested { get; private set; }

	public static bool IsQuitting { get; private set; }

	public static CancellationToken Token
	{
		get
		{
			if (IsRequested)
			{
				return new CancellationToken(canceled: true);
			}
			if (cts == null)
			{
				cts = new CancellationTokenSource();
			}
			return cts.Token;
		}
	}

	public static event Action Resumed;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatics()
	{
		UninstallHooks();
		RestoreToken();
		GameShutdown.Resumed = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InstallHooks()
	{
		if (!hooksRegistered)
		{
			hooksRegistered = true;
			Application.wantsToQuit += OnWantsToQuit;
			Application.quitting += RequestQuit;
		}
	}

	private static void UninstallHooks()
	{
		if (hooksRegistered)
		{
			Application.wantsToQuit -= OnWantsToQuit;
			Application.quitting -= RequestQuit;
			hooksRegistered = false;
		}
	}

	public static void Request()
	{
		if (!IsRequested)
		{
			IsRequested = true;
			Debug.Log($"#shutdown# GameShutdown.Request: cancelling token (isQuitting:[{IsQuitting}])");
			try
			{
				cts?.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
			Debug.Log("#shutdown# GameShutdown.Request: token cancelled, unwind returned");
		}
	}

	public static void RequestQuit()
	{
		Debug.Log($"#shutdown# GameShutdown.RequestQuit (wasRequested:[{IsRequested}])");
		IsQuitting = true;
		Request();
	}

	public static void ThrowIfRequested()
	{
		Token.ThrowIfCancellationRequested();
	}

	public static void SetQuitBlocker(Func<bool> blocker)
	{
		quitBlocker = blocker;
	}

	public static async UniTask<bool> WaitForResumeAsync()
	{
		UniTaskCompletionSource<bool> completion = new UniTaskCompletionSource<bool>();
		Resumed += OnResumed;
		try
		{
			if (IsQuitting)
			{
				return false;
			}
			if (!IsRequested)
			{
				return true;
			}
			return await completion.Task;
		}
		finally
		{
			Resumed -= OnResumed;
		}
		void OnResumed()
		{
			completion.TrySetResult(result: true);
		}
	}

	private static bool OnWantsToQuit()
	{
		Debug.Log("#shutdown# Application.wantsToQuit received");
		RequestQuit();
		if (!quitForced && IsQuitBlocked())
		{
			if (!quitDeferred)
			{
				quitDeferred = true;
				QuitWhenUnblocked().Forget();
			}
			Debug.Log("#shutdown# Application.wantsToQuit: deferring quit until in-flight engine work lands");
			return false;
		}
		Debug.Log("#shutdown# Application.wantsToQuit: allowing quit");
		return true;
	}

	private static bool IsQuitBlocked()
	{
		if (quitBlocker == null)
		{
			return false;
		}
		try
		{
			return quitBlocker();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	private static async UniTaskVoid QuitWhenUnblocked()
	{
		float deadline = Time.realtimeSinceStartup + 15f;
		while (IsQuitBlocked() && Time.realtimeSinceStartup < deadline)
		{
			await UniTask.Yield();
		}
		if (IsQuitBlocked())
		{
			quitForced = true;
			Debug.LogWarning($"#shutdown# quit still blocked after {15f}s, quitting anyway");
		}
		await UniTask.NextFrame();
		Debug.Log("#shutdown# re-issuing Application.Quit");
		Application.Quit();
	}

	private static void RestoreToken()
	{
		Debug.Log($"#shutdown# GameShutdown.RestoreToken (wasRequested:[{IsRequested}] wasQuitting:[{IsQuitting}])");
		IsRequested = false;
		IsQuitting = false;
		quitDeferred = false;
		quitForced = false;
		try
		{
			cts?.Dispose();
		}
		catch (ObjectDisposedException)
		{
		}
		cts = new CancellationTokenSource();
	}
}
