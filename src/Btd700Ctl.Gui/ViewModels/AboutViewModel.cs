using System.Diagnostics;
using System.Windows.Input;

namespace Btd700Ctl.Gui.ViewModels;

public class AboutViewModel
{
    public ICommand ShowGithubCommand { get; }

    public AboutViewModel()
    {
        ShowGithubCommand = new Command(ShowGithub);
    }

    private void ShowGithub()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/sam-k0/BTD700-Gui",
            UseShellExecute = true
        });
    }
}