using System;
using Avalonia.Controls;
using Btd700Ctl.Gui.ViewModels;

namespace Btd700Ctl.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Opened += MainWindow_Opened;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        if(DataContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
