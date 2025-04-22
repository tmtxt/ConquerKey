using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace ConquerKey;

public class ActiveWindow : IActiveWindow
{
	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	public ActiveWindow()
	{
		var foregroundWindow = GetForegroundWindow();
		Instance = AutomationElement.FromHandle(foregroundWindow);
	}

	public AutomationElement Instance { get; }
}