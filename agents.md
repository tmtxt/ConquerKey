# ConquerKey - AI Agent Instructions

## Project Overview

ConquerKey is a proof-of-concept (POC) Windows utility application that enables keyboard-driven control of Windows applications. The app allows users to interact with UI elements (buttons, links, inputs, etc.) without using a mouse - similar to Vimium for browsers but for native Windows applications.

## How It Works

1. **Global Key Listening**: When the app starts, it registers a global keyboard hook using `GlobalKeyListener` to capture key presses system-wide
2. **Trigger Activation**: When the user presses the trigger key binding (`Ctrl + Windows + T`), the app activates
3. **UI Element Discovery**: The `ActionWindow` appears on top of the currently active window and uses Windows UI Automation to find all interactable/clickable elements
4. **Hint Display**: Each discovered element gets a numbered hint label (`HintLabel`) overlaid on it
5. **User Input**: A `HintTextBox` appears for the user to type the number of the element they want to interact with
6. **Action Execution**: After pressing Enter, the app simulates a click on the selected element using the appropriate `IActionHandler`

## Technology Stack

- **.NET 9.0** with **WPF** (Windows Presentation Foundation)
- **Windows UI Automation** (`System.Windows.Automation`) for discovering UI elements
- **Win32 Interop** (`user32.dll`, `kernel32.dll`) for:
  - Global keyboard hooks
  - Mouse click simulation
  - Window activation
- **Microsoft.Extensions.DependencyInjection** for IoC/DI

## Project Structure

```
ConquerKey/
├── App.xaml.cs              # Application entry point, DI configuration
├── GlobalKeyListener.cs     # Global keyboard hook implementation
├── IGlobalKeyListener.cs    # Interface for key listener
├── WindowUtilities.cs       # Helper methods for window/UI operations
├── User32Interop.cs         # Win32 API interop declarations
├── ActionHandlers/
│   ├── Actions.cs           # Action type constants
│   ├── IActionHandler.cs    # Interface for action handlers
│   └── ClickActionHandler.cs # Click action implementation
└── Windows/
    ├── MainWindow.xaml(.cs) # Main application window
    ├── ActionWindow.cs      # Overlay window with hints
    └── HintWindow.xaml(.cs) # Alternative hint window implementation
```

## Key Components

### GlobalKeyListener

- Implements a low-level keyboard hook using `SetWindowsHookEx` with `WH_KEYBOARD_LL`
- Listens for the trigger combination: `Ctrl + Win + T`
- Creates and shows the `ActionWindow` when triggered

### ActionWindow

- Transparent overlay window positioned exactly over the active window
- Uses `AutomationElement.FromHandle()` to get the active window
- Calls `IActionHandler.FindInteractableElements()` to discover clickable elements
- Renders numbered hint labels on each element's position
- Contains a text box for entering the element number
- Closes on Escape key or when losing focus

### IActionHandler / ClickActionHandler

- Strategy pattern for different interaction types
- `FindInteractableElements()`: Uses UI Automation conditions to find visible elements of various control types (Button, Hyperlink, Tab, CheckBox, etc.)
- `Interact()`: Simulates a mouse click at the element's center position

### WindowUtilities

- `ActivateWindow()`: Properly activates a window across thread boundaries
- `PixelToDeviceIndependentUnit()`: Handles DPI scaling for proper positioning
- `SendMouseClick()`: Simulates mouse clicks using `SendInput`
- `FindClickableElements()`: Legacy method for finding UI elements

## Development Notes

### Adding New Action Types

1. Create a new class implementing `IActionHandler`
2. Register it in `App.ConfigureServices()` with a keyed service
3. Add the corresponding action key in `Actions.cs`

### Trigger Key Binding

The current trigger is hardcoded in `GlobalKeyListener.HookCallback()`:

```csharp
if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
    (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin)) &&
    vkCode == KeyInterop.VirtualKeyFromKey(Key.T))
```

### Known Limitations (POC Status)

- Some UI elements may not be discovered depending on the application's accessibility implementation
- The hint numbering can become crowded with many elements
- No configuration UI for changing the trigger key binding
- Single action type (click) currently implemented

## Building and Running

```bash
# Build the solution
dotnet build ConquerKey.sln

# Run the application
dotnet run --project ConquerKey/ConquerKey.csproj
```

## Testing

```bash
# Run unit tests
dotnet test ConquerKey.Test/ConquerKey.Test.csproj
```
