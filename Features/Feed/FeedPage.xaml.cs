using System.Windows.Controls;

namespace NtfyDesktop.Features.Feed;

public partial class FeedPage : Page
{
    public FeedPage(FeedViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
