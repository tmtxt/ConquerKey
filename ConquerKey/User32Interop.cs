using System.Runtime.InteropServices;

namespace ConquerKey;

public static class User32Interop
{
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

	[DllImport("user32.dll")]
	public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();
}