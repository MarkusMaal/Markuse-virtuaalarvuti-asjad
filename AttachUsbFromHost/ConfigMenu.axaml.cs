using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AttachUsbFromHost;

public partial class ConfigMenu : Window
{
    public ConfigMenu()
    {
        InitializeComponent();
        NameField.Text = Program.scriptConfig.SSH_NAME;
        VMField.Text = Program.scriptConfig.VM_NAME;
        IPField.Text = Program.scriptConfig.SSH_IP;
        LocationField.Text = Program.scriptConfig.SCRIPT_LOCATION;
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }

    private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Program.scriptConfig.SCRIPT_LOCATION = LocationField.Text!;
        Program.scriptConfig.SSH_NAME = NameField.Text!;
        Program.scriptConfig.SSH_IP = IPField.Text!;
        Program.scriptConfig.VM_NAME = VMField.Text!;
        Program.scriptConfig.Save(Program.masv_root);
        this.Close();
    }
}