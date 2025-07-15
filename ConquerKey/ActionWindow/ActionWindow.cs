using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;

namespace ConquerKey.ActionWindow;

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
	}
}