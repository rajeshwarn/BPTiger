using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.History
{
    public enum HistoryTypeEnum
    {
//        NotSpecified,
        NoChange,
        Changed,
        Added,
        Deleted,
    }
    public enum HistoryItemTypeEnum
    {
        File,
        Directory,
    }

    [Serializable]
    public class HistoryItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public HistoryTypeEnum HistoryType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HistoryItemTypeEnum HistoryItemType { get; set; } = HistoryItemTypeEnum.File;

        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
    }

    [Serializable]
    public class BackupSessionHistoryData
    {
        [XmlIgnore]
        public int SessionHistoryIndex { get; set; }

        public DateTime TimeStamp { get; set; }
        public string SessionSignature { get; set; }
        public string AtchiveName { get; set; }

        public List<FolderData> SourceBackupPathList { get; set; }
        public string TargetPath { get; set; }

        public List<HistoryItem> HistoryItemList { get; set; } = new List<HistoryItem>();
    }

    public class BackupSessionHistory
    {
        IStorageInterface m_Storage;

        static readonly public string HistoryDirectory = ".BackupComnpleteHistory";

        BackupSessionHistory() { }

        public BackupSessionHistory(IStorageInterface storage)
        {
            m_Storage = storage;
        }

        public BackupSessionHistoryData HistoryData { get; set; }

        public void Reset(DateTime dateTime, string signature, List<FolderData> sourceBackupPathList, string targetPath)
        {
            HistoryData = new BackupSessionHistoryData()
            {
                TimeStamp = dateTime,
                SessionSignature = signature,
                TargetPath = targetPath,
                SourceBackupPathList = sourceBackupPathList,
                AtchiveName = BackupProfileData.TargetBackupBaseDirectoryName,
            };

            HistoryData.HistoryItemList.Clear();
        }

        public void AddFile(string source, string dest, HistoryTypeEnum historyType)
        {
            switch (historyType)
            {
                case HistoryTypeEnum.Added:
                case HistoryTypeEnum.Changed:
                case HistoryTypeEnum.Deleted:
                    var relativePath = ExtractRelativePath(HistoryData.TargetPath, dest);
                    var hItem = new HistoryItem() { SourcePath = source, TargetPath = relativePath, HistoryType = historyType };
                    HistoryData.HistoryItemList.Add(hItem);

                    break;

                case HistoryTypeEnum.NoChange:

                default:

                    break;
            }
        }


        public void AddDeletedFolder(string source, string dest)
        {
            var relativePath = ExtractRelativePath(HistoryData.TargetPath, dest);
            HistoryData.HistoryItemList.Add(new HistoryItem() { SourcePath = source, TargetPath = relativePath, HistoryType = HistoryTypeEnum.Deleted, HistoryItemType = HistoryItemTypeEnum.Directory });
        }


        private string ExtractRelativePath(string path, string fullPath)
        {
            if (fullPath.Length > path.Length)
            {
                var relativePath = fullPath.Substring(path.Length + 1);

                if (relativePath.StartsWith(HistoryData.SessionSignature))
                {
                    relativePath = relativePath.Substring(HistoryData.SessionSignature.Length + 1);

                    if (relativePath.StartsWith(HistoryData.AtchiveName))
                    {
                        relativePath = relativePath.Substring(HistoryData.AtchiveName.Length + 1);
                    }
                }

                return relativePath;
            }
            else
            {
                return fullPath;
            }
        }
        //public void SaveHistoryItem(string path, HistoryItem item)
        //{
        //    var fileName = m_Storage.GetFileName(path);
        //    var directory = m_Storage.GetDirectoryName(path);

        //    var historyDir = m_Storage.Combine(directory, HistoryDirectory);
        //    m_Storage.CreateDirectory(historyDir);

        //    var destPath = m_Storage.Combine(historyDir, fileName + "_history.json");

        //    string json = json = JsonConvert.SerializeObject(item, Formatting.Indented);

        //    File.WriteAllText(destPath, json);
        //}

        public void SaveHistory()
        {
            var m_IStorage = new FileSystemStorage();
            var fullPath = m_IStorage.Combine(HistoryData.TargetPath, HistoryData.SessionSignature);

            var historyDir = m_IStorage.Combine(fullPath, HistoryDirectory);
            m_IStorage.CreateDirectory(historyDir);

            string historyFile = m_IStorage.Combine(historyDir, $"{HistoryData.SessionSignature}_history.json");

            var historyData = new object[1];
            historyData[0] = this;
            string json = json = JsonConvert.SerializeObject(historyData, Formatting.Indented);

            File.WriteAllText(historyFile, json);
        }

        public static BackupSessionHistory LoadHistory(string path, string signature)
        {
            var m_IStorage = new FileSystemStorage();
            var fullPath = m_IStorage.Combine(path, signature);

            var historyDir = m_IStorage.Combine(fullPath, HistoryDirectory);
            //m_IStorage.CreateDirectory(historyDir);

            string historyFile = m_IStorage.Combine(historyDir, $"{signature}_history.json");
            BackupSessionHistory history = null;

            try
            {
                using (StreamReader fileStream = File.OpenText(historyFile))
                {
                    using (JsonTextReader fileReader = new JsonTextReader(fileStream))
                    {
                        Newtonsoft.Json.Linq.JToken jsonToken = null;
                        try
                        {
                            jsonToken = Newtonsoft.Json.Linq.JToken.ReadFrom(fileReader);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Exception in IJagLogsImport: {ex.Message}");
                        }

                        int iCount = jsonToken.Count();
                        var historyObj = jsonToken[0];

                        history = JsonConvert.DeserializeObject(historyObj.ToString(), typeof(BackupSessionHistory)) as BackupSessionHistory;

                        //foreach (Newtonsoft.Json.Linq.JObject obj in jsonToken)
                        //                    {
                        //                    history = JsonConvert.DeserializeObject(obj.ToString(), typeof(BackupSessionHistory)) as BackupSessionHistory;
                        //                  }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Trace.WriteLine("**Exception LoadHistory:\n" + ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Trace.WriteLine("**Exception LoadHistory:\n" + ex.Message);
            }

            return history;
        }
    }
}
