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
	private readonly AutomationElement _activeWindow;
	private readonly AutomationElementCollection _interactableElements;
	private readonly TextBox _hintTextBox;
	private bool _isClosing;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	public ActionWindow(IActionHandler actionHandler)
	{
		_actionHandler = actionHandler;

		var foregroundWindow = GetForegroundWindow();
		_activeWindow = AutomationElement.FromHandle(foregroundWindow);
		_interactableElements = actionHandler.FindInteractableElements(_activeWindow);

		Activated += ActionWindow_Activated;
		Deactivated += ActionWindow_Deactivated;
		KeyDown += ActionWindow_KeyDown;

		ConfigureWindow();
		AddHintLabels();
		_hintTextBox = AddHintTextBox();
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
			var boundingRect = clickableElement.Current.BoundingRectangle;

			// Skip elements with invalid bounding rectangles
			if (boundingRect.IsEmpty ||
					double.IsNaN(boundingRect.X) || double.IsNaN(boundingRect.Y) ||
					double.IsInfinity(boundingRect.X) || double.IsInfinity(boundingRect.Y))
			{
				return;
			}

			var textBlock = new TextBlock
			{
				Text = index.ToString(),
				Foreground = Brushes.Black,
				FontSize = 12,
				Background = Brushes.Aqua,
				Margin = new Thickness(0),
				Padding = new Thickness(0),
			};

			var x = PixelToDeviceIndependentUnit(this, boundingRect.X - _activeWindow.Current.BoundingRectangle.X, false);
			var y = PixelToDeviceIndependentUnit(this, boundingRect.Y - _activeWindow.Current.BoundingRectangle.Y, true);

			// Skip if calculated positions are invalid
			if (double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y))
			{
				return;
			}

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

	private TextBox AddHintTextBox()
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

		var hintTextBox = new TextBox
		{
			FontSize = 12,
			Width = 200,
			Height = 30,
			Margin = new Thickness(10),
			Padding = new Thickness(5),
			Background = Brushes.LightGray,
			Foreground = Brushes.Black,
		};
		Canvas.SetLeft(hintTextBox, 80); // X-coordinate
		Canvas.SetTop(hintTextBox, Height - 30); // Y-coordinate

		if (Content is Canvas canvas)
		{
			canvas.Children.Add(hintLabel);
			canvas.Children.Add(hintTextBox);
		}

		hintTextBox.PreviewTextInput += (s, evt) =>
		{
			evt.Handled = !int.TryParse(evt.Text, out _);
		};

		hintTextBox.KeyDown += (s, evt) =>
		{
			if (evt.Key != Key.Enter) return;

			_isClosing = true;
			Close();

			// Handle the Enter key press here
			var uiElement = _interactableElements[int.Parse(hintTextBox.Text)];
			if (uiElement != null)
			{
				_actionHandler.Interact(_activeWindow, uiElement);
			}

			evt.Handled = true; // Mark the event as handled if necessary
		};

		return hintTextBox;
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

	private void ActionWindow_Activated(object? sender, EventArgs e)
	{
		_hintTextBox.Focus();
	}

	private void ActionWindow_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Escape) return;

		_isClosing = true;
		Close();
		e.Handled = true; // Mark the event as handled
	}

	private void ActionWindow_Deactivated(object? sender, EventArgs e)
	{
		if (_isClosing) return;

		_isClosing = true;
		Close();
	}
}