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

	[DllImport("user32.dll")]
	public static extern long SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	public extern static uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

	[StructLayout(LayoutKind.Sequential)]
	public struct INPUT
	{
		public int type;
		public InputUnion U;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct InputUnion
	{
		[FieldOffset(0)]
		public MOUSEINPUT mi;
		[FieldOffset(0)]
		public KEYBDINPUT ki;
		[FieldOffset(0)]
		public HARDWAREINPUT hi;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MOUSEINPUT
	{
		public int dx;
		public int dy;
		public int mouseData;
		public uint dwFlags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct KEYBDINPUT
	{
		public ushort wVk;
		public ushort wScan;
		public uint dwFlags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HARDWAREINPUT
	{
		public uint uMsg;
		public ushort wParamL;
		public ushort wParamH;
	}

	public const int INPUT_MOUSE = 0;
	public static uint MOUSEEVENTF_LEFTDOWN = 0x0002;
	public static uint MOUSEEVENTF_LEFTUP = 0x0004;
}