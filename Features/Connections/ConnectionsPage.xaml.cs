using System.Windows.Controls;

namespace NtfyDesktop.Features.Connections;

public partial class ConnectionsPage : Page
{
    public ConnectionsPage(ConnectionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
