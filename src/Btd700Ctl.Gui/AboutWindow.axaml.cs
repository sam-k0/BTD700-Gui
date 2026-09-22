using Avalonia.Controls;

namespace Btd700Ctl.Gui;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = new ViewModels.AboutViewModel();
    }
}
