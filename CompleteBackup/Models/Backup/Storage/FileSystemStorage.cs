using CompleteBackup.DataRepository;
using CompleteBackup.Models.Profile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.Backup.Storage
{
    public class FileSystemStorage : IStorageInterface
    {
        //private const int cMaxFilePathLength = Win32LongPathFile.MAX_PATH;
        //private const int cMaxFolderPathLength = Win32LongPathDirectory.MAX_PATH;
        private int MAX_PATH_LENGTH = Win32LongPathFile.MAX_PATH;
//        private int MAX_DIRECTORY_NAME = Win32LongPathFile.MAX_DIRECTORY_NAME;
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

        public bool IsLongPath(string path)
        {
            return Win32LongPathFile.IsLongPath(path);
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
            if (!IsLongPath(path))
            {
                return Path.GetDirectoryName(path);
            }
            else
            {
                return Win32LongPathPath.GetDirectoryName(path);
            }
        }

        public bool FileExists(string path)
        {
            bool bExists = false;

            if (!IsLongPath(path))
            {
                bExists = System.IO.File.Exists(path);
            }
            else
            {
                bExists = Win32LongPathFile.Exists(path);
            }

            int code = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            return bExists;
        }

        public bool IsFolder(string path)
        {
            FileAttributes attr = GetFileAttributes(path);

            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public bool DirectoryExists(string path)
        {
            if ((path == null) || (path == string.Empty))
            {
                return false;
            }
            else if (!IsLongPath(path))
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

        public void SetFileAttribute(string path, FileAttributes attribute)
        {
            if (!IsLongPath(path))
            {
                File.SetAttributes(path, attribute);
            }
            else
            {
                Win32LongPathFile.SetAttributes(path, attribute);
            }
        }


        //used buy delete file/directory to delete read only files
        public void SetFileAttributeRecrusive(string folder, FileAttributes attribute)
        {
            foreach (string file in GetFiles(folder))
            {
                SetFileAttribute(file, attribute);
            }

            foreach (string subDir in GetDirectories(folder))
            {
                SetFileAttribute(subDir, attribute);
                SetFileAttributeRecrusive(subDir, attribute);
            }
        }

        public bool DeleteDirectory(string path, bool bRecursive = false)
        {
            if (!IsLongPath(path))
            {
                //Handle read only
                //SetFileAttribute(directory, FileAttributes.Normal);
                try
                {
                    Directory.Delete(path, bRecursive);
                }
                catch (System.IO.IOException)
                {
                    //Retry if recrusive path is long path
                    Win32LongPathDirectory.Delete(path, bRecursive);
                }
            }
            else
            {
                Win32LongPathDirectory.Delete(path, bRecursive);
            }

            return true;
        }

        public void DeleteFile(string path)
        {
            if (!IsLongPath(path))
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
                if (!IsLongPath(path))
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

        public string[] GetDirectoriesNames(string path)
        {
            string[] setEntries = GetDirectories(path);

            for (int i = 0; i < setEntries.Length; i++)
            {
                setEntries[i] = GetFileName(setEntries[i]);
            }

            return setEntries;
        }

        public DateTime GetLastWriteTime(string path)
        {
            if (!IsLongPath(path))
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
                if (!IsLongPath(file1) && !IsLongPath(file2))
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
                string directoryName = GetDirectoryName(path);

                if (!IsLongPath(path))
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
            if (!IsLongPath(sourcePath) && !IsLongPath(targetPath))
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

                if (!IsLongPath(targetPath))
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

            if (!IsLongPath(sourcePath) && !IsLongPath(targetPath))
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
                    if (!IsLongPath(sourcePath) && !IsLongPath(targetPath))
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
                    //ProfileDataRefreshTask
                    var project = BackupProjectRepository.Instance.SelectedBackupProject;
                    if (project.BackupProfileList.FirstOrDefault(p => (p.ProfileDataRefreshTask != null) && p.ProfileDataRefreshTask.IsBusy) != null)
                    {
                        Thread.Sleep(1000);
                        bRetry = true;
                    }
                    else
                    {
                        MessageBoxResult result = MessageBoxResult.None;
                        result = MessageBox.Show($"{ex.Message}\n\nPress Yes to retry or No to Cancel", "Directory Access", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);

                        if (result == MessageBoxResult.Yes)
                        {
                            bRetry = true;
                        }
                    }
                }
            }
            while (bRetry);

            return bResult;
        }


        public string[] GetDirectories(string path, string searchPattern = null, System.IO.SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!IsLongPath(path))
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
            if (!IsLongPath(directory))
            {
                return Directory.GetFiles(directory)?.ToList();
            }
            else
            {
                return Win32LongPathDirectory.GetFiles(directory, searchPattern, searchOption);
            }
        }


        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;

        public System.Drawing.Icon ExtractIconFromPath(string path)
        {
            Win32FileSystem.SHFILEINFO shinfo = new Win32FileSystem.SHFILEINFO();
            Win32FileSystem.SHGetFileInfo(path, 0, ref shinfo,
                                          (uint)System.Runtime.InteropServices.Marshal.SizeOf(shinfo),
                                          SHGFI_ICON | SHGFI_LARGEICON);

            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }


        public long GetSizeOfFiles(string path)
        {
            long size = 0;

            try
            {
                size = new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch (DirectoryNotFoundException ex)
            {
                DataRepository.BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.Logger.Writeln("***Exception: " + ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                DataRepository.BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.Logger.Writeln("***Exception: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex )
            {
                DataRepository.BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile.Logger?.Writeln("***Exception: " + ex.Message);
            }

            return size;
        }

        public bool? IsDriveReady(string path)
        {
            var drive = DriveInfo.GetDrives().Where(d => d.ToString().Contains(path)).FirstOrDefault();

            return drive.IsReady;
        }


        #region misc helpers
        public long GetNumberOfDirectories(string path)
        {
            long directories = new DirectoryInfo(path).GetDirectories("*.*", SearchOption.AllDirectories).Sum(file => 1);

            return directories;
        }
        public long GetNumberOfFiles(string path)
        {
            long files = 0;

            try
            {
                files = new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => 1);
            }
            catch(UnauthorizedAccessException)
            {

            }

            return files;
        }

        //private bool GetNumberOfFilesRec(FileSystemInfo[] FSInfo, ref long files, ref long directories)
        //{
        //    // Check the FSInfo parameter.
        //    if (FSInfo == null)
        //    {
        //        throw new ArgumentNullException("FSInfo");
        //    }

        //    // Iterate through each item.
        //    foreach (FileSystemInfo i in FSInfo)
        //    {
        //        // Check to see if this is a DirectoryInfo object.
        //        if (i is DirectoryInfo)
        //        {
        //            // Add one to the directory count.
        //            directories++;

        //            // Cast the object to a DirectoryInfo object.
        //            DirectoryInfo dInfo = (DirectoryInfo)i;
        //            try
        //            {
        //                // Iterate through all sub-directories.
        //                GetNumberOfFilesRec(dInfo.GetFileSystemInfos(), ref files, ref directories);
        //            }
        //            catch (PathTooLongException ex)
        //            { }
        //        }
        //        // Check to see if this is a FileInfo object.
        //        else if (i is FileInfo)
        //        {
        //            // Add one to the file count.
        //            files++;

        //        }
        //    }

        //    return true;
        //}

        #endregion
    }
}
