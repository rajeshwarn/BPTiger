using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
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
    public class BackupSessionHistory
    {
        IStorageInterface m_Storage;
        string signature;

        BackupSessionHistory() { }

        public BackupSessionHistory(IStorageInterface storage)
        {
            m_Storage = storage;
            //signature = profile.
        }


        [XmlIgnore]
        public int SessionHistoryIndex { get; set; }


        public List<HistoryItem> HistoryItemList { get; set; } = new List<HistoryItem>();

        public DateTime TimeStamp { get; set; }


        public void Reset(DateTime dateTime)
        {
            HistoryItemList.Clear();
            TimeStamp = dateTime;
        }

        public void AddNewFile(string source, string dest)
        {
            HistoryItemList.Add(new HistoryItem() { SourcePath = source, TargetPath = dest, HistoryType = HistoryTypeEnum.Added });
        }
        public void AddUpdatedFile(string source, string dest)
        {
            var hItem = new HistoryItem() { SourcePath = source, TargetPath = dest, HistoryType = HistoryTypeEnum.Changed };
//            SaveHistoryItem(dest, hItem);
            HistoryItemList.Add(hItem);
        }
        public void AddDeletedFile(string source, string dest)
        {
            HistoryItemList.Add(new HistoryItem() { SourcePath = source, TargetPath = dest, HistoryType = HistoryTypeEnum.Deleted });
        }
        public void AddDeletedFolder(string source, string dest)
        {
            HistoryItemList.Add(new HistoryItem() { SourcePath = source, TargetPath = dest, HistoryType = HistoryTypeEnum.Deleted, HistoryItemType = HistoryItemTypeEnum.Directory });
        }
        public void AddNoChangeFile(string source, string dest)
        {
            //HistoryItemList.Add(new HistoryItem() { Path = source, HistoryType = HistoryTypeEnum.NoChange });
        }


        public static bool IsHistoryFile(string path)
        {
            return path.EndsWith(".json", true, null);
        }


        public void SaveHistoryItem(string path, HistoryItem item)
        {
            var fileName = m_Storage.GetFileName(path);
            var directory = m_Storage.GetDirectoryName(path);
            var destPath = m_Storage.Combine(directory, fileName + "_history.json");

            string json = json = JsonConvert.SerializeObject(item, Formatting.Indented);

            File.WriteAllText(destPath, json);
        }

        public static void SaveHistory(string path, string signature, BackupSessionHistory history)
        {
            var m_IStorage = new FileSystemStorage();
            var fullPath = m_IStorage.Combine(path, signature);
            string historyFile = m_IStorage.Combine(fullPath, $"{signature}_history.json");

            var historyData = new object[1];
            historyData[0] = history;
            string json = json = JsonConvert.SerializeObject(historyData, Formatting.Indented);

            File.WriteAllText(historyFile, json);
            //System.Diagnostics.Process.Start(path);

            var hhh = LoadHistory(path, signature);

        }

        public static BackupSessionHistory LoadHistory(string path, string signature)
        {
            var m_IStorage = new FileSystemStorage();
            var fullPath = m_IStorage.Combine(path, signature);
            string historyFile = m_IStorage.Combine(fullPath, $"{signature}_history.json");
            BackupSessionHistory history = null;

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

            return history;
        }



        //XmlDocument m_xmlDoc;
        //XmlNode m_NewItemsNode;
        //XmlNode m_UpdatedItemsNode;
        //XmlNode m_DeletedItemsNode;
        //XmlNode m_NoChangeItemsNode;

        //long m_iNewFiles = 0;
        //long m_iUpdatedFiles = 0;
        //long m_iDeletedFiles = 0;
        //long m_iDeletedFolders = 0;
        //long m_iNoChangeFiles = 0;

        //public void CreateChangeLogFile()
        //{
        //    m_xmlDoc = new XmlDocument();
        //    XmlNode rootNode = m_xmlDoc.CreateElement("ChangeLog");
        //    m_xmlDoc.AppendChild(rootNode);

        //    m_NewItemsNode = m_xmlDoc.CreateElement("New");
        //    m_UpdatedItemsNode = m_xmlDoc.CreateElement("Updated");
        //    m_DeletedItemsNode = m_xmlDoc.CreateElement("Deleted");
        //    m_NoChangeItemsNode = m_xmlDoc.CreateElement("NoChange");

        //    rootNode.AppendChild(m_NewItemsNode);
        //    rootNode.AppendChild(m_UpdatedItemsNode);
        //    rootNode.AppendChild(m_DeletedItemsNode);
        //    rootNode.AppendChild(m_NoChangeItemsNode);
        //}

        //public void SaveChangeLogFile(string path, string signature)
        //{
        //    XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
        //    attribute.Value = m_iNoChangeFiles.ToString();
        //    m_NoChangeItemsNode.Attributes.Append(attribute);


        //    //            var tmpPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProject);
        //    //            var name = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        //    var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{signature}_ChangeLog.xml");

        //    string targetFile = m_IStorage.Combine(path, $"{signature}_ChangeLog.xml");


        //    m_xmlDoc.Save(file);
        //    var storage = new FileSystemStorage();
        //    storage.MoveFile(file, targetFile);


        //}

        //public void AddNewFile(string item)
        //{
        //    m_iNewFiles++;

        //    XmlNode userNode = m_xmlDoc.CreateElement("file");
        //    XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
        //    attribute.Value = m_iNewFiles.ToString();
        //    userNode.Attributes.Append(attribute);
        //    userNode.InnerText = item;
        //    m_NewItemsNode.AppendChild(userNode);
        //}
        //public void AddUpdatedFile(string item)
        //{
        //    m_iUpdatedFiles++;

        //    XmlNode userNode = m_xmlDoc.CreateElement("file");
        //    XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
        //    attribute.Value = m_iUpdatedFiles.ToString();
        //    userNode.Attributes.Append(attribute);
        //    userNode.InnerText = item;
        //    m_UpdatedItemsNode.AppendChild(userNode);
        //}
        //public void AddDeletedFile(string item)
        //{
        //    m_iDeletedFiles++;

        //    XmlNode userNode = m_xmlDoc.CreateElement("file");
        //    XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
        //    attribute.Value = m_iDeletedFiles.ToString();
        //    userNode.Attributes.Append(attribute);
        //    userNode.InnerText = item;
        //    m_DeletedItemsNode.AppendChild(userNode);
        //}

        //public void AddDeletedFolder(string item)
        //{
        //    m_iDeletedFolders++;

        //    XmlNode userNode = m_xmlDoc.CreateElement("folder");
        //    XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
        //    attribute.Value = m_iDeletedFolders.ToString();
        //    userNode.Attributes.Append(attribute);
        //    userNode.InnerText = item;
        //    m_DeletedItemsNode.AppendChild(userNode);
        //}

        //public void AddNoChangeFile(string item)
        //{
        //    m_iNoChangeFiles++;

        //    //XmlNode userNode = m_xmlDoc.CreateElement("file");
        //    //XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
        //    //attribute.Value = m_iNoChangeFiles.ToString();
        //    //userNode.Attributes.Append(attribute);
        //    //userNode.InnerText = item;
        //    //m_NoChangeItemsNode.AppendChild(userNode);
        //}

    }
}
