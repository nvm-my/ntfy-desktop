using System.Windows;
using System.Windows.Controls;

namespace NtfyDesktop.Features.Settings;

public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _vm;
    private bool _suspendTokenSync;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _vm = viewModel;

        // Re-read settings every time the page is shown so the PasswordBox
        // repopulates with the stored token even if the singleton VM's state
        // drifted (page is transient, VM is singleton). Snapshot reset happens
        // inside Load(), so IsDirty starts at false on every entry.
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _vm.Load();
        SyncPasswordBoxFromViewModel();
    }

    private void SyncPasswordBoxFromViewModel()
    {
        // PasswordBox.Password isn't a DependencyProperty so we can't bind it.
        // Pump the VM value in manually, suppressing the PasswordChanged echo.
        _suspendTokenSync = true;
        TokenBox.Password = _vm.AccessToken;
        _suspendTokenSync = false;
    }

    private void OnTokenChanged(object sender, RoutedEventArgs e)
    {
        if (_suspendTokenSync) return;
        // Setting the ObservableProperty triggers the VM's snapshot-based dirty
        // recompute; no separate "did the user touch it" flag needed.
        _vm.AccessToken = TokenBox.Password;
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.SaveCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Unexpected error: " + ex.Message);
        }
    }

    private void OnDiscardClicked(object sender, RoutedEventArgs e)
    {
        _vm.Load();
        SyncPasswordBoxFromViewModel();
    }
}
