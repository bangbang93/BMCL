namespace BMCLV2.Plugin
{
    interface IBmclPlugin
    {
        PluginType GetType();
    }

    enum PluginType
    {
        MainWindowTab
    }
}
