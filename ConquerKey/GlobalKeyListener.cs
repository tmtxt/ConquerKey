using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace ConquerKey;

public class GlobalKeyListener : IGlobalKeyListener
{
	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private LowLevelKeyboardProc _hookCallbackDelegate;

	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	private IntPtr _hookId = IntPtr.Zero;

	public void StartListening()
	{
		using var curProcess = Process.GetCurrentProcess();
		using var curModule = curProcess.MainModule;
		if (curModule == null)
		{
			throw new Exception("Module not found");
		}

		_hookId = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, GetModuleHandle(curModule.ModuleName), 0);
	}

	public void StopListening()
	{
		if (_hookId == IntPtr.Zero)
		{
			return;
		}

		UnhookWindowsHookEx(_hookId);
		_hookId = IntPtr.Zero;
	}

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode < 0 || wParam != WM_KEYDOWN)
		{
			return CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		var vkCode = Marshal.ReadInt32(lParam);
		if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
			(Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin)) &&
			vkCode == KeyInterop.VirtualKeyFromKey(Key.T))
		{
			Console.WriteLine("test");
		}

		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}

	public void Dispose()
	{
		StopListening();
	}
}