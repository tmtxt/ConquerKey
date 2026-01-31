using System.IO;
using System.Reflection;
using System.Windows.Input;
using ConquerKey.ActionHandlers;

namespace ConquerKey;

/// <summary>
/// Manages plugin discovery and registration.
/// Scans the entry assembly for built-in plugins and loads external plugins from ~/.conquerkey/plugins.
/// When key binding conflicts occur, the last-registered plugin wins (external plugins override built-in).
/// </summary>
public class PluginManager
{
  private readonly Dictionary<PluginKeyBinding, IConquerKeyPlugin> _plugins = new();

  /// <summary>
  /// Gets all registered plugins.
  /// </summary>
  public IReadOnlyCollection<IConquerKeyPlugin> Plugins => _plugins.Values;

  /// <summary>
  /// Gets all registered key bindings and their associated plugins.
  /// </summary>
  public IReadOnlyDictionary<PluginKeyBinding, IConquerKeyPlugin> PluginsByKeyBinding => _plugins;

  /// <summary>
  /// Discovers and loads all plugins from the entry assembly and the external plugins folder.
  /// </summary>
  public void LoadPlugins()
  {
    // First, load built-in plugins from the entry assembly
    LoadPluginsFromAssembly(Assembly.GetEntryAssembly());

    // Then, load external plugins from ~/.conquerkey/plugins (last-registered wins)
    LoadExternalPlugins();
  }

  /// <summary>
  /// Tries to get a plugin by its key binding.
  /// </summary>
  /// <param name="modifiers">The modifier keys pressed.</param>
  /// <param name="key">The trigger key pressed.</param>
  /// <param name="plugin">The matching plugin, if found.</param>
  /// <returns>True if a plugin was found for the key binding; otherwise, false.</returns>
  public bool TryGetPlugin(ModifierKeys modifiers, Key key, out IConquerKeyPlugin? plugin)
  {
    var keyBinding = new PluginKeyBinding(modifiers, key);
    return _plugins.TryGetValue(keyBinding, out plugin);
  }

  /// <summary>
  /// Registers a plugin. If a plugin with the same key binding already exists, it will be replaced.
  /// </summary>
  /// <param name="plugin">The plugin to register.</param>
  public void RegisterPlugin(IConquerKeyPlugin plugin)
  {
    _plugins[plugin.KeyBinding] = plugin;
  }

  private void LoadPluginsFromAssembly(Assembly? assembly)
  {
    if (assembly == null) return;

    try
    {
      var pluginTypes = assembly.GetTypes()
        .Where(t => typeof(IConquerKeyPlugin).IsAssignableFrom(t)
          && t.IsClass
          && !t.IsAbstract);

      foreach (var pluginType in pluginTypes)
      {
        try
        {
          if (Activator.CreateInstance(pluginType) is IConquerKeyPlugin plugin)
          {
            RegisterPlugin(plugin);
          }
        }
        catch (Exception ex)
        {
          // Log or handle plugin instantiation errors
          System.Diagnostics.Debug.WriteLine($"Failed to create plugin instance of type {pluginType.Name}: {ex.Message}");
        }
      }
    }
    catch (ReflectionTypeLoadException ex)
    {
      // Handle cases where some types couldn't be loaded
      System.Diagnostics.Debug.WriteLine($"Failed to load some types from assembly {assembly.FullName}: {ex.Message}");
    }
  }

  private void LoadExternalPlugins()
  {
    var pluginsFolder = GetPluginsFolder();

    if (!Directory.Exists(pluginsFolder))
    {
      // Create the plugins folder if it doesn't exist
      try
      {
        Directory.CreateDirectory(pluginsFolder);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Failed to create plugins folder: {ex.Message}");
      }
      return;
    }

    var dllFiles = Directory.GetFiles(pluginsFolder, "*.dll");

    foreach (var dllFile in dllFiles)
    {
      try
      {
        // Load the assembly into the default context
        var assembly = Assembly.LoadFrom(dllFile);
        LoadPluginsFromAssembly(assembly);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Failed to load plugin assembly {dllFile}: {ex.Message}");
      }
    }
  }

  private static string GetPluginsFolder()
  {
    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    return Path.Combine(userProfile, ".conquerkey", "plugins");
  }
}
