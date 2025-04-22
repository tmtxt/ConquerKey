using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;

namespace ConquerKey;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private static IntPtr _hookID = IntPtr.Zero;
	private TextBox _textBox;
	private LowLevelKeyboardProc _hookCallbackDelegate;

	public MainWindow()
	{
		InitializeComponent();
		// Loaded += MainWindow_LoadedSimple;
		// Closed += MainWindow_Closed;
	}

	private void MainWindow_Closed(object? sender, EventArgs e)
	{
		UnhookWindowsHookEx(_hookID);
	}

	private void MainWindow_LoadedSimple(object sender, RoutedEventArgs e)
	{
		_hookCallbackDelegate = HookCallback;
		_hookID = SetHook(_hookCallbackDelegate);
	}

	private void ShowHintWindow()
	{
		var activeWindow = WindowUtilities.GetActiveWindow();
		var hintWindow = new HintWindow(activeWindow);
		// hintWindow.Topmost = true;
		hintWindow.Show();
		// hintWindow.Activate();
		WindowUtilities.ActivateWindow(hintWindow);
	}

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
		{
			int vkCode = Marshal.ReadInt32(lParam);
			if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
				(Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin)) &&
				//(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) &&
				//(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) &&
				vkCode == KeyInterop.VirtualKeyFromKey(Key.T))
			{
				// Application.Current.Dispatcher.Invoke(() =>
				// {
				// 	var mainWindow = Application.Current.MainWindow as MainWindow;
				// 	if (mainWindow != null)
				// 	{
				// 		mainWindow.Topmost = true;
				// 		mainWindow.Activate();
				// 	}
				// });
				// Topmost = true;
				// Activate();
				// BringIntoView();
				// ShowHintWindow();
				// Dispatcher.InvokeAsync(ShowHintWindow);
				Application.Current.Dispatcher.Invoke(ShowHintWindow);
			}
		}
		return CallNextHookEx(_hookID, nCode, wParam, lParam);
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		using (var curProcess = Process.GetCurrentProcess())
		using (var curModule = curProcess.MainModule)
		{
			return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
		}
	}

	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);
}