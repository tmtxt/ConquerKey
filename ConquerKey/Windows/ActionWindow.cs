using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ConquerKey.ActionHandlers;
using static ConquerKey.WindowUtilities;

namespace ConquerKey.Windows;

public class ActionWindow : Window
{
	private readonly IActionHandler _actionHandler;
	private AutomationElement _activeWindow;
	private AutomationElementCollection _interactableElements;
	private TextBox _hintTextBox;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();
	
	public ActionWindow(IActionHandler actionHandler)
	{
		_actionHandler = actionHandler;
		
		var foregroundWindow = GetForegroundWindow();
		_activeWindow = AutomationElement.FromHandle(foregroundWindow);
		_interactableElements = actionHandler.FindInteractableElements(_activeWindow);
		
		Activated += HintWindow_Activated;
		
		ConfigureWindow();
		AddHintLabels();
		AddHintTextBox();
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
	
	private void AddHintTextBox()
	{
		var hintLabel = new Label
		{
			Content = "Enter Hint:",
			Margin = new Thickness(10),
			Foreground = Brushes.Black,
			FontSize = 12,
			Background = Brushes.Chartreuse,
		};
		Canvas.SetLeft(hintLabel, 0); // X-coordinate
		Canvas.SetTop(hintLabel, Height - 30); // Y-coordinate

		_hintTextBox = new TextBox
		{
			FontSize = 12,
			Width = 200,
			Height = 30,
			Margin = new Thickness(10),
			Padding = new Thickness(5),
			Background = Brushes.LightGray,
			Foreground = Brushes.Black,
		};
		Canvas.SetLeft(_hintTextBox, 80); // X-coordinate
		Canvas.SetTop(_hintTextBox, Height - 30); // Y-coordinate

		if (Content is Canvas canvas)
		{
			canvas.Children.Add(hintLabel);
			canvas.Children.Add(_hintTextBox);
		}

		_hintTextBox.PreviewTextInput += (s, evt) =>
		{
			evt.Handled = !int.TryParse(evt.Text, out _);
		};

		_hintTextBox.KeyDown += (s, evt) =>
		{
			if (evt.Key != Key.Enter) return;

			Close();

			// Handle the Enter key press here
			var uiElement = _interactableElements[int.Parse(_hintTextBox.Text)];
			if (uiElement != null)
			{
				_actionHandler.Interact(_activeWindow, uiElement);
			}

			evt.Handled = true; // Mark the event as handled if necessary
		};
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
	
	private void HintWindow_Activated(object? sender, EventArgs e)
	{
		_hintTextBox.Focus();
	}
}