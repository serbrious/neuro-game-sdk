using NeuroSdk.Resources;

namespace NeuroSdk
{
    partial class NeuroSdkSetup
    {
        static NeuroSdkSetup()
        {
            ResourceManager.InjectAssemblies();
        }
    }
}
