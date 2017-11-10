using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.backup
{
    public class DifferentialWatcherBackup: DifferentialBackup
    {

        public DifferentialWatcherBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        protected override void ProcessBackupRootFolders(string targetPath, string lastTargetPath)
        {
            try
            {
                ProcessBackupWatcherRootFolders(targetPath, lastTargetPath);
            }
            catch(Exception ex)
            {
                m_Logger.Writeln($"***DifferentialWatcherBackup Exception\n{ex.Message}");
            }
        }

        protected void ProcessDifferentialBackupRootFolders_XXXX(string targetPath, string lastTargetPath)
        {
            foreach (var item in m_Profile.BackupWatcherItemList)
            {
                switch (item.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        {

                            if (!m_IStorage.IsFolder(item.FullPath))
                            {
                                var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                if (!m_IStorage.FileExists(newTargetPath))
                                {
                                    m_Logger.Writeln($"**Update File, Warning - file does not exist, will copy the updated version {newTargetPath}");

                                    CopyFile(item.FullPath, newTargetPath);
                                }
                                else
                                {
                                    MoveFile(newTargetPath, newLastTargetPath, true);
                                    CopyFile(item.FullPath, newTargetPath, true);
                                }

                            }

                            break;
                        }

                    case WatcherChangeTypes.Created:
                        {
                            var sourcePath = item.FullPath;
                            var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);

                            bool? bFolder = null;
                            try
                            {
                                bFolder = m_IStorage.IsFolder(item.FullPath);
                            }
                            catch (FileNotFoundException)
                            {
                                m_Logger.Writeln($"**Error Create File, File not found {item.FullPath}");
                            }
                            catch (DirectoryNotFoundException)
                            {
                                m_Logger.Writeln($"**Error Create Directory, Directory not found {item.FullPath}");
                            }

                            if (bFolder == true)
                            {
                                if (!m_IStorage.DirectoryExists(newTargetPath))
                                {
                                    CreateDirectory(newTargetPath);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Create Directory, Can't create - directory already exists {newTargetPath}");
                                }
                            }
                            else if (bFolder == false)
                            {
                                if (!m_IStorage.FileExists(newTargetPath))
                                {
                                    CopyFile(sourcePath, newTargetPath);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Create File, Can't create - file already exists {newTargetPath}");
                                }
                            }

                            break;
                        }

                    case WatcherChangeTypes.Deleted:
                        {
                            var sourcePath = item.FullPath;
                            var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                            var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);

                            bool? bFolder = null;
                            try
                            {
                                bFolder = m_IStorage.IsFolder(item.FullPath);
                            }
                            catch (FileNotFoundException)
                            {
                                m_Logger.Writeln($"**Error Delete File, File not found {item.FullPath}");
                            }
                            catch (DirectoryNotFoundException)
                            {
                                m_Logger.Writeln($"**Error Delete Directory, Directory not found {item.FullPath}");
                            }

                            if (bFolder == true)
                            {
                                if (m_IStorage.DirectoryExists(newTargetPath))
                                {
                                    MoveDirectory(newTargetPath, newLastTargetPath);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Delete Directory, Can't delete - directory not found {newTargetPath}");
                                }
                            }
                            else if (bFolder == false)
                            {
                                if (m_IStorage.FileExists(newTargetPath))
                                {
                                    MoveFile(newTargetPath, newLastTargetPath, true);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Delete File, Can't delete - file not found {newTargetPath}");
                                }
                            }
                            break;
                        }

                    case WatcherChangeTypes.Renamed:
                        {
                            if (m_IStorage.IsFolder(item.WatchPath))
                            {
                                var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                var oldTargetPath = m_IStorage.Combine(m_IStorage.GetDirectoryName(newTargetPath), m_IStorage.GetFileName(item.OldPath));
                                var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), m_IStorage.GetFileName(item.OldPath));

                                if (m_IStorage.IsFolder(oldTargetPath))
                                {
                                    if (m_IStorage.DirectoryExists(oldTargetPath))
                                    {
                                        if (!m_IStorage.DirectoryExists(newTargetPath))
                                        {
                                            //CopyDirectory(oldTargetPath, newLastTargetPath);
                                            MoveDirectory(oldTargetPath, newTargetPath);
                                        }
                                        else
                                        {
                                            m_Logger.Writeln($"**Rename Directory, Can't rename - directory already exists {newTargetPath}");
                                        }
                                    }
                                    else
                                    {
                                        m_Logger.Writeln($"**Rename Directory, Can't rename - Directory not found in backup folder {oldTargetPath}");
                                    }
                                }
                                else if (m_IStorage.FileExists(oldTargetPath))
                                {
                                    if (!m_IStorage.FileExists(newTargetPath))
                                    {
                                        CopyFile(oldTargetPath, newLastTargetPath);
                                        MoveFile(oldTargetPath, newTargetPath);
                                    }
                                    else
                                    {
                                        m_Logger.Writeln($"**Rename File, Can't rename - file already exists {newTargetPath}");
                                    }
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Rename file, Can't rename - file not found in backup folder {oldTargetPath}");
                                }
                            }

                            break;
                        }

                    default:
                        break;
                }
            }
        }
    }
}
