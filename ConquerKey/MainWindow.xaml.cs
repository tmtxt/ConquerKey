using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Condition = System.Windows.Automation.Condition;

namespace ConquerKey;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private static IntPtr _hookID = IntPtr.Zero;
	private TextBox _textBox;

	public MainWindow()
	{
		InitializeComponent();
		Loaded += MainWindow_LoadedSimple;
		Closed += MainWindow_Closed;
	}

	private void MainWindow_Closed(object? sender, EventArgs e)
	{
		UnhookWindowsHookEx(_hookID);
	}

	private void MainWindow_LoadedSimple(object sender, RoutedEventArgs e)
	{
		_hookID = SetHook(HookCallback);
	}

	private void ShowHintWindow()
	{
		var hintWindow = new HintWindow();
		hintWindow.Topmost = true;
		hintWindow.Show();
		// hintWindow.Activate();
		WindowUtilities.ActivateWindow(hintWindow);
	}

	private void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		string windowTitle =
			"CargoWise Next - ediProd - Branch: Sydney Aust Branch - Company: WiseTech Global (Australia) Pty Ltd - Department: Development";
		var rootElement = GetRootElementByWindowTitle(windowTitle);
		rootElement.SetFocus();

		// Add your logic here
		Width = rootElement.Current.BoundingRectangle.Width * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchX);
		// Width = rootElement.Current.BoundingRectangle.Width;
		Height = (rootElement.Current.BoundingRectangle.Height + 30) * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchY);
		// Left = rootElement.Current.BoundingRectangle.X;
		// Top = rootElement.Current.BoundingRectangle.Y;
		Left = rootElement.Current.BoundingRectangle.X * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchX);
		Top = rootElement.Current.BoundingRectangle.Y * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchY);

		AutomationElementCollection clickableElements = FindClickableElements(rootElement);
		for (var index = 0; index < clickableElements.Count; index++)
		{
			var clickableElement = clickableElements[index];
			AddHintText(clickableElement, index, rootElement);
		}

		_textBox = new TextBox
		{
			//Text = "",
			Width = 200,
			Height = 30,
			Margin = new Thickness(10),
			Padding = new Thickness(5),
			Background = Brushes.LightGray,
			Foreground = Brushes.Black
		};
		Canvas.SetLeft(_textBox, 0); // X-coordinate
		Canvas.SetTop(_textBox, Height - 30); // Y-coordinate
		// Add the TextBlock to the Canvas
		if (Content is Canvas canvas)
		{
			canvas.Children.Add(_textBox);
		}

		_textBox.PreviewTextInput += (s, evt) =>
		{
			evt.Handled = !int.TryParse(evt.Text, out _);
		};

		_textBox.KeyDown += (s, evt) =>
		{
			if (evt.Key != Key.Enter) return;

			// Handle the Enter key press here
			// MessageBox.Show($"You pressed Enter. Text: {textBox.Text}");
			var clickableElement = clickableElements[int.Parse(_textBox.Text)];
			if (clickableElement.TryGetCurrentPattern(InvokePattern.Pattern, out object pattern))
			{
				((InvokePattern)pattern).Invoke(); // Perform the click
			}

			evt.Handled = true; // Mark the event as handled if necessary
		};

		// Activate();
		_textBox.Focus();

		_hookID = SetHook(HookCallback);
	}

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
		{
			int vkCode = Marshal.ReadInt32(lParam);
			if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
				(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) &&
				(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) &&
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
				Dispatcher.InvokeAsync(ShowHintWindow);
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

	private void AddHintText(AutomationElement clickableElement, int index, AutomationElement rootElement)
	{
		// Add your logic here
		var textBlock = new TextBlock
		{
			Text = index.ToString(),
			Foreground = Brushes.Black,
			FontSize = 12,
			Background = Brushes.Aqua,
			Margin = new Thickness(0),
			Padding = new Thickness(0),
		};

		var x = clickableElement.Current.BoundingRectangle.X - rootElement.Current.BoundingRectangle.X;
		var y = clickableElement.Current.BoundingRectangle.Y - rootElement.Current.BoundingRectangle.Y;

		// Set the absolute position
		Canvas.SetLeft(textBlock, x); // X-coordinate
		Canvas.SetTop(textBlock, y); // Y-coordinate

		// Add the TextBlock to the Canvas
		if (Content is Canvas canvas)
		{
			canvas.Children.Add(textBlock);
		}
	}

	static AutomationElement GetRootElementByWindowTitle(string windowTitle)
	{
		return AutomationElement.RootElement.FindFirst(
			TreeScope.Children,
			new PropertyCondition(AutomationElement.NameProperty, windowTitle));
	}

	/// <summary>
	/// Need to update this method. Currently, it doesn't find all the elements on CW1 UI
	/// </summary>
	/// <param name="rootElement"></param>
	/// <returns></returns>
	private AutomationElementCollection FindClickableElements(AutomationElement rootElement)
	{
		// Define a condition to find elements that are clickable
		var clickableCondition = new OrCondition(
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
			new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, true)
		);
		var visibleCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
		var finalCondition = new AndCondition(
			clickableCondition,
			visibleCondition
			);
		// var clickableCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);

		// Find all matching elements
		var clickableElements = rootElement.FindAll(TreeScope.Descendants, finalCondition);

		return clickableElements;
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