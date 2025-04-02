using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;

namespace AttachUsbFromHost
{
    class ScriptConfig
    {
        /// <summary>
        /// Username for SSH
        /// </summary>
        public string SSH_NAME { get; set; }

        /// <summary>
        /// IP address for SSH connection
        /// </summary>
        public string SSH_IP { get; set; }


        /// <summary>
        /// Name of this virtual machine
        /// </summary>
        public string VM_NAME { get; set; }

        /// <summary>
        /// Root directory of the virsh_attach_usb script
        /// </summary>
        public string SCRIPT_LOCATION { get; set; }

        [JsonIgnore]
        private readonly JsonSerializerOptions _scriptCnfSerializerOptions = new() { WriteIndented = true, TypeInfoResolver = ScriptConfigSourceGenerationContext.Default };


        /// <summary>
        /// Replaces current configuration
        /// </summary>
        /// <param name="masv_root">Root directory for Markus' virtual computer stuff system. Usually %UserProfile\.masv.</param>
        public void Load(string masv_root)
        {
            if (File.Exists(masv_root + "/VMConfig.json"))
            {
                ScriptConfig? cnf = JsonSerializer.Deserialize<ScriptConfig>(File.ReadAllText(masv_root + "/VMConfig.json"), _scriptCnfSerializerOptions);
                this.SSH_NAME = cnf.SSH_NAME;
                this.SSH_IP = cnf.SSH_IP;
                this.VM_NAME = cnf.VM_NAME;
                this.SCRIPT_LOCATION = cnf.SCRIPT_LOCATION;
            } else
            {
                this.SSH_NAME = "markus";
                this.SSH_IP = "192.168.1.201";
                this.VM_NAME = "win10";
                this.SCRIPT_LOCATION = "~/.softweb/virsh_attach_usb/";
                Save(masv_root);
            }
        }

        /// <summary>
        /// Saves current configuration
        /// </summary>
        /// <param name="masv_root">Root directory for Markus' virtual computer stuff system. Usually %UserProfile\.masv.</param>
        public void Save(string masv_root)
        {
            var jsonData = JsonSerializer.Serialize(this, this._scriptCnfSerializerOptions);
            File.WriteAllText(masv_root + "/VMConfig.json", jsonData);
        }

    }

    // Required for generating trimmed executables
    [JsonSerializable(typeof(ScriptConfig))]
    [JsonSerializable(typeof(string))]
    internal partial class ScriptConfigSourceGenerationContext : JsonSerializerContext
    {
    }
}
