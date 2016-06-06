namespace BMCLV2.Plugin
{
    public interface IBmclPlugin
    {
        PluginType GetType();
    }

    public enum PluginType
    {
        Normal,
        MainWindowTab,
        Auth,
        Mirror
    }
}
