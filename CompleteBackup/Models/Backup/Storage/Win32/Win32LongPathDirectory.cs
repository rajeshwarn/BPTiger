using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Storage
{
    class Win32LongPathDirectory
    {
        //public const int MAX_PATH = 245;//260;

        public static void CreateDirectory(string path)
        {
            var paths = GetAllPathsFromPath(GetWin32LongPath(path));
            foreach (var item in paths)
            {
                if (!Exists(item))
                {
                    var ok = Win32FileSystem.CreateDirectory(item, null);// IntPtr.Zero);
                    if (!ok)
                    {
                        ThrowWin32Exception();
                    }
                }
            }            
        }

        public static void Delete(string path, bool recursive = false)
        {
            if (!recursive)
            {
                //handle read only
                //Win32LongPathFile.SetAttributes(path, System.IO.FileAttributes.Normal);
                if (!Win32FileSystem.RemoveDirectory(GetWin32LongPath(path)))
                {
                    ThrowWin32Exception();
                }
            }
            else
            {
                var longPath = new string[] { GetWin32LongPath(path) };
                DeleteDirectoriesRecrusive(longPath);
            }

            //if (path.Length < MAX_PATH && !recursive)
            //{
            //    //Handle read only
            //    //            SetFileAttribute(directory, FileAttributes.Normal);
            //    System.IO.Directory.Delete(path, recursive);
            //}
            //else
            //{
            //    if (!recursive)
            //    {
            //        //handle read only
            //        //            SetFileAttribute(directory, FileAttributes.Normal);
            //        bool ok = Win32FileSystem.RemoveDirectory(GetWin32LongPath(path));
            //        if (!ok) ThrowWin32Exception();
            //    }
            //    else
            //    {
            //        DeleteDirectories(new string[] { GetWin32LongPath(path) });
            //    }
            //}
        }


        private static void DeleteDirectoriesRecrusive(string[] directories)
        {
            foreach (string directory in directories)
            {
                var files = Win32LongPathDirectory.GetFiles(GetWin32LongPath(directory), null, System.IO.SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    Win32LongPathFile.Delete(GetWin32LongPath(file));
                }
                directories = Win32LongPathDirectory.GetDirectories(directory, null, System.IO.SearchOption.TopDirectoryOnly);
                DeleteDirectoriesRecrusive(directories);
                bool ok = Win32FileSystem.RemoveDirectory(GetWin32LongPath(directory));
                if (!ok) ThrowWin32Exception();
            }
        }


        public static bool Exists(string path)
        {
            var attr = Win32FileSystem.GetFileAttributesW(GetWin32LongPath(path));

            return (attr != Win32FileSystem.INVALID_FILE_ATTRIBUTES && ((attr & Win32FileSystem.FILE_ATTRIBUTE_DIRECTORY) == Win32FileSystem.FILE_ATTRIBUTE_DIRECTORY));
        }


        public static string[] GetDirectories(string path, string searchPattern = "*", System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly)
        {
            searchPattern = searchPattern ?? "*";
            var dirs = new List<string>();
            InternalGetDirectories(path, searchPattern, searchOption, ref dirs);
            return dirs.ToArray();
        }

        private static void InternalGetDirectories(string path, string searchPattern, System.IO.SearchOption searchOption, ref List<string> dirs)
        {
            Win32FileSystem.WIN32_FIND_DATA findData;
            IntPtr findHandle = Win32FileSystem.FindFirstFile(System.IO.Path.Combine(GetWin32LongPath(path), searchPattern), out findData);

            try
            {
                if (findHandle != new IntPtr(-1))
                {

                    do
                    {
                        if ((findData.dwFileAttributes & System.IO.FileAttributes.Directory) != 0)
                        {
                            if (findData.cFileName != "." && findData.cFileName != "..")
                            {
                                string subdirectory = System.IO.Path.Combine(path, findData.cFileName);
                                dirs.Add(GetCleanPath(subdirectory));
                                if (searchOption == System.IO.SearchOption.AllDirectories)
                                {
                                    InternalGetDirectories(subdirectory, searchPattern, searchOption, ref dirs);
                                }
                            }
                        }
                    } while (Win32FileSystem.FindNextFile(findHandle, out findData));
                    Win32FileSystem.FindClose(findHandle);
                }
                else
                {
                    //ThrowWin32Exception();
                }
            }
            catch (Exception)
            {
                Win32FileSystem.FindClose(findHandle);
                throw;
            }
        }

        public static List<string> GetFiles(string path, string searchPattern = "*", System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly)
        {
            searchPattern = searchPattern ?? "*";

            var files = new List<string>();
            var dirs = new List<string> { GetWin32LongPath(path) };

            if (searchOption == System.IO.SearchOption.AllDirectories)
            {
                //Add all the subpaths
                dirs.AddRange(Win32LongPathDirectory.GetDirectories(path, null, System.IO.SearchOption.AllDirectories));
            }

            foreach (var dir in dirs)
            {
                Win32FileSystem.WIN32_FIND_DATA findData;
                IntPtr findHandle = Win32FileSystem.FindFirstFile(System.IO.Path.Combine(GetWin32LongPath(dir), searchPattern), out findData);

                try
                {
                    if (findHandle != new IntPtr(-1))
                    {
                        do
                        {
                            if ((findData.dwFileAttributes & System.IO.FileAttributes.Directory) == 0)
                            {
                                string filename = System.IO.Path.Combine(dir, findData.cFileName);
                                files.Add(GetCleanPath(filename));
                            }
                        } while (Win32FileSystem.FindNextFile(findHandle, out findData));
                        Win32FileSystem.FindClose(findHandle);
                    }
                }
                catch (Exception)
                {
                    Win32FileSystem.FindClose(findHandle);
                    throw;
                }
            }

            return files;
        }


        public static void Move(string sourceDirName, string destDirName)
        {
            if (!Win32FileSystem.MoveFileW(GetWin32LongPath(sourceDirName), GetWin32LongPath(destDirName)))
            {
                ThrowWin32Exception();
            }
        }

#region Helpers

        [DebuggerStepThrough]
        public static void ThrowWin32Exception()
        {
            int code = Marshal.GetLastWin32Error();
            if (code != 0)
            {
                throw new System.ComponentModel.Win32Exception(code);
            }
        }

        public static string GetWin32LongPath(string path)
        {
            if (path.StartsWith(@"\\?\")) return path;

            var newpath = path;
            if (newpath.StartsWith("\\"))
            {
                newpath = @"\\?\UNC\" + newpath.Substring(2);
            }
            else if (newpath.Contains(":"))
            {
                newpath = @"\\?\" + newpath;
            }
            else
            {
                var currdir = Environment.CurrentDirectory;
                newpath = Combine(currdir, newpath);
                while (newpath.Contains("\\.\\")) newpath = newpath.Replace("\\.\\", "\\");
                newpath = @"\\?\" + newpath;
            }
            return newpath.TrimEnd('.');
        }

        private static string GetCleanPath(string path)
        {
            if (path.StartsWith(@"\\?\UNC\")) return @"\\" + path.Substring(8);
            if (path.StartsWith(@"\\?\")) return path.Substring(4);
            return path;
        }

        private static List<string> GetAllPathsFromPath(string path)
        {
            bool unc = false;
            var prefix = @"\\?\";
            if (path.StartsWith(prefix + @"UNC\"))
            {
                prefix += @"UNC\";
                unc = true;
            }
            var split = path.Split('\\');
            int i = unc ? 6 : 4;
            var list = new List<string>();
            var txt = "";

            for (int a = 0; a < i; a++)
            {
                if (a > 0) txt += "\\";
                txt += split[a];
            }
            for (; i < split.Length; i++)
            {
                txt = Combine(txt, split[i]);
                list.Add(txt);
            }

            return list;
        }

        private static string Combine(string path1, string path2)
        {
            return path1.TrimEnd('\\') + "\\" + path2.TrimStart('\\').TrimEnd('.');
        }

#endregion
    }
}
