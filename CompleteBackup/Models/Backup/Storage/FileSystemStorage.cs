using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.Backup.Storage
{
    public class FileSystemStorage : IStorageInterface
    {
        //private const int cMaxFilePathLength = Win32LongPathFile.MAX_PATH;
        //private const int cMaxFolderPathLength = Win32LongPathDirectory.MAX_PATH;
        private int MAX_PATH_LENGTH = Win32LongPathFile.MAX_PATH;
        //private const int cMaxWin32PathLength = Win32LongPathFile.MAX_PATH;

        public bool IsFileSameByLastChangeOnly { get; set; } = true;

        public bool IsLongPathSupported
        {
            get
            {
                return MAX_PATH_LENGTH == Win32LongPathFile.MAX_PATH;
            }
            set
            {
                if (value == true)
                {
                    MAX_PATH_LENGTH = Win32LongPathFile.MAX_PATH;
                }
                else
                {
                    MAX_PATH_LENGTH = int.MaxValue;
                }
            }
        }
        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetDirectoryName(string path)
        {
            if (path.Length < MAX_PATH_LENGTH)
            {
                return  Path.GetDirectoryName(path);
            }
            else
            {
                return Win32LongPathPath.GetDirectoryName(path);
            }
        }

        public bool FileExists(string path)
        {
            if (path.Length < MAX_PATH_LENGTH)
            {
                return System.IO.File.Exists(path);
            }
            else
            {
                return Win32LongPathFile.Exists(path);
            }
        }

        public bool DirectoryExists(string path)
        {
            if (path == null)
            {
                return false;
            }
            else if (path.Length < MAX_PATH_LENGTH)
            {
                return System.IO.Directory.Exists(path);
            }
            else
            {
                return Win32LongPathDirectory.Exists(path);
            }
        }


        public FileAttributes GetFileAttributes(string path)
        {
            if (path.Length < Win32FileSystem.MAX_PATH)
            {
                return File.GetAttributes(path);
            }
            else
            {
                return Win32LongPathFile.GetAttributes(path);
            }
        }

        //used buy delete file/directory to delete read only files
        private void SetFileAttributeRecrusive(string folder, FileAttributes attribute)
        {
            if (folder.Length < MAX_PATH_LENGTH)
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    File.SetAttributes(file, attribute);
                }

                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    File.SetAttributes(subDir, attribute);
                    SetFileAttributeRecrusive(subDir, attribute);
                }
            }
            else
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    Win32LongPathFile.SetAttributes(file, attribute);
                }

                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    Win32LongPathFile.SetAttributes(subDir, attribute);
                    SetFileAttributeRecrusive(subDir, attribute);
                }
            }
        }

        public bool DeleteDirectory(string path, bool bRecursive = true)
        {
            if (path.Length < MAX_PATH_LENGTH)
            {
                //Handle read only
                //SetFileAttribute(directory, FileAttributes.Normal);
                Directory.Delete(path, bRecursive);
            }
            else
            {
                Win32LongPathDirectory.Delete(path, bRecursive);
            }

            return true;
        }

        public void DeleteFile(string path)
        {
            if (path.Length < MAX_PATH_LENGTH)
            {
                File.Delete(path);
            }
            else
            {
                Win32LongPathFile.Delete(path);
            }
        }

        public string[] GetDirectories(string path)
        {
            string[] setEntries = null;

            if (path != null)
            {
                if (path.Length < Win32LongPathDirectory.MAX_PATH)
                {
                    setEntries = Directory.GetDirectories(path);
                }
                else
                {
                    setEntries = Win32LongPathDirectory.GetDirectories(path);
                }
            }

            return setEntries;
        }


        public DateTime GetLastWriteTime(string path)
        {
            if (path.Length < MAX_PATH_LENGTH)
            {
                return File.GetLastWriteTime(path);
            }
            else
            {
                return Win32LongPathFile.GetLastWriteTime(path);
            }
        }

        public bool IsFileSame(string file1, string file2)
        {
            if (IsFileSameByLastChangeOnly)
            {
                if (file1.Length < MAX_PATH_LENGTH && file2.Length < MAX_PATH_LENGTH)
                {
                    DateTime time1 = File.GetLastWriteTime(file1);
                    DateTime time2 = File.GetLastWriteTime(file2);

                    return time1 == time2;
                }
                else
                {
                    DateTime time1 = Win32LongPathFile.GetLastWriteTime(file1);
                    DateTime time2 = Win32LongPathFile.GetLastWriteTime(file2);

                    return time1 == time2;
                }
            }
            else
            {
                return File.ReadLines(file1).SequenceEqual(File.ReadLines(file2));
            }
        }


        public void CreateDirectory(string path, bool bCheckIfExist = false)
        {
            bool bExists = false;

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (bCheckIfExist)
            {
                bExists = DirectoryExists(path);
            }

            if (!(bExists & bCheckIfExist))
            {
                if (path.Length < MAX_PATH_LENGTH)
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                else
                {
                    Win32LongPathDirectory.CreateDirectory(path);
                }
            }
        }

        public void CopyFile(string sourcePath, string targetPath, bool bOverwrite = false)
        {
            if (sourcePath.Length < MAX_PATH_LENGTH && (targetPath.Length < MAX_PATH_LENGTH))
            {
                File.Copy(sourcePath, targetPath, bOverwrite);
            }
            else
            {
                Win32LongPathFile.Copy(sourcePath, targetPath, bOverwrite);
            }
        }

        public void MoveFile(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            if (bCreateFolder)
            {
                string targetDirectoryName = null;

                if (targetPath.Length < MAX_PATH_LENGTH)
                {
                    targetDirectoryName = Path.GetDirectoryName(targetPath);
                }
                else
                {
                    string targetFileName = Path.GetFileName(targetPath);
                    targetDirectoryName = targetPath.Substring(0, targetPath.Length - targetFileName.Length);
                }

                if (!DirectoryExists(targetDirectoryName))
                {
                    CreateDirectory(targetDirectoryName);
                }
            }

            if (sourcePath.Length < MAX_PATH_LENGTH && (targetPath.Length < MAX_PATH_LENGTH))
            {
                File.Move(sourcePath, targetPath);
            }
            else
            {
                Win32LongPathFile.Move(sourcePath, targetPath);
            }
        }


        public bool MoveDirectory(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
          //  Trace.WriteLine($"Move Directory: {sourcePath} --> {targetPath}");

            bool bResult = false;
            if (bCreateFolder)
            {
                string targetDirectoryName = GetDirectoryName(targetPath);

                if (!DirectoryExists(targetDirectoryName))
                {
                    CreateDirectory(targetDirectoryName);
                }
            }

            bool bRetry = false;
            do
            {
                try
                {
                    bRetry = false;
                    if (sourcePath.Length < MAX_PATH_LENGTH && targetPath.Length < MAX_PATH_LENGTH)
                    {
                        Directory.Move(sourcePath, targetPath);
                    }
                    else
                    {
                        Win32LongPathDirectory.Move(sourcePath, targetPath);
                    }
                    bResult = true;
                }
                catch (IOException ex)
                {
                    MessageBoxResult result = MessageBoxResult.None;
                    result = MessageBox.Show($"{ex.Message}\n\nPress Yes to retry or No to Cancel", "Directory Access", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        bRetry = true;
                    }
                }
            }
            while (bRetry);

            return bResult;
        }


        public string[] GetDirectories(string path, string searchPattern = null, System.IO.SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (path.Length < MAX_PATH_LENGTH)
            {
                return Directory.GetFiles(path, searchPattern);
            }
            else
            {
                return Win32LongPathDirectory.GetDirectories(path, searchPattern, searchOption);
            }
        }



        public List<string> GetFiles(string directory, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (directory.Length < MAX_PATH_LENGTH)
            {
                return Directory.GetFiles(directory).ToList();
            }
            else
            {
                return Win32LongPathDirectory.GetFiles(directory, searchPattern, searchOption);
            }
        }


        #region misc helpers
        public bool GetNumberOfFiles(string path, ref long files, ref long directories)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] fsInfo = dir.GetFileSystemInfos();

            return GetNumberOfFilesRec(fsInfo, ref files, ref directories);
        }

        private bool GetNumberOfFilesRec(FileSystemInfo[] FSInfo, ref long files, ref long directories)
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
                    GetNumberOfFilesRec(dInfo.GetFileSystemInfos(), ref files, ref directories);
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

        #endregion
    }
}
