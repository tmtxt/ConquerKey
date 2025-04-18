using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ConquerKey;

public static class WindowUtilities {

	/// <summary>
	/// Activates a WPF window even if the window is activated on a separate thread
	/// </summary>
	/// <param name="window"></param>
	public static void ActivateWindow(Window window)
	{
		var hwnd = new WindowInteropHelper(window).EnsureHandle();

		var threadId1 = User32Interop.GetWindowThreadProcessId(User32Interop.GetForegroundWindow(), IntPtr.Zero);
		var threadId2 = User32Interop.GetWindowThreadProcessId(hwnd, IntPtr.Zero);

		if (threadId1 != threadId2)
		{
			User32Interop.AttachThreadInput(threadId1, threadId2, true);
			User32Interop.SetForegroundWindow(hwnd);
			User32Interop.AttachThreadInput(threadId1, threadId2, false);
		}
		else
			User32Interop.SetForegroundWindow(hwnd);
	}

}