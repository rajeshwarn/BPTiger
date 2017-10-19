using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Storage
{
    class Win32LongPathFile
    {
        public const int MAX_PATH = 260;

        public static bool Exists(string path)
        {
            var attr = Win32FileSystem.GetFileAttributesW(GetWin32LongPath(path));

            return (attr != Win32FileSystem.INVALID_FILE_ATTRIBUTES && ((attr & Win32FileSystem.FILE_ATTRIBUTE_ARCHIVE) == Win32FileSystem.FILE_ATTRIBUTE_ARCHIVE));

            //if (path.Length < MAX_PATH)
            //{
            //    return System.IO.File.Exists(path);
            //}
            //else
            //{
            //    var attr = Win32FileSystem.GetFileAttributesW(GetWin32LongPath(path));

            //    return (attr != Win32FileSystem.INVALID_FILE_ATTRIBUTES && ((attr & Win32FileSystem.FILE_ATTRIBUTE_ARCHIVE) == Win32FileSystem.FILE_ATTRIBUTE_ARCHIVE));
            //}
        }

        public static void Delete(string path)
        {
            if (path.Length < MAX_PATH) System.IO.File.Delete(path);
            else
            {
                bool ok = Win32FileSystem.DeleteFileW(GetWin32LongPath(path));
                if (!ok) ThrowWin32Exception();
            }
        }

        public static void AppendAllText(string path, string contents)
        {
            AppendAllText(path, contents, Encoding.Default);
        }

        public static void AppendAllText(string path, string contents, Encoding encoding)
        {
            if (path.Length < MAX_PATH)
            {
                System.IO.File.AppendAllText(path, contents, encoding);
            }
            else
            {
                var fileHandle = CreateFileForAppend(GetWin32LongPath(path));
                using (var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Write))
                {
                    var bytes = encoding.GetBytes(contents);
                    fs.Position = fs.Length;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public static void WriteAllText(string path, string contents)
        {
            WriteAllText(path, contents, Encoding.Default);
        }

        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            if (path.Length < MAX_PATH)
            {
                System.IO.File.WriteAllText(path, contents, encoding);
            }
            else
            {
                var fileHandle = CreateFileForWrite(GetWin32LongPath(path));

                using (var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Write))
                {
                    var bytes = encoding.GetBytes(contents);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (path.Length < MAX_PATH)
            {
                System.IO.File.WriteAllBytes(path, bytes);
            }
            else
            {
                var fileHandle = CreateFileForWrite(GetWin32LongPath(path));

                using (var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public static void Copy(string sourceFileName, string destFileName, bool bOverwrite = false)
        {
            if (!Win32FileSystem.CopyFileW(GetWin32LongPath(sourceFileName), GetWin32LongPath(destFileName), !bOverwrite))
            {
                    ThrowWin32Exception();
            }
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            if (!Win32FileSystem.MoveFileW(GetWin32LongPath(sourceFileName), GetWin32LongPath(destFileName)))
            {
                    ThrowWin32Exception();
            }
        }

        public static string ReadAllText(string path)
        {
            return ReadAllText(path, Encoding.Default);
        }

        public static string ReadAllText(string path, Encoding encoding)
        {
            if (path.Length < MAX_PATH) { return System.IO.File.ReadAllText(path, encoding); }
            var fileHandle = GetFileHandle(GetWin32LongPath(path));

            using (var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Read))
            {
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                return encoding.GetString(data);
            }
        }

        public static string[] ReadAllLines(string path)
        {
            return ReadAllLines(path, Encoding.Default);
        }

        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            if (path.Length < MAX_PATH) { return System.IO.File.ReadAllLines(path, encoding); }
            var fileHandle = GetFileHandle(GetWin32LongPath(path));

            using (var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Read))
            {
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                var str = encoding.GetString(data);
                if (str.Contains("\r")) return str.Split(new[] { "\r\n" }, StringSplitOptions.None);
                return str.Split('\n');
            }
        }
        public static byte[] ReadAllBytes(string path)
        {
            if (path.Length < MAX_PATH) return System.IO.File.ReadAllBytes(path);
            var fileHandle = GetFileHandle(GetWin32LongPath(path));

            using (var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Read))
            {
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                return data;
            }
        }


        public static void SetAttributes(string path, System.IO.FileAttributes attributes)
        {
            var longFilename = GetWin32LongPath(path);
            Win32FileSystem.SetFileAttributesW(longFilename, (int)attributes);

            //if (path.Length < MAX_PATH)
            //{
            //    System.IO.File.SetAttributes(path, attributes);
            //}
            //else
            //{
            //    var longFilename = GetWin32LongPath(path);
            //    Win32FileSystem.SetFileAttributesW(longFilename, (int)attributes);
            //}
        }

        #region Helper methods

        private static SafeFileHandle CreateFileForWrite(string filename)
        {
            if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
            SafeFileHandle hfile = Win32FileSystem.CreateFile(filename, (int)Win32FileSystem.FILE_GENERIC_WRITE, Win32FileSystem.FILE_SHARE_NONE, IntPtr.Zero, Win32FileSystem.CREATE_ALWAYS, 0, IntPtr.Zero);
            if (hfile.IsInvalid) ThrowWin32Exception();
            return hfile;
        }

        private static SafeFileHandle CreateFileForAppend(string filename)
        {
            if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
            SafeFileHandle hfile = Win32FileSystem.CreateFile(filename, (int)Win32FileSystem.FILE_GENERIC_WRITE, Win32FileSystem.FILE_SHARE_NONE, IntPtr.Zero, Win32FileSystem.CREATE_NEW, 0, IntPtr.Zero);
            if (hfile.IsInvalid)
            {
                hfile = Win32FileSystem.CreateFile(filename, (int)Win32FileSystem.FILE_GENERIC_WRITE, Win32FileSystem.FILE_SHARE_NONE, IntPtr.Zero, Win32FileSystem.OPEN_EXISTING, 0, IntPtr.Zero);
                if (hfile.IsInvalid) ThrowWin32Exception();
            }
            return hfile;
        }

        internal static SafeFileHandle GetFileHandle(string filename)
        {
            if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
            SafeFileHandle hfile = Win32FileSystem.CreateFile(filename, (int)Win32FileSystem.FILE_GENERIC_READ, Win32FileSystem.FILE_SHARE_READ, IntPtr.Zero, Win32FileSystem.OPEN_EXISTING, 0, IntPtr.Zero);
            if (hfile.IsInvalid) ThrowWin32Exception();
            return hfile;
        }

        internal static SafeFileHandle GetFileHandleWithWrite(string filename)
        {
            if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
            SafeFileHandle hfile = Win32FileSystem.CreateFile(filename, (int)(Win32FileSystem.FILE_GENERIC_READ | Win32FileSystem.FILE_GENERIC_WRITE | Win32FileSystem.FILE_WRITE_ATTRIBUTES), Win32FileSystem.FILE_SHARE_NONE, IntPtr.Zero, Win32FileSystem.OPEN_EXISTING, 0, IntPtr.Zero);
            if (hfile.IsInvalid) ThrowWin32Exception();
            return hfile;
        }

        public static System.IO.FileStream GetFileStream(string filename, System.IO.FileAccess access = System.IO.FileAccess.Read)
        {
            var longFilename = GetWin32LongPath(filename);
            SafeFileHandle hfile;
            if (access == System.IO.FileAccess.Write)
            {
                hfile = Win32FileSystem.CreateFile(longFilename, (int)(Win32FileSystem.FILE_GENERIC_READ | Win32FileSystem.FILE_GENERIC_WRITE | Win32FileSystem.FILE_WRITE_ATTRIBUTES), Win32FileSystem.FILE_SHARE_NONE, IntPtr.Zero, Win32FileSystem.OPEN_EXISTING, 0, IntPtr.Zero);
            }
            else
            {
                hfile = Win32FileSystem.CreateFile(longFilename, (int)Win32FileSystem.FILE_GENERIC_READ, Win32FileSystem.FILE_SHARE_READ, IntPtr.Zero, Win32FileSystem.OPEN_EXISTING, 0, IntPtr.Zero);
            }

            if (hfile.IsInvalid) ThrowWin32Exception();

            return new System.IO.FileStream(hfile, access);
        }


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

            if (path.StartsWith("\\"))
            {
                path = @"\\?\UNC\" + path.Substring(2);
            }
            else if (path.Contains(":"))
            {
                path = @"\\?\" + path;
            }
            else
            {
                var currdir = Environment.CurrentDirectory;
                path = Combine(currdir, path);
                while (path.Contains("\\.\\")) path = path.Replace("\\.\\", "\\");
                path = @"\\?\" + path;
            }
            return path.TrimEnd('.'); ;
        }

        private static string Combine(string path1, string path2)
        {
            return path1.TrimEnd('\\') + "\\" + path2.TrimStart('\\').TrimEnd('.'); ;
        }


        #endregion

        public static void SetCreationTime(string path, DateTime creationTime)
        {
            long cTime = 0;
            long aTime = 0;
            long wTime = 0;

            using (var handle = GetFileHandleWithWrite(path))
            {
                Win32FileSystem.GetFileTime(handle, ref cTime, ref aTime, ref wTime);
                var fileTime = creationTime.ToFileTimeUtc();
                if (!Win32FileSystem.SetFileTime(handle, ref fileTime, ref aTime, ref wTime))
                {
                    ThrowWin32Exception();
                }
            }
        }

        public static void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            long cTime = 0;
            long aTime = 0;
            long wTime = 0;

            using (var handle = GetFileHandleWithWrite(path))
            {
                Win32FileSystem.GetFileTime(handle, ref cTime, ref aTime, ref wTime);

                var fileTime = lastAccessTime.ToFileTimeUtc();
                if (!Win32FileSystem.SetFileTime(handle, ref cTime, ref fileTime, ref wTime))
                {
                    ThrowWin32Exception();
                }
            }
        }

        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            long cTime = 0;
            long aTime = 0;
            long wTime = 0;

            using (var handle = GetFileHandleWithWrite(path))
            {
                Win32FileSystem.GetFileTime(handle, ref cTime, ref aTime, ref wTime);

                var fileTime = lastWriteTime.ToFileTimeUtc();
                if (!Win32FileSystem.SetFileTime(handle, ref cTime, ref aTime, ref fileTime))
                {
                    ThrowWin32Exception();
                }
            }
        }

        public static DateTime GetLastWriteTime(string path)
        {
            long cTime = 0;
            long aTime = 0;
            long wTime = 0;

            using (var handle = GetFileHandleWithWrite(path))
            {
                Win32FileSystem.GetFileTime(handle, ref cTime, ref aTime, ref wTime);

                return DateTime.FromFileTimeUtc(wTime);
            }
        }
    }
}
