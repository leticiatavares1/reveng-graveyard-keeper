using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using UnityEngine;

namespace NGTools;

[EditorBrowsable(EditorBrowsableState.Never)]
[Browsable(false)]
public class InternalNGDebug
{
	public class NGTools : Exception
	{
		public NGTools(string message)
			: base(message)
		{
		}
	}

	internal const char MultiContextsStartChar = '\u0001';

	internal const char MultiContextsEndChar = '\u0004';

	internal const char MultiContextsSeparator = ';';

	internal const char DataStartChar = '\u0002';

	internal const char DataEndChar = '\u0004';

	internal const char DataSeparator = '\n';

	internal const char DataSeparatorReplace = '\u0005';

	internal const char MultiTagsStartChar = '\u0003';

	internal const char MultiTagsEndChar = '\u0004';

	internal const char MultiTagsSeparator = ';';

	private static EventWaitHandle waitHandle;

	public static string LogPath;

	private static int lastLogHash;

	private static int lastLogCounter;

	static InternalNGDebug()
	{
		LogPath = "NGTLogs.txt";
		waitHandle = new EventWaitHandle(initialState: true, EventResetMode.AutoReset, "578943af-6fd1-4792-b36c-1713c20a37d9");
	}

	public static void Log(object message)
	{
		Debug.Log("[NG Tools Pro] " + message);
	}

	public static void Log(object message, UnityEngine.Object context)
	{
		Debug.Log("[NG Tools Pro] " + message, context);
	}

	public static void Log(int error, string message)
	{
		Debug.Log("[NG Tools Pro] #" + error + " - " + message);
	}

	public static void Log(int error, string message, UnityEngine.Object context)
	{
		Debug.Log("[NG Tools Pro] #" + error + " - " + message, context);
	}

	public static void LogWarning(string message)
	{
		Debug.LogWarning("[NG Tools Pro] " + message);
	}

	public static void LogWarning(string message, UnityEngine.Object context)
	{
		Debug.LogWarning("[NG Tools Pro] " + message, context);
	}

	public static void LogWarning(int error, string message)
	{
		Debug.LogWarning("[NG Tools Pro] #" + error + " - " + message);
	}

	public static void LogWarning(int error, string message, UnityEngine.Object context)
	{
		Debug.LogWarning("[NG Tools Pro] #" + error + " - " + message, context);
	}

	public static void LogError(object message)
	{
		Debug.LogError("[NG Tools Pro] " + message);
	}

	public static void LogError(object message, UnityEngine.Object context)
	{
		Debug.LogError("[NG Tools Pro] " + message, context);
	}

	public static void LogError(int error, string message)
	{
		Debug.LogError("[NG Tools Pro] #" + error + " - " + message);
	}

	public static void LogError(int error, string message, UnityEngine.Object context)
	{
		Debug.LogError("[NG Tools Pro] #" + error + " - " + message, context);
	}

	public static void LogException(string message, Exception exception)
	{
		Debug.LogException(new NGTools(exception.GetType().Name + ": " + message + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace));
	}

	public static void LogException(Exception exception)
	{
		Debug.LogException(new NGTools(exception.Message + Environment.NewLine + exception.StackTrace));
	}

	public static void LogException(Exception exception, UnityEngine.Object context)
	{
		Debug.LogException(new NGTools(exception.Message + Environment.NewLine + exception.StackTrace), context);
	}

	public static void LogException(string message, Exception exception, UnityEngine.Object context)
	{
		Debug.LogException(new NGTools(message + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace), context);
	}

	public static void LogException(int error, Exception exception)
	{
		Debug.LogException(new NGTools("[E" + error + "] " + exception.GetType().Name + ": " + exception.Message + Environment.NewLine + exception.StackTrace));
	}

	public static void LogException(int error, Exception exception, UnityEngine.Object context)
	{
		Debug.LogException(new NGTools("[E" + error + "] " + exception.GetType().Name + ": " + exception.Message + Environment.NewLine + exception.StackTrace), context);
	}

	public static void LogException(int error, string message, Exception exception)
	{
		Debug.LogException(new NGTools("[E" + error + "] " + message + Environment.NewLine + exception.GetType().Name + ": " + exception.Message + Environment.NewLine + exception.StackTrace));
	}

	public static void LogException(int error, string message, Exception exception, UnityEngine.Object context)
	{
		Debug.LogException(new NGTools("[E" + error + "] " + message + Environment.NewLine + exception.GetType().Name + ": " + exception.Message + Environment.NewLine + exception.StackTrace), context);
	}

