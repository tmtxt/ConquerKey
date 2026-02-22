using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using ConquerKey.ActionHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConquerKey;

public class GlobalKeyListener : IGlobalKeyListener
{
	#region User32.dll interop

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

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

	#endregion

	private IntPtr _hookId = IntPtr.Zero;
	private readonly IServiceProvider _serviceProvider;
	private readonly ActionManager _actionManager;
	private LowLevelKeyboardProc _lowLevelKeyboardProc;

	public GlobalKeyListener(IServiceProvider serviceProvider, ActionManager actionManager)
	{
		_serviceProvider = serviceProvider;
		_actionManager = actionManager;
		_lowLevelKeyboardProc = HookCallback;
	}

	public void StartListening()
	{
		// For single-file published apps, use "user32.dll" as a fallback
		// since the main module may not be accessible in the expected way
		var moduleHandle = IntPtr.Zero;

		using var curProcess = Process.GetCurrentProcess();
		using var curModule = curProcess.MainModule;

		if (curModule != null)
		{
			moduleHandle = GetModuleHandle(curModule.ModuleName);
		}

		// Fallback: use IntPtr.Zero which works for global hooks
		if (moduleHandle == IntPtr.Zero)
		{
			moduleHandle = GetModuleHandle(null!);
		}

		_hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _lowLevelKeyboardProc, moduleHandle, 0);

		if (_hookId == IntPtr.Zero)
		{
			throw new Exception($"Failed to set keyboard hook. Error code: {Marshal.GetLastWin32Error()}");
		}
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
		var key = KeyInterop.KeyFromVirtualKey(vkCode);

		// Build the current modifier keys state
		var modifiers = ModifierKeys.None;
		if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			modifiers |= ModifierKeys.Control;
		if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
			modifiers |= ModifierKeys.Alt;
		if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			modifiers |= ModifierKeys.Shift;
		if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
			modifiers |= ModifierKeys.Windows;

		// Try to find an action matching the current key combination
		if (_actionManager.TryGetAction(modifiers, key, out var action) && action != null)
		{
			var actionWindow = ActivatorUtilities.CreateInstance<Windows.ActionWindow>(_serviceProvider, action);
			actionWindow.Show();
			WindowUtilities.ActivateWindow(actionWindow);
		}

		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}

	public void Dispose()
	{
		StopListening();
	}
}