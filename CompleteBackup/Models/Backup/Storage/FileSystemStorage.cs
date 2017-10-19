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
    class FileSystemStorage : IStorageInterface
    {
        private const int MAX_PATH = Win32FileSystem.MAX_PATH;
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
            if (path.Length < MAX_PATH)
            {
                return  Path.GetDirectoryName(path);
            }
            else
            {
                int iIndex = path.LastIndexOf('\\');
                return path.Substring(0, iIndex);
            }
        }

        public bool FileExists(string path)
        {
            return Win32LongPathFile.Exists(path);

            //if (path.Length < MAX_PATH)
            //{
            //    return File.Exists(path);
            //}
            //else
            //{
            //    return Win32FileSystem.FileExists(Win32FileSystem.GetWin32LongPath(path));
            //}
        }

        public bool DirectoryExists(string path)
        {
            return Win32LongPathDirectory.Exists(path);

            //if (path.Length < MAX_PATH)
            //{
            //    return Directory.Exists(path);
            //}
            //else
            //{
            //    var longPath = Win32FileSystem.GetLongPath(path);

            //    return Win32FileSystem.DirectoryExists(longPath);
            //}
        }

        //used buy delete file/directory to delete read only files
        private void SetFileAttributeRecrusive(string folder, FileAttributes attribute)
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

        public bool DeleteDirectory(string path, bool bRecursive = true)
        {
            Win32LongPathDirectory.Delete(path, bRecursive);

            //            SetFileAttribute(directory, FileAttributes.Normal);
            //Directory.Delete(path, bRecursive);

            return true;
        }

        public void DeleteFile(string path)
        {
            Win32LongPathFile.Delete(path);

            //if (sourcePath.Length < MAX_PATH)
            //{
            //    File.Delete(sourcePath);
            //}
            //else
            //{
            //    Win32FileSystem.DeleteFileW(Win32FileSystem.GetWin32LongPath(sourcePath));
            //}
        }


        public string[] GetDirectories(string path)
        {
            string[] setEntries = null;

            if (path.Length < Win32LongPathDirectory.MAX_PATH)
            {
                setEntries = Directory.GetDirectories(path);
            }
            else
            {
                setEntries = Win32LongPathDirectory.GetDirectories(path);
            }

            //try
            //{
            //    setEntries = Directory.GetDirectories(path);
            //}
            //catch (IOException ex)
            //{
            //}

            return setEntries;
        }


        //private DateTime GetLastWriteTime(string path)
        //{
        //    if (path.Length < MAX_PATH)
        //    {
        //        return File.GetLastWriteTime(path);
        //    }
        //    else
        //    {
        //        return Win32LongPathFile.GetLastWriteTime(path);
        //    }
        //}

        public bool IsFileSame(string file1, string file2)
        {

            DateTime time1;
            DateTime time2;

            //bool bSame = File.ReadLines(file1).SequenceEqual(File.ReadLines(file2));
            if (file1.Length < MAX_PATH && file2.Length < MAX_PATH)
            {
                time1 = File.GetLastWriteTime(file1);
                time2 = File.GetLastWriteTime(file2);
            }
            else
            {
                time1 = Win32LongPathFile.GetLastWriteTime(file1);
                time2 = Win32LongPathFile.GetLastWriteTime(file2);
            }

            return time1 == time2;
        }

//        public List<string> GetFileList(string directory)
//        {
//            var fileList = new List<string>();

//            if (DirectoryExists(directory))
//            {
//                try
//                {
//                    string[] sourceFileEntries = GetFiles(directory);
//                    foreach (string path in sourceFileEntries)
//                    {
//                        fileList.Add(Path.GetFileName(path));
//                    }
//                }
//                catch (IOException ex)
//                {
////                    MessageBox.Show($"{ex.Message}", "Folder Selection", MessageBoxButton.OK, MessageBoxImage.Error);
//                }
//            }

//            return fileList;
//        }


        public void CreateDirectory(string path, bool bCheckIfExist = false)
        {
            if (bCheckIfExist)
            {
                if (!DirectoryExists(path))
                {
                    Win32LongPathDirectory.CreateDirectory(path);
                }
            }
            else
            {
                Win32LongPathDirectory.CreateDirectory(path);
            }

            //if (path.Length < MAX_PATH)
            //{
            //    if (bCheckIfExist)
            //    {
            //        if (!DirectoryExists(path))
            //        {
            //            Directory.CreateDirectory(path, null);
            //        }
            //    }
            //    else
            //    {
            //        Directory.CreateDirectory(path, null);
            //    }
            //}
            //else
            //{
            //    if (bCheckIfExist)
            //    {
            //        if (!DirectoryExists(Win32FileSystem.GetLongPath(path)))
            //        {
            //            Win32FileSystem.CreateDirectory(Win32FileSystem.GetLongPath(path), null);
            //        }
            //    }
            //    else
            //    {
            //        Win32FileSystem.CreateDirectory(Win32FileSystem.GetLongPath(path), null);
            //    }
            //}
        }

        public void CopyFile(string sourcePath, string targetPath, bool overwrite = false)
        {
            Win32LongPathFile.Copy(sourcePath, targetPath, overwrite);

            //if (sourcePath.Length < MAX_PATH && targetPath.Length < MAX_PATH)
            //{
            //    File.Copy(sourcePath, targetPath, overwrite);
            //}
            //else
            //{
            //    var sourcePathLong = Win32FileSystem.GetLongPath(sourcePath);
            //    var targetPathLong = Win32FileSystem.GetLongPath(targetPath);

            //    Win32FileSystem.CopyFile(sourcePathLong, targetPathLong, overwrite);
            //}
        }

        public void MoveFile(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
         //   Trace.WriteLine($"Move File: {sourcePath} --> {targetPath}");

            if (bCreateFolder)
            {
                string targetDirectoryName = null;


                if (targetPath.Length < MAX_PATH)
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
            
            Win32LongPathFile.Move(sourcePath, targetPath);

            //if (sourcePath.Length < MAX_PATH && targetPath.Length < MAX_PATH)
            //{
            //    File.Move(Win32FileSystem.GetLongPath(sourcePath), Win32FileSystem.GetLongPath(targetPath));
            //}
            //else
            //{
            //    Win32FileSystem.MoveFileW(Win32FileSystem.GetLongPath(sourcePath), Win32FileSystem.GetLongPath(targetPath));
            //}
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
                    if (sourcePath.Length < MAX_PATH && targetPath.Length < MAX_PATH)
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
            if (path.Length < MAX_PATH)
            {
                string[] fileEntries = Directory.GetFiles(path, searchPattern);

                return fileEntries;
            }
            else
            {
                return Win32LongPathDirectory.GetDirectories(path, searchPattern, searchOption);
            }
        }



        public List<string> GetFiles(string directory, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (directory.Length < MAX_PATH)
            {
                return Directory.GetFiles(directory).ToList();
            }
            else
            {
                var sourceFileEntries = Win32LongPathDirectory.GetFiles(directory, searchPattern, searchOption);
                //foreach (string path in sourceFileEntries)
                //{
                //    fileList.Add(Path.GetFileName(path));
                //}

                return sourceFileEntries;
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
