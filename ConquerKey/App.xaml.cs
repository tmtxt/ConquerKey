using System.Windows;
using ConquerKey.Configuration;
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
		var configManager = new ConfigManager();

		var actionManager = new ActionManager(configManager);
		actionManager.LoadActions();

		var elementFinderManager = new ElementFinderManager();
		elementFinderManager.LoadFinders();

		services.AddSingleton(configManager);
		services.AddSingleton(actionManager);
		services.AddSingleton(elementFinderManager);
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