	public static void InternalLog(object message)
	{
		if (Conf.DebugMode != 0)
		{
			Debug.Log("[NG Tools Pro] " + message);
		}
	}

	public static void InternalLogWarning(object message)
	{
		if (Conf.DebugMode != 0)
		{
			Debug.LogWarning("[NG Tools Pro] " + message);
		}
	}

	public static void Assert(bool assertion, object message, UnityEngine.Object context)
	{
		if (Conf.DebugMode != 0 && !assertion)
		{
			Debug.LogError("[NG Tools Pro] " + message, context);
		}
	}

	public static void Assert(bool assertion, object message)
	{
		if (Conf.DebugMode != 0 && !assertion)
		{
			Debug.LogError("[NG Tools Pro] " + message);
		}
	}

	public static void LogFile(object log)
	{
		if (Conf.DebugMode == Conf.DebugModes.None)
		{
			return;
		}
		waitHandle.WaitOne();
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			VerboseLog(log);
			int hashCode = log.GetHashCode();
			if (hashCode != lastLogHash)
			{
				lastLogHash = hashCode;
				lastLogCounter = 0;
				File.AppendAllText(LogPath, log?.ToString() + Environment.NewLine);
			}
			else
			{
				lastLogCounter++;
				if (lastLogCounter <= 2)
				{
					File.AppendAllText(LogPath, log?.ToString() + Environment.NewLine);
				}
				else if (lastLogCounter == 3)
				{
					File.AppendAllText(LogPath, "…" + Environment.NewLine);
				}
			}
		}
		else
		{
			Debug.Log("[NG Tools Pro] " + log);
		}
		waitHandle.Set();
	}

	public static void LogFileException(string message, Exception exception)
	{
		if (Conf.DebugMode == Conf.DebugModes.None)
		{
			return;
		}
		waitHandle.WaitOne();
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			VerboseLog(message);
			VerboseLogException(exception);
			int num = message.GetHashCode() + exception.GetHashCode();
			if (num != lastLogHash)
			{
				lastLogHash = num;
				lastLogCounter = 0;
				File.AppendAllText(LogPath, message + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine);
			}
			else
			{
				lastLogCounter++;
				if (lastLogCounter <= 2)
				{
					File.AppendAllText(LogPath, message + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine);
				}
				else if (lastLogCounter == 3)
				{
					File.AppendAllText(LogPath, "…" + Environment.NewLine);
				}
			}
		}
		else
		{
			Debug.LogError("[NG Tools Pro] " + message);
			Debug.LogError("[NG Tools Pro] " + exception.Message + Environment.NewLine + exception.StackTrace);
		}
		waitHandle.Set();
	}

	public static void LogFileException(Exception exception)
	{
		if (Conf.DebugMode == Conf.DebugModes.None)
		{
			return;
		}
		waitHandle.WaitOne();
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			VerboseLogException(exception);
			int hashCode = exception.GetHashCode();
			if (hashCode != lastLogHash)
			{
				lastLogHash = hashCode;
				lastLogCounter = 0;
				File.AppendAllText(LogPath, exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine);
			}
			else
			{
				lastLogCounter++;
				if (lastLogCounter <= 2)
				{
					File.AppendAllText(LogPath, exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine);
				}
				else if (lastLogCounter == 3)
				{
					File.AppendAllText(LogPath, "…" + Environment.NewLine);
				}
			}
		}
		else
		{
			Debug.LogError("[NG Tools Pro] " + exception.Message + Environment.NewLine + exception.StackTrace);
		}
		waitHandle.Set();
	}

	public static void AssertFile(bool assertion, object message)
	{
		if (Conf.DebugMode != 0 && !assertion)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
			{
				VerboseAssert((message ?? "NULL").ToString());
				LogFile((message ?? "NULL").ToString());
			}
			else
			{
				Debug.LogError("[NG Tools Pro] " + (message ?? "NULL").ToString());
			}
		}
	}

	private static void VerboseAssert(object message)
	{
		if (Conf.DebugMode == Conf.DebugModes.Verbose)
		{
			LogError(message);
		}
	}

	private static void VerboseLog(object message)
	{
		if (Conf.DebugMode == Conf.DebugModes.Verbose)
		{
			Log(message);
		}
	}

	private static void VerboseLogException(Exception exception)
	{
		if (Conf.DebugMode == Conf.DebugModes.Verbose)
		{
			LogException(exception);
		}
	}
}
