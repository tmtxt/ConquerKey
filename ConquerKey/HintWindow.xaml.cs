using System.Windows;
using System.Windows.Automation;

namespace ConquerKey;

public partial class HintWindow : Window
{
	private readonly AutomationElement _activeWindow;

	public HintWindow(AutomationElement activeWindow)
	{
		_activeWindow = activeWindow;

		InitializeComponent();
		Loaded += HintWindow_Loaded;
		Activated += HintWindow_Activated;

		Topmost = true;
		SetPositionFromActiveWindow();
	}

	private void SetPositionFromActiveWindow()
	{
		Width = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Width,
			false);
		Height = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Height + 30,
			true); // + 30 for textbox to input the hint number
		Left = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Left, false);
		Top = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Top, true);
	}

	private void HintWindow_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void HintWindow_Activated(object? sender, EventArgs e)
	{
		MyTextBox.Focus();
	}
}