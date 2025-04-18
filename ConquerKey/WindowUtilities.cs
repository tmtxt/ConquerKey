using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;
using System.Windows.Media;

namespace ConquerKey;

public static class WindowUtilities {

	/// <summary>
	/// Activates a WPF window even if the window is activated on a separate thread
	/// https://weblog.west-wind.com/posts/2020/Oct/12/Window-Activation-Headaches-in-WPF
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

	public static AutomationElement GetActiveWindow()
	{
		IntPtr foregroundWindow = User32Interop.GetForegroundWindow();
		return AutomationElement.FromHandle(foregroundWindow);
	}

	public static double PixelToDeviceIndependentUnit(Window window, double pixel, bool isVertical)
	{
		const double defaultDpi = 96.0;
		var pixelPerInch = isVertical ? VisualTreeHelper.GetDpi(window).PixelsPerInchY : VisualTreeHelper.GetDpi(window).PixelsPerInchX;
		return pixel * (defaultDpi / pixelPerInch);
	}

	/// <summary>
	/// Need to update this method. Currently, it doesn't find all the elements on CW1 UI
	/// </summary>
	/// <param name="rootElement"></param>
	/// <returns></returns>
	public static AutomationElementCollection FindClickableElements(AutomationElement rootElement)
	{
		// Define a condition to find elements that are clickable
		var clickableCondition = new OrCondition(
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
			new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, true)
		);
		var visibleCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
		var controlElementCondition = new OrCondition(
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Hyperlink),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.CheckBox),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem),
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem)
			);
		var finalCondition = new AndCondition(
			controlElementCondition,
			visibleCondition
		);


		// Find all matching elements
		var clickableElements = rootElement.FindAll(TreeScope.Descendants, finalCondition);
		var count = clickableElements.Count;

		return clickableElements;
	}

	/// <summary>
	/// Move the mouse cursor to the specified coordinates and perform a left mouse click.
	/// </summary>
	public static void SendMouseClick(int x, int y)
	{
		User32Interop.SetCursorPos(x, y);

		// Simulate a left mouse button click
		var input = new User32Interop.INPUT[2];

		// Mouse down
		input[0] = new User32Interop.INPUT
		{
			type = User32Interop.INPUT_MOUSE,
			U = new User32Interop.InputUnion
			{
				mi = new User32Interop.MOUSEINPUT
				{
					dwFlags = User32Interop.MOUSEEVENTF_LEFTDOWN,
				}
			}
		};

		// Mouse up
		input[1] = new User32Interop.INPUT
		{
			type = User32Interop.INPUT_MOUSE,
			U = new User32Interop.InputUnion
			{
				mi = new User32Interop.MOUSEINPUT
				{
					dwFlags = User32Interop.MOUSEEVENTF_LEFTUP,
				}
			}
		};

		User32Interop.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(User32Interop.INPUT)));
	}
}