using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Utilities;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.Profile
{
    public class ProfileDataRefreshWorkerTask : BackgroundWorker
    {
        private ProfileDataRefreshWorkerTask() { }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void StartTask()
        {
            if (!IsBusy)
            {
                RunWorkerAsync();
            }
        }

        public ProfileDataRefreshWorkerTask(BackupProfileData profile)
        {
            //m_Profile = profile;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                var storage = profile.GetStorageInterface();
                try
                {
                    UpdateAlerts(profile);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        //profile.BackupTargetDiskSize = "n/a";
                        //profile.BackupTargetUsedSize = "n/a";
                        //profile.BackupTargetFreeSize = "n/a";

                        profile.BackupSourceFilesNumber = 0;
                        profile.BackupSourceFoldersSize = 0;

                        profile.RestoreSourceFilesNumber = 0;
                        profile.RestoreSourceFoldersSize = 0;

                        profile.BackupTargetDiskSizeNumber = 0;
                        profile.BackupTargetUsedSizeNumber = 0;
                        profile.BackupTargetFreeSizeNumber = 0;
                    }));


                    //Backup Items
                    foreach (var item in profile.BackupFolderList.Where(i => i.IsAvailable))
                    {
                        if (item.IsFolder)
                        {
                            item.NumberOfFiles = storage.GetNumberOfFiles(item.Path);
                            item.TotalSize = storage.GetSizeOfFiles(item.Path);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.BackupSourceFilesNumber += item.NumberOfFiles;
                                profile.BackupSourceFoldersSize += item.TotalSize;
                            }));
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.BackupSourceFilesNumber++;
                            }));
                        }
                    }


                    //Restore Items
                    foreach (var item in profile.RestoreFolderList)
                    {
                        if (item.IsFolder)
                        {
                            item.NumberOfFiles = storage.GetNumberOfFiles(item.Path);
                            item.TotalSize = storage.GetSizeOfFiles(item.Path);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.RestoreSourceFilesNumber += item.NumberOfFiles;
                                profile.RestoreSourceFoldersSize += item.TotalSize;
                            }));
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.RestoreSourceFilesNumber++;
                            }));
                        }
                    }


                    //Target Backup Folder
                    if (profile.IsValidFolderName(profile.GetTargetBackupFolder()))
                    {
                        //Get last backup time
                        DateTime? lastTime = null;
                        var lastSet = BackupBase.GetLastBackupSetName(profile);
                        if (lastSet != null)
                        {
                            var sessionHistory = BackupSessionHistory.LoadHistory(profile.GetTargetBackupFolder(), lastSet);
                            lastTime = sessionHistory?.TimeStamp;
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.LastBackupDateTime = lastTime;
                            }));
                        }
                    }

                    if (profile.IsValidFolderName(profile.GetTargetBackupFolder()))
                    {
                        //Target backup storage, total disk size
                        string rootDrive = System.IO.Path.GetPathRoot(profile.GetTargetBackupFolder());
                        foreach (System.IO.DriveInfo drive in System.IO.DriveInfo.GetDrives().Where(d => d.ToString().Contains(rootDrive)))
                        {
                            if (drive.IsReady)
                            {
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    profile.BackupTargetDiskSizeNumber = drive.TotalSize;
                                    profile.BackupTargetFreeSizeNumber = drive.AvailableFreeSpace;

                                    //profile.BackupTargetDiskSize = FileFolderSizeHelper.GetNumberSizeString(profile.BackupTargetDiskSizeNumber);
                                }));

                                break;
                            }
                        }

                        //Target backup folder used Space
                        var totaltargetUseSize = storage.GetSizeOfFiles(profile.GetTargetBackupFolder());
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            profile.BackupTargetUsedSizeNumber = totaltargetUseSize;
                            //profile.BackupTargetUsedSize = FileFolderSizeHelper.GetNumberSizeString(profile.BackupTargetUsedSizeNumber);
                        }));

                        //Target backup storage free space
                        //Application.Current.Dispatcher.Invoke(new Action(() =>
                        //{
                        //    profile.BackupTargetFreeSizeNumber = profile.BackupTargetDiskSizeNumber - profile.BackupTargetUsedSizeNumber;
                        //}));
                        //rootDrive = Path.GetPathRoot(profile.TargetBackupFolder);
                        //foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.ToString().Contains(rootDrive)))
                        //{
                        //    if (drive.IsReady)
                        //    {
                        //        profile.BackupTargetFreeSizeNumber = drive.AvailableFreeSpace;
                        //        //m_BackupTargetFreeSizeNumber = drive.TotalFreeSpace;
                        //        Application.Current.Dispatcher.Invoke(new Action(() =>
                        //        {
                        //            profile.BackupTargetFreeSize = FileFolderSizeHelper.GetNumberSizeString(profile.BackupTargetFreeSizeNumber);
                        //        }));

                        //        break;
                        //    }
                        //}
                    }

                    ////Source Foldes Size
                    //m_BackupSourceFoldersSizeNumber = 0;
                    //foreach (var item in FolderList)
                    //{
                    //    m_BackupSourceFoldersSizeNumber += new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                    //    Application.Current.Dispatcher.Invoke(new Action(() =>
                    //    {
                    //        BackupSourceFoldersSize = FileFolderSizeHelper.GetNumberSizeString(m_BackupSourceFoldersSizeNumber);
                    //    }));
                    //}

                    if (m_ProfileDataUpdateEventCallback != null)
                    {
                        m_ProfileDataUpdateEventCallback(profile);
                    }

                    //update data to persistent storage
                    DataRepository.BackupProjectRepository.Instance.SaveProject();
                }
                catch (TaskCanceledException ex)
                {
                    Trace.WriteLine($"Profile Data Update exception: {ex.Message}");
                    e.Result = $"Profile Data Update exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                }
            };
        }

        public delegate void ProfileDataUpdateEvent(BackupProfileData profile);
        private event ProfileDataUpdateEvent m_ProfileDataUpdateEventCallback;

        public void RegisterEvent(ProfileDataUpdateEvent callback)
        {
            m_ProfileDataUpdateEventCallback += callback;
        }

        public void UnRegisterEvent(ProfileDataUpdateEvent callback)
        {
            m_ProfileDataUpdateEventCallback -= callback;
        }



        private void UpdateAlerts(BackupProfileData profile)
        {
            var storage = profile.GetStorageInterface();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                profile.BackupAlertList.Clear();
                profile.RestoreAlertList.Clear();
            }));

            if (profile.BackupFolderList.Count() == 0)
            {
                BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupItemListEmpty);
            }
            else
            {
                foreach (var item in profile.BackupFolderList)
                {
                    if (item.IsFolder)
                    {
                        if (storage.DirectoryExists(item.Path))
                        {
                            item.IsAvailable = true;
                        }
                        else
                        {
                            BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupItemListFolderNotAvailable, item.Path);
                        }
                    }
                    else
                    {
                        if (storage.FileExists(item.Path))
                        {
                            item.IsAvailable = true;
                        }
                        else
                        {
                            BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupItemListFileNotAvailable, item.Path);
                        }
                    }
                }
            }


            if (profile.IsValidFolderName(profile.GetTargetBackupFolder()))
            {
                if (!storage.DirectoryExists(profile.GetTargetBackupFolder()))
                {
                    var folderData = profile.GetTargetBackupFolderData();
                    folderData.IsAvailable = false;

                    BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupDestinationFolderNotAvailable, folderData.Path);
                }
            }
            else
            {
                BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupDestinationFolderNotConfigured);
            }


            //Restore
            if (profile.LastBackupDateTime == null)
            {
                BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupDestinationNotFound);
            }
            else
            {
                if (profile.RestoreFolderList.Count == 0)
                {
                    BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.RestoreItemListEmpty);
                }

                if (profile.IsValidFolderName(profile.GetTargetRestoreFolder()))
                {
                    if (!storage.DirectoryExists(profile.GetTargetRestoreFolder()))
                    {
                        BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotAvailable, profile.GetTargetRestoreFolder());
                    }
                }
                else
                {
                    BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotConfigured);
                }
            }
        }


    }
}
