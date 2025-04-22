using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;

namespace ConquerKey;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;

	public App()
	{
		var serviceCollection = new ServiceCollection();
		ConfigureServices(serviceCollection);
		_serviceProvider = serviceCollection.BuildServiceProvider();
	}

	private void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<MainWindow>();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}
}