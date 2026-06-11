using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>
/// Shows a modal dialog window for an <see cref="IDialogViewModel"/>: resolves the main window as the owner,
/// relays the view model's close request to the window, and returns the dialog result — so each dialog service
/// is free of duplicated window plumbing.
/// </summary>
public static class DialogHost
{
    /// <summary>
    /// Opens <typeparamref name="TWindow"/> hosting <paramref name="viewModel"/> as a modal dialog over the
    /// main window, returning its boolean result (or <see langword="false"/> if there is no main window).
    /// </summary>
    public static async Task<bool> ShowAsync<TWindow>(IDialogViewModel viewModel)
        where TWindow : Window, new()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } owner })
        {
            return false;
        }

        var window = new TWindow { DataContext = viewModel };

        // The view model never touches the Window; it asks to close with a result, and we relay it.
        viewModel.CloseRequested += result => window.Close(result);

        return await window.ShowDialog<bool>(owner);
    }
}
