using Avalonia.Controls;
using Avalonia.Threading;
using System.Linq;
using System.Threading;
using MsBox.Avalonia;
using System.Threading.Tasks;

namespace AttachUsbFromHost;

public partial class MainWindow : Window
{
    MainWindowModel mwm = new();
    public static string[] Devices { get; set; }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = mwm;
        Refresh();
    }
    
    private void Refresh()
    {
        new Thread(() =>
        {
            ReloadItems();
            Dispatcher.UIThread.Post(() =>
            {
                RefreshListBox();
            });
        }).Start();
    }

    private void RefreshListBox()
    {
        DeviceList.Items.Clear();
        foreach (string device in Devices)
        {
            DeviceList.Items.Add(device);
        }
        DeviceList.IsEnabled = true;
    }

    private void MinusButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeviceList.FontSize -= 1;
    }

    private void PlusButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeviceList.FontSize += 1;
    }

    private void ReloadButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Refresh();
    }

    public void ReloadItems()
    {
        Devices = [];
        Dispatcher.UIThread.Post(() =>
        {
            Loader.IsVisible = true;
            SideButtons.IsVisible = false;
            DeviceList.IsVisible = false;
            TopText.IsVisible = false;
        });
        string output = Program.GetStdOut("lsusb");
        Dispatcher.UIThread.Post(() =>
        {
            this.IsEnabled = true;
            Loader.IsVisible = false;
            SideButtons.IsVisible = true;
            DeviceList.IsVisible = true;
            TopText.IsVisible = true;
            if (output == "")
            {
                MessagePopupShow("SSH ühendus", "Hostiga ühenduse loomine nurjus", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                return;
            }
            else
            {
                Devices = output[..^1].Split("\n");
                FormatItems();
            }
        });
    }

    private void FormatItems()
    {
        for (int i = 0; i < Devices.Length; i++)
        {
            if (Devices[i].ToString() != null)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                string FriendlyName = string.Join(':', Devices[i]
                    .ToString()
                    .Split(":")[^1])
                    .Substring(5);
                string VendorID = string.Join(':', Devices[i]
                    .ToString()
                    .Split(":")[1]).Substring(4);
                string DeviceID = Devices[i]
                    .ToString()
                    .Split(":")[2][..4];
                Devices[i] = FriendlyName + " (" + VendorID + ":" + DeviceID + ")";
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }
    }

    private async void ConfigButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await new ConfigMenu().ShowDialog(this);
        Refresh();
        
    }

    private void RemoveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int idx = DeviceList.SelectedIndex;
        this.IsEnabled = false;
        new Thread(() =>
        {
            string status = Program.GetStdOut(Program.scriptConfig.SCRIPT_LOCATION + "detach.sh " + idx + " " + Program.scriptConfig.VM_NAME);
            Dispatcher.UIThread.Post(() =>
            {
                this.IsEnabled = true;
                if (status.Contains("successfully"))
                {
                    MessagePopupShow("USB ühendaja", "Seade eemaldati edukalt!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                }
                else
                {
                    MessagePopupShow("USB ühendaja", "Seadme eemaldamine virtuaalarvutist nurjus.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            });
        }).Start();
    }

    private async void MessagePopupShow(string title, string message, MsBox.Avalonia.Enums.ButtonEnum buttons, MsBox.Avalonia.Enums.Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, icon, WindowStartupLocation.CenterOwner);
        _ = await box.ShowAsPopupAsync(this);
    }

    private void ConnectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int idx = DeviceList.SelectedIndex;
        this.IsEnabled = false;
        new Thread(() =>
        {
            string status = Program.GetStdOut(Program.scriptConfig.SCRIPT_LOCATION + "attach.sh " + idx + " " + Program.scriptConfig.VM_NAME);
            Dispatcher.UIThread.Post(() =>
            {
                this.IsEnabled = true;
                if (status.Contains("successfully"))
                {
                    MessagePopupShow("USB ühendaja", "Seade ühendati edukalt!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                }
                else
                {
                    MessagePopupShow("USB ühendaja", "Seadme ühendamine nurjus.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            });
        }).Start();
    }
}