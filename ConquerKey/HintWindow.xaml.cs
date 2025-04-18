using System.Windows;

namespace ConquerKey;

public partial class HintWindow : Window
{
	public HintWindow()
	{
		InitializeComponent();
		Activated += HintWindow_Activated;
	}

	private void HintWindow_Activated(object? sender, EventArgs e)
	{
		MyTextBox.Focus();
	}
}