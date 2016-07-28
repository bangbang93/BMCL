namespace BMCLV2.Plugin
{
    public interface IBmclPlugin
    {
        PluginType GetPluginType();
    }

    public enum PluginType
    {
        Normal,
        MainWindowTab,
        Auth,
        Mirror
    }
}
