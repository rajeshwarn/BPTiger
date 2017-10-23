using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.History
{
    class BackupSessionHistory
    {
        List<string> AddedFiles { get; set; } = new List<string>();
        List<string> ChangedFiles { get; set; } = new List<string>();
        List<string> DeletedFiles { get; set; } = new List<string>();


        public void 


        public void SaveHistory(string path)
        {
            string json = json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, json);
            System.Diagnostics.Process.Start(path);
        }
    }
}
