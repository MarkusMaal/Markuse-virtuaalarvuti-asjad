using Avalonia.Controls;
using Avalonia.Threading;
using System.Linq;
using System.Threading;
using MsBox.Avalonia;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using System.Diagnostics;
using MsBox.Avalonia.Enums;
using Tmds.DBus.Protocol;
using System.Runtime.InteropServices;

namespace AttachUsbFromHost;

public partial class MainWindow : Window
{
    MainWindowModel mwm = new();
    public static string[] Devices { get; set; }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = mwm;
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
                _ = MessagePopupShow("SSH ühendus", "Hostiga ühenduse loomine nurjus", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
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
        await new ConfigMenu()
        {
            Background = this.Background
        }.ShowDialog(this);
        Refresh();
    }

    private async void NotVirtualPc()
    {
        await MessagePopupShow("USB sisestamise tööriist", "Tegu ei ole virtuaalse arvutiga", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        this.Close();
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
                    _ = MessagePopupShow("USB ühendaja", "Seade eemaldati edukalt!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                }
                else
                {
                    _ = MessagePopupShow("USB ühendaja", "Seadme eemaldamine virtuaalarvutist nurjus.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            });
        }).Start();
    }

    private async Task MessagePopupShow(string title, string message, MsBox.Avalonia.Enums.ButtonEnum buttons, MsBox.Avalonia.Enums.Icon icon)
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
                    _ = MessagePopupShow("USB ühendaja", "Seade ühendati edukalt!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                }
                else
                {
                    _ = MessagePopupShow("USB ühendaja", "Seadme ühendamine nurjus.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            });
        }).Start();
    }

    private void LookupButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DeviceList.SelectedIndex < 0) return;
        string selection = DeviceList.Items[DeviceList.SelectedIndex]!.ToString()!;
        string[] splits = selection.Split('(');
        string[] vidpid = splits[^1][..^1].Split(':');
        string vid = vidpid[0];
        string pid = vidpid[1];
        new Process() {
            StartInfo = {
                FileName = $"https://devicehunt.com/view/type/usb/vendor/{vid}/device/{pid}",
                UseShellExecute = true,
            }
        }.Start();
    }

    private void Window_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!Directory.Exists(Program.masv_root))
        {
            this.IsEnabled = true;
            this.TopText.IsVisible = false;
            this.DeviceList.IsVisible = false;
            this.Loader.IsVisible = false;
            NotVirtualPc();
            return;
        }
        Refresh();
        if (File.Exists(Program.mas_root + "/bg_common.png"))
        {
            var _commonBg = Program.mas_root + "/bg_common.png";
            using var ms = new FileStream(_commonBg, FileMode.Open, FileAccess.Read);
            this.Background = new ImageBrush(new Bitmap(ms)) { Stretch = Stretch.UniformToFill };
            ms.Close();
        }
    }
}