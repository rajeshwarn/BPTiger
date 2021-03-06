﻿using System;
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
        public bool? IsDriveReady(string path)
        {
            var drive = DriveInfo.GetDrives().Where(d => d.ToString().Contains(path)).FirstOrDefault();

            return drive.IsReady;
        }

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
        public void SetFileAttributeRecrusive(string folder, FileAttributes attribute)
        {

        }

        public void SetFileAttribute(string path, FileAttributes attribute)
        {

        }


        public void CreateDirectory(string path, bool bCheckIfExist = false)
        {
        }

        public long GetSizeOfFiles(string path)
        {

            return 0;
        }

        public long GetNumberOfDirectories(string path)
        {
            long directories = new DirectoryInfo(path).GetDirectories("*.*", SearchOption.AllDirectories).Sum(file => 1);

            return directories;
        }
        public long GetNumberOfFiles(string path)
        {
            long size = new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);

            return size;
            
            //DirectoryInfo dir = new DirectoryInfo(path);
            //FileSystemInfo[] fsInfo = dir.GetFileSystemInfos();

            //return GetNumberOfFiles(fsInfo, ref files, ref directories);
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

        public string[] GetDirectories(string path, string searchPattern = null, System.IO.SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
                return Directory.GetDirectories(path, searchPattern, searchOption);
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



        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public bool IsFolder(string path)
        {
            FileAttributes attr = GetFileAttributes(path);

            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
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

        public DateTime GetLastWriteTime(string path)
        {
            if (path.Length < 260)
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

        public System.Drawing.Icon ExtractIconFromPath(string path)
        {
            return null;
        }
    }
}
