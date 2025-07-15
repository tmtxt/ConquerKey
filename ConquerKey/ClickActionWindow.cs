using System.Windows;

namespace ConquerKey;

public class ClickActionWindow : Window
{
	private readonly ICapturedWindow _capturedWindow;

	public ClickActionWindow(ICapturedWindow capturedWindow)
	{
		_capturedWindow = capturedWindow;
	}
}