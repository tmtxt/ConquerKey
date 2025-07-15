using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using Brushes = System.Windows.Media.Brushes;
using Label = System.Windows.Controls.Label;
using TextBox = System.Windows.Controls.TextBox;

namespace ConquerKey.Windows;

public partial class HintWindow : Window
{
	private readonly AutomationElement _activeWindow;
	private readonly AutomationElementCollection _clickableElements;
	private TextBox _hintTextBox;

	public HintWindow(AutomationElement activeWindow)
	{
		_activeWindow = activeWindow;
		_clickableElements = WindowUtilities.FindClickableElements(_activeWindow);

		InitializeComponent();
		// Loaded += HintWindow_Loaded;
		Activated += HintWindow_Activated;
		KeyDown += HintWindow_KeyDown;

		Topmost = true;
		SetPosition();
		AddHintLabels();
		AddHintTextBox();
	}

	private void HintWindow_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Escape) return;

		Close();
		e.Handled = true; // Mark the event as handled
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
			//Text = "",
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
			// MessageBox.Show($"You pressed Enter. Text: {textBox.Text}");
			var clickableElement = _clickableElements[int.Parse(_hintTextBox.Text)];
			if (clickableElement.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
			{
				((InvokePattern)pattern).Invoke(); // Perform the click
			}
			else
			{
				var rect = clickableElement.Current.BoundingRectangle;
				var x = (int)(rect.X + rect.Width / 2);
				var y = (int)(rect.Y + rect.Height / 2);

				WindowUtilities.SendMouseClick(x, y);
			}

			evt.Handled = true; // Mark the event as handled if necessary
		};
	}

	private void AddHintLabels()
	{
		for (var index = 0; index < _clickableElements.Count; index++)
		{
			var clickableElement = _clickableElements[index];
			AddHintText(clickableElement, index);
		}

		return;

		void AddHintText(AutomationElement clickableElement, int index)
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

			var x = WindowUtilities.PixelToDeviceIndependentUnit(this, clickableElement.Current.BoundingRectangle.X - _activeWindow.Current.BoundingRectangle.X, false);
			var y = WindowUtilities.PixelToDeviceIndependentUnit(this, clickableElement.Current.BoundingRectangle.Y - _activeWindow.Current.BoundingRectangle.Y, true);

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

	private void SetPosition()
	{
		Width = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Width,
			false);
		Height = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Height + 30,
			true); // + 30 for textbox to input the hint number
		Left = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Left, false);
		Top = WindowUtilities.PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Top, true);
	}

	private void HintWindow_Activated(object? sender, EventArgs e)
	{
		_hintTextBox.Focus();
	}
}