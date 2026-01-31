using System.Windows;
using ConquerKey.Windows;
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
		// Create and configure plugin manager
		var pluginManager = new PluginManager();
		pluginManager.LoadPlugins();

		services.AddSingleton(pluginManager);
		services.AddSingleton<MainWindow>();
		services.AddSingleton<IGlobalKeyListener, GlobalKeyListener>();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		var globalKeyListener = _serviceProvider.GetRequiredService<IGlobalKeyListener>();
		globalKeyListener.StartListening();

		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		if (_serviceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}

		base.OnExit(e);
	}
}