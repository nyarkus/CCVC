using CCVC;
using System.Security.Principal;
using Microsoft.Win32;
using ConsolePlayer;

class Program
{
    public static void Main(string[] args)
    {
        string source;
        if (args.Length == 0)
        {
            if (!CheckRegistry())
            {
                if (IsRunAsAdmin())
                {
                    AddToRegistry();
                    Console.WriteLine("File association successfully registered!");
                }
                else
                    Console.WriteLine("TIP: You can run ConsolePlayer.exe as admin rights to register file association");
            }
            else
            {
                if(IsRunAsAdmin())
                {
                    RemoveRegistry();
                    Console.WriteLine("File association successfully removed!");
                }
                else
                    Console.WriteLine("TIP: You can run ConsolePlayer.exe as admin rights to REMOVE file association");
            }
            Console.WriteLine("Enter a path to .ccv file:");
            source = Console.ReadLine().Trim('"');
            
        }
        else
            source = args[0].Trim('"');

        var video = CVideo.Load(source);
        CCVC.Players.ConsolePlayer.Play(video);
    }

    private static bool IsRunAsAdmin()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private const string progId = "CCVC";
    private static void AddToRegistry()
    {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\.ccv"))
        {
            key.SetValue("", progId);
        }

        var applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsolePlayer.exe");
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CCV.ico");

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{progId}"))
        {
            key.SetValue("", "Console Player");
            key.CreateSubKey("DefaultIcon").SetValue("", iconPath);
            key.CreateSubKey("shell\\open\\command").SetValue("", $"\"{applicationPath}\" \"%1\"");
        }

        IconUpdater.UpdateIcons();
    }

    private static void RemoveRegistry()
    {
        Registry.CurrentUser.DeleteSubKey($"Software\\Classes\\.ccv");
        var applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsolePlayer.exe");
        Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{progId}");

        IconUpdater.UpdateIcons();
    }

    private static bool CheckRegistry()
    {
        var key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\.ccv");
        if(key is null)
            return false;

        var applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsolePlayer.exe");

        key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{progId}");
        if (key is null)
            return false;

        return true;
    }
}