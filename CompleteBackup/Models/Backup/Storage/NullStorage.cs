using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.Backup.Storage
{
    class NullStorage : IStorageInterface
    {
        public string GetDirectoryName(string path)
        {
            if (path.Length < 260)
            {
                return Path.GetDirectoryName(path);
            }
            else
            {
                int iIndex = path.LastIndexOf('\\');
                return path.Substring(0, iIndex);
            }
        }

        public void CreateDirectory(string path, bool bCheckIfExist = false)
        {
        }
        public bool GetNumberOfFiles(string path, ref long files, ref long directories)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] fsInfo = dir.GetFileSystemInfos();

            return GetNumberOfFiles(fsInfo, ref files, ref directories);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetDirectories(string path)
        {
            string[] setEntries = Directory.GetDirectories(path);

            return setEntries;
        }

        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        private bool GetNumberOfFiles(FileSystemInfo[] FSInfo, ref long files, ref long directories)
        {
            // Check the FSInfo parameter.
            if (FSInfo == null)
            {
                throw new ArgumentNullException("FSInfo");
            }

            // Iterate through each item.
            foreach (FileSystemInfo i in FSInfo)
            {
                // Check to see if this is a DirectoryInfo object.
                if (i is DirectoryInfo)
                {
                    // Add one to the directory count.
                    directories++;

                    // Cast the object to a DirectoryInfo object.
                    DirectoryInfo dInfo = (DirectoryInfo)i;

                    // Iterate through all sub-directories.
                    GetNumberOfFiles(dInfo.GetFileSystemInfos(), ref files, ref directories);
                }
                // Check to see if this is a FileInfo object.
                else if (i is FileInfo)
                {
                    // Add one to the file count.
                    files++;

                }
            }

            return true;
        }
        public List<string> GetFiles(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var fileList = new List<string>();

            if (Directory.Exists(directory))
            {
                string[] sourceFileEntries = Directory.GetFiles(directory);
                foreach (string path in sourceFileEntries)
                {
                    fileList.Add(Path.GetFileName(path));
                }
            }

            return fileList;
        }

        public bool IsFileSame(string file1, string file2)
        {
            //var sourceFileInfo = new System.IO.FileInfo(file1);
            //var lastSetFileInfo = new System.IO.FileInfo(file2);

            var acce = File.GetLastWriteTime(file1);
            var write = File.GetLastWriteTime(file1);
            var cere = File.GetCreationTime(file1);

            var acce2 = File.GetLastWriteTime(file2);
            var write2 = File.GetLastWriteTime(file2);
            var cere2 = File.GetCreationTime(file2);


            bool bSame = File.GetLastWriteTime(file1) == File.GetLastWriteTime(file2);
            //bool bSame = File.ReadLines(file1).SequenceEqual(File.ReadLines(file2));

            return bSame;
        }

        public void CopyFile(string sourcePath, string targetPath, bool overwrite = false)
        {
        }

        public void MoveFile(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            Trace.WriteLine($"Move File: {sourcePath} --> {targetPath}");
        }

        public void DeleteFile(string sourcePath)
        {
            Trace.WriteLine($"Delete file: {sourcePath}");
        }

        public bool DeleteDirectory(string path, bool bRecursive = false)
        {
            Trace.WriteLine($"Delete Directory: {path}");

            return true;
        }

        public bool MoveDirectory(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            Trace.WriteLine($"Move Directory: {sourcePath} --> {targetPath}");

            return true;
        }


        public string[] GetDirectories(string path, string searchPattern = "*", System.IO.SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return null;
        }
    }
}
