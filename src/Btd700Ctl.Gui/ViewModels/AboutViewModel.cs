using System.Diagnostics;
using System.Windows.Input;
using System.Reflection;
using Tmds.DBus.Protocol;

namespace Btd700Ctl.Gui.ViewModels;

public class AboutViewModel
{
    public ICommand ShowGithubCommand { get; }

    public string Version { get; } = "Version " + 
                    Assembly.GetExecutingAssembly().
                    GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unpublished";

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