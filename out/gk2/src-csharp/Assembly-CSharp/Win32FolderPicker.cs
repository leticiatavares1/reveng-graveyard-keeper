using System;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class Win32FolderPicker
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int ShowDelegate(IntPtr thisPtr, IntPtr hwnd);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int SetOptionsDelegate(IntPtr thisPtr, uint fos);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int SetFolderDelegate(IntPtr thisPtr, IntPtr psi);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int SetTitleDelegate(IntPtr thisPtr, [MarshalAs(UnmanagedType.LPWStr)] string title);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int GetResultDelegate(IntPtr thisPtr, out IntPtr item);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int GetDisplayNameDelegate(IntPtr thisPtr, int sigdn, out IntPtr name);

	private const uint ClsctxInprocServer = 1u;

	private const int SigdnFilesyspath = -2147123200;

	private const uint FosNoChangeDir = 8u;

	private const uint FosPickFolders = 32u;

	private const uint FosForceFileSystem = 64u;

	private const uint FosPathMustExist = 2048u;

	private const int ErrorCancelled = -2147023673;

	public static string PickFolder(string title, string startDirectory)
	{
		Guid rclsid = new Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");
		Guid riid = new Guid("D57C7288-D4AD-4768-BE02-9D969532D960");
		int num = CoCreateInstance(ref rclsid, IntPtr.Zero, 1u, ref riid, out var ppv);
		if (num != 0 || ppv == IntPtr.Zero)
		{
			Debug.LogWarning($"[SteamWorkshopCreator] Failed to create folder dialog ({num:X8}).");
			return "";
		}
		try
		{
			SetOptions(ppv, 2152u);
			if (!string.IsNullOrEmpty(title))
			{
				SetTitle(ppv, title);
			}
			if (!string.IsNullOrEmpty(startDirectory))
			{
				TrySetFolder(ppv, startDirectory);
			}
			num = Show(ppv, GetActiveWindow());
			if (num == -2147023673 || num != 0)
			{
				return "";
			}
			if (GetResult(ppv, out var item) != 0 || item == IntPtr.Zero)
			{
				return "";
			}
			try
			{
				if (GetDisplayName(item, -2147123200, out var name) != 0 || name == IntPtr.Zero)
				{
					return "";
				}
				object obj = Marshal.PtrToStringUni(name);
				Marshal.FreeCoTaskMem(name);
				if (obj == null)
				{
					obj = "";
				}
				return (string)obj;
			}
			finally
			{
				Marshal.Release(item);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[SteamWorkshopCreator] Folder dialog failed: " + ex.Message);
			return "";
		}
		finally
		{
			Marshal.Release(ppv);
		}
	}

	private static void TrySetFolder(IntPtr dialog, string startDirectory)
	{
		Guid riid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
		if (SHCreateItemFromParsingName(startDirectory, IntPtr.Zero, ref riid, out var ppv) == 0 && !(ppv == IntPtr.Zero))
		{
			SetFolder(dialog, ppv);
			Marshal.Release(ppv);
		}
	}

	private static IntPtr VTable(IntPtr com, int index)
	{
		return Marshal.ReadIntPtr(Marshal.ReadIntPtr(com), index * IntPtr.Size);
	}

	private static int Show(IntPtr dialog, IntPtr hwnd)
	{
		return Marshal.GetDelegateForFunctionPointer<ShowDelegate>(VTable(dialog, 3))(dialog, hwnd);
	}

	private static void SetOptions(IntPtr dialog, uint fos)
	{
		Marshal.GetDelegateForFunctionPointer<SetOptionsDelegate>(VTable(dialog, 9))(dialog, fos);
	}

	private static void SetFolder(IntPtr dialog, IntPtr folder)
	{
		Marshal.GetDelegateForFunctionPointer<SetFolderDelegate>(VTable(dialog, 12))(dialog, folder);
	}

	private static void SetTitle(IntPtr dialog, string title)
	{
		Marshal.GetDelegateForFunctionPointer<SetTitleDelegate>(VTable(dialog, 17))(dialog, title);
	}

	private static int GetResult(IntPtr dialog, out IntPtr item)
	{
		return Marshal.GetDelegateForFunctionPointer<GetResultDelegate>(VTable(dialog, 20))(dialog, out item);
	}

	private static int GetDisplayName(IntPtr item, int sigdn, out IntPtr name)
	{
		return Marshal.GetDelegateForFunctionPointer<GetDisplayNameDelegate>(VTable(item, 5))(item, sigdn, out name);
	}

	[DllImport("ole32.dll")]
	private static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid riid, out IntPtr ppv);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, out IntPtr ppv);

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();
}
