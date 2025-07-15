using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using ConquerKey.ActionHandlers;
using static ConquerKey.WindowUtilities;

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
		
		ConfigureWindow();
		AddHintLabels();
	}

	private void AddHintLabels()
	{
		for (var index = 0; index < _interactableElements.Count; index++)
		{
			var clickableElement = _interactableElements[index];
			AddHintLabel(clickableElement, index);
		}
		
		void AddHintLabel(AutomationElement clickableElement, int index)
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

			var x = PixelToDeviceIndependentUnit(this, clickableElement.Current.BoundingRectangle.X - _activeWindow.Current.BoundingRectangle.X, false);
			var y = PixelToDeviceIndependentUnit(this, clickableElement.Current.BoundingRectangle.Y - _activeWindow.Current.BoundingRectangle.Y, true);

			// Set the absolute position
			Canvas.SetLeft(textBlock, x); // X-coordinate
			Canvas.SetTop(textBlock, y); // Y-coordinate

			// Add the TextBlock to the Canvas
			if (Content is Canvas canvas)
			{
				canvas.Children.Add(textBlock);
			}
		}
	}

	private void ConfigureWindow()
	{
		Topmost = true;
		Width = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Width, false);
		// + 30 for textbox to input the hint number
		Height = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Height + 30, true);
		Left = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Left, false);
		Top = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Top, true);
		WindowStyle = WindowStyle.None;
		AllowsTransparency = true;
		Background = Brushes.Transparent;
		
		var canvas = new Canvas();

		var border = new Border
		{
			BorderBrush = Brushes.White,
			BorderThickness = new Thickness(10),
			CornerRadius = new CornerRadius(5)
		};
		canvas.Children.Add(border);
		Content = canvas;
	}
}