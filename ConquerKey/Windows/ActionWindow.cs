using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using ConquerKey.ActionHandlers;

namespace ConquerKey.Windows;

public class ActionWindow : Window
{
	private readonly IActionHandler _actionHandler;
	private AutomationElement _activeWindow;
	private AutomationElementCollection _interactableElements;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();
	
	public ActionWindow(IActionHandler actionHandler)
	{
		_actionHandler = actionHandler;
		
		var foregroundWindow = GetForegroundWindow();
		_activeWindow = AutomationElement.FromHandle(foregroundWindow);
		
		_interactableElements = actionHandler.FindInteractableElements(_activeWindow);

		Topmost = true;
		Width = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Width, false);
		Height = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Height + 30,
			true); // + 30 for textbox to input the hint number
		Left = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Left, false);
		Top = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Top, true);
	}
}