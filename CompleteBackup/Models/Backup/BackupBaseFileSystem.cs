using CompleteBackup.Models.Backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.Profile;
using CompleteBackup.Models.Utilities;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CompleteBackup.Models.backup
{
    public class BackupBaseFileSystem
    {
        private IStorageInterface m_IStorage;
        private BackupProfileData m_Profile;
        private BackupPerfectLogger m_Logger;

        protected BackupSessionHistory m_BackupSessionHistory;

        public BackupBaseFileSystem(BackupProfileData profile)
        {
            m_Profile = profile;
            m_IStorage = profile.GetStorageInterface();
            m_Logger = profile.Logger;

            m_BackupSessionHistory = new BackupSessionHistory(profile.GetStorageInterface());
        }

        #region File System wrappers

        protected void HandleSameFile(string sourcePath, string targetPath)
        {
            m_BackupSessionHistory.AddFile(sourcePath, targetPath, HistoryTypeEnum.NoChange);
        }



        protected void CopyUpdatedFile(string sourcePath, string targetPath, bool overwrite = false)
        {
            m_Logger.Writeln($"File UpdatedL: {sourcePath}");
            m_IStorage.CopyFile(sourcePath, targetPath, overwrite);
        }
        protected void CopyRenamedFile(string sourcePath, string targetPath, bool overwrite = false)
        {
            m_Logger.Writeln($"File Renamed: {sourcePath}");
            m_IStorage.CopyFile(sourcePath, targetPath, overwrite);
        }

        protected void CopyNewFile(string sourcePath, string targetPath, bool overwrite = false)
        {
            m_Logger.Writeln($"New File {sourcePath}");

            m_IStorage.CopyFile(sourcePath, targetPath, overwrite);
        }


        protected void CreateDirectory(string path, bool bCheckIfExist = false)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Create Directory {path}");
            }
            else
            {
                m_Logger.Writeln($"Create Directory {path}");
            }

            try
            {
                m_IStorage.CreateDirectory(path, bCheckIfExist);
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while creating directory: {path}\n{ex.Message}");
            }
        }

        protected bool MoveDirectory(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Move Directory {sourcePath} To {targetPath}");
            }
            else
            {
                m_Logger.Writeln($"Move Directory {sourcePath}");
            }

            var parentDir = m_IStorage.GetDirectoryName(targetPath);
            if (!m_IStorage.DirectoryExists(parentDir))
            {
                CreateDirectory(parentDir);
            }

            return m_IStorage.MoveDirectory(sourcePath, targetPath, bCreateFolder);
        }

        protected bool DeleteDirectory(string path, bool bRecursive = true)
        {
            bool bRet = false;
            try
            {               
                bRet = m_IStorage.DeleteDirectory(path, bRecursive);
                m_Logger.Writeln($"Delete Directory {path}");
            }
            catch (UnauthorizedAccessException)
            {
                m_Logger.Writeln($"Delete Read only Directory: {path}");
                m_IStorage.SetFileAttributeRecrusive(path, FileAttributes.Normal);
                bRet = m_IStorage.DeleteDirectory(path, bRecursive);
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while deleteing directory: {path}\n{ex.Message}");
            }

            return bRet;
        }

        protected void MoveFile(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            //m_Logger.Writeln($"Move File {sourcePath}");

            m_IStorage.MoveFile(sourcePath, targetPath, bCreateFolder);
        }

        protected void DeleteFile(string path)
        {
            try
            {
                m_IStorage.DeleteFile(path);
                m_Logger.Writeln($"Delete File {path}");
            }
            catch (UnauthorizedAccessException)
            {
                m_Logger.Writeln($"Delete Read only file File {path}");
                m_IStorage.SetFileAttribute(path, FileAttributes.Normal);
                m_IStorage.DeleteFile(path);
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while deleteing file: {path}\n{ex.Message}");
            }
        }

        #endregion

    }
}
