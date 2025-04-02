
namespace AttachUsbFromHost;

class MainWindowModel
{
    public string Title
    {
        get
        {
            return $"{Program.scriptConfig.SSH_NAME}@{Program.scriptConfig.SSH_IP} (virtuaalarvuti: {Program.scriptConfig.VM_NAME})";
        }
        set {}
    } 

    public MainWindowModel()
    {

    }

}
