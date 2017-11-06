using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Storage
{
    public interface IStorageInterface
    {
        System.Drawing.Icon ExtractIconFromPath(string path);
        System.IO.FileAttributes GetFileAttributes(string path);
        string Combine(string path1, string path2);
        string GetDirectoryName(string path);
        string GetFileName(string path);
        bool FileExists(string path);
        bool DirectoryExists(string path);
        string[] GetDirectories(string path);
        string[] GetDirectoriesNames(string path);
        bool? IsDriveReady(string path);
        long GetNumberOfDirectories(string path);
        long GetNumberOfFiles(string path);
        long GetSizeOfFiles(string path);
        DateTime GetLastWriteTime(string path);
        bool IsFolder(string path);
        bool IsFileSame(string file1, string file2);
        void CreateDirectory(string path, bool bCheckIfExist = false);
        bool DeleteDirectory(string path, bool bRecursive = true);
        void CopyFile(string sourcePath, string targetPath, bool overwrite = false);
        void MoveFile(string sourcePath, string targetPath, bool bCreateFolder = false);
        void DeleteFile(string sourcePath);
        bool MoveDirectory(string sourcePath, string targetPath, bool bCreateFolder = false);

        List<string> GetFiles(string path, string searchPattern = "*", System.IO.SearchOption searchOptions = System.IO.SearchOption.TopDirectoryOnly);
        string[] GetDirectories(string path, string searchPattern = "*", System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly);
    }
}
