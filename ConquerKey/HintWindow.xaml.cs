using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConquerKey;

public partial class HintWindow : Window
{
	private readonly AutomationElement _activeWindow;
	private TextBox _hintTextBox;

	public HintWindow(AutomationElement activeWindow)
	{
		_activeWindow = activeWindow;

		InitializeComponent();
		// Loaded += HintWindow_Loaded;
		Activated += HintWindow_Activated;

		Topmost = true;
		SetPosition();
		AddHintLabels();
		AddHintTextBox();
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
	}

	private void AddHintLabels()
	{
		var clickableElements = WindowUtilities.FindClickableElements(_activeWindow);
		for (var index = 0; index < clickableElements.Count; index++)
		{
			var clickableElement = clickableElements[index];
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

			var x = clickableElement.Current.BoundingRectangle.X - _activeWindow.Current.BoundingRectangle.X;
			var y = clickableElement.Current.BoundingRectangle.Y - _activeWindow.Current.BoundingRectangle.Y;

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