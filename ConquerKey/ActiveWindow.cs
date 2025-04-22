using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace ConquerKey;

public class ActiveWindow : IActiveWindow
{
	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	public AutomationElement Current
	{
		get
		{
			var foregroundWindow = GetForegroundWindow();
			return AutomationElement.FromHandle(foregroundWindow);
		}
	}
}