#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public bool SetAsSystemProxy(bool state, IPAddress? ip, int port)
    {
        var shellContent = new StringBuilder();
        if (state)
        {
            var hasIpAndProt = ip != null && port >= 0;
            shellContent.AppendLine("gsettings set org.gnome.system.proxy mode 'manual'");
            if (hasIpAndProt)
            {
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.http host '{ip}'");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.http port {port}");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.https host '{ip}'");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.https port {port}");
            }
        }
        else
        {
            shellContent.AppendLine($"gsettings set org.gnome.system.proxy mode 'none'");
        }
        IPlatformService @this = this;
        @this.RunShell(shellContent.ToString(), false);
        return true;
    }
}
#endif