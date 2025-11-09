# Fluens Browser

A modern, lightweight web browser built with WinUI 3 for Windows, designed for speed and simplicity.

## Features

- **Tabbed Browsing**: Create, close, and manage multiple tabs with full persistence across sessions.
- **History Tracking**: Automatically track and display browsing history with visit counts and dates.
- **Bookmarks**: Save and organize your favorite websites with folders and notes.
- **Startup Options**: Customize what happens when the browser starts, including restoring previous tabs or opening a new tab.
- **Keyboard Shortcuts**: Efficient navigation with shortcuts like Ctrl+T (new tab), Ctrl+W (close tab), and F5 (refresh).
- **Local Storage**: Uses SQLite for fast, local data storage of tabs, history, and bookmarks.

## Requirements

- Windows 10 version 1903 (19H1) or later
- .NET 10 Runtime
- Windows App SDK 1.8 or later

## Installation

1. Clone the repository:
   ```
   git clone https://github.com/SamLS42/Fluens-Browser.git
   cd Fluens-Browser
   ```

2. Open the solution in Visual Studio 2026 or later.

3. Restore NuGet packages and build the solution.

4. Run the `Fluens.UI` project.

## Usage

- Launch the application to start browsing.
- Use the tab bar to manage tabs.
- Access settings via the settings view to configure startup behavior and other options.
- View history and bookmarks in their respective sections.

## Architecture

The project is structured into three main components:

- **Fluens.Data**: Handles data models and Entity Framework Core integration with SQLite.
- **Fluens.AppCore**: Contains core services, view models, and business logic using ReactiveUI.
- **Fluens.UI**: The WinUI 3 user interface layer.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.