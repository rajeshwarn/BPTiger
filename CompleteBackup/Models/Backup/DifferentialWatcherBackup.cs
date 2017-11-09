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

        protected override void ProcessDifferentialBackupRootFolders(string currentTargetPath, string lastTargetPath)
        {
            foreach (var item in m_Profile.BackupWatcherItemList)
            {
                switch (item.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        {
                            if (!m_IStorage.IsFolder(item.FullPath))
                            {
                                var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(currentTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                if (!m_IStorage.FileExists(newTargetPath))
                                {
                                    m_Logger.Writeln($"**Update File, Warning - file does not exist, will copy the updated version {newTargetPath}");

                                    CopyFile(item.FullPath, newTargetPath);
                                }
                                else
                                {
                                    var newLastPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);

                                    CopyFile(item.FullPath, newTargetPath, true);
                                }

                            }

                            break;
                        }

                    case WatcherChangeTypes.Created:
                        {
                            var sourcePath = item.FullPath;
                            var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                            if (m_IStorage.IsFolder(item.FullPath))
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
                            else
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
                            if (m_IStorage.IsFolder(newTargetPath))
                            {
                                if (m_IStorage.DirectoryExists(newTargetPath))
                                {
                                    DeleteDirectory(newTargetPath);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Delete Directory, Can't delete - directory not found {newTargetPath}");
                                }
                            }
                            else
                            {
                                if (m_IStorage.FileExists(newTargetPath))
                                {
                                    DeleteFile(newTargetPath);
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

                                if (m_IStorage.IsFolder(oldTargetPath))
                                {
                                    if (m_IStorage.DirectoryExists(oldTargetPath))
                                    {
                                        if (!m_IStorage.DirectoryExists(newTargetPath))
                                        {
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
