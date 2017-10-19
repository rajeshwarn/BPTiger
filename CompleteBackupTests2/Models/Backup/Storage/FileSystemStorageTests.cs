using Microsoft.VisualStudio.TestTools.UnitTesting;
using CompleteBackup.Models.Backup.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace CompleteBackup.Models.Backup.Storage.Tests
{
    [TestClass()]
    public class FileSystemStorageTests
    {

        string GetTempFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }


        [TestMethod()]
        public void CombineTest()
        {
            string path1 = @"c:\path1";
            string path2 = @"path2";

            var fs = new FileSystemStorage();
            var result = fs.Combine(path1, path2);

            if (String.Compare(result, $"{path1}\\{path2}", true) != 0)
            {
                Trace.WriteLine($"path1: {path1}, path2: {path2}");
                Trace.WriteLine($"result: {result}");
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void GetFileNameTest()
        {
            var testData = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(@"c:\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"\\server:\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"file1.txt", "file1.txt"),
                new Tuple<string, string>("", ""),
                new Tuple<string, string>(@"c:\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"c:\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"c:\path2\012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt", "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt"),
                new Tuple<string, string>(@"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"path2\012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt", "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt"),
                new Tuple<string, string>(@"file1", "file1"),
                new Tuple<string, string>(@"\", ""),
            };

            var fs = new FileSystemStorage();
            foreach (var data in testData)
            {
                var result = fs.GetFileName(data.Item1);
                if (String.Compare(result, data.Item2, true) != 0)
                {
                    Trace.WriteLine($"Input path: {data.Item1}");
                    Trace.WriteLine($"Output file: {result}, Should be: {data.Item2}");
                    Assert.Fail();
                }
            }
        }

        [TestMethod()]
        public void GetDirectoryNameTest()
        {
            var testData = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(@"c:\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"\\server:\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"file1.txt", "file1.txt"),
                new Tuple<string, string>("", ""),
                new Tuple<string, string>(@"c:\path1\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"c:\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"c:\path2\012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt", "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt"),
                new Tuple<string, string>(@"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\path2\file1.txt", "file1.txt"),
                new Tuple<string, string>(@"path2\012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt", "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.txt"),
                new Tuple<string, string>(@"file1", "file1"),
                new Tuple<string, string>(@"\", ""),
            };

            var fs = new FileSystemStorage();
            foreach (var data in testData)
            {
                var result = fs.GetFileName(data.Item1);
                if (String.Compare(result, data.Item2, true) != 0)
                {
                    Trace.WriteLine($"Input path: {data.Item1}");
                    Trace.WriteLine($"Output file: {result}, Should be: {data.Item2}");
                    Assert.Fail();
                }
            }
        }

        [TestMethod()]
        public void FileExistsTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            var result = fs.FileExists(path);
            if (result)
            {
                Trace.WriteLine($"File {path} exists on file system");
                Assert.Fail();
            }

            path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            System.IO.File.WriteAllText(path, "test");

            result = fs.FileExists(path);
            if (!result)
            {
                Trace.WriteLine($"File {path} does no exist on file system");
                Assert.Fail();
            }

            fs.DeleteFile(path);
        }

        [TestMethod()]
        public void DirectoryExistsTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            var result = fs.DirectoryExists(path);
            if (result)
            {
                Trace.WriteLine($"Directory {path} exists on file system");
                Assert.Fail();
            }

            path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            fs.CreateDirectory(path);

            result = fs.DirectoryExists(path);
            if (!result)
            {
                Trace.WriteLine($"Directory {path} does no exist on file system");
                Assert.Fail();
            }

            fs.DeleteDirectory(path);
        }

        [TestMethod()]
        public void GetFileAttributesTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            System.IO.File.WriteAllText(path, "test");

            var result = fs.GetFileAttributes(path);
            if (result == 0 || ((int)result == -1))
            {
                Trace.WriteLine($"Failed to read file attribute from filr {path}");
                Assert.Fail();
            }

            fs.DeleteFile(path);
        }

        [TestMethod()]
        public void DeleteDirectoryTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            fs.CreateDirectory(path);

            var result = fs.DirectoryExists(path);
            if (!result)
            {
                Trace.WriteLine($"Failed to create directory: {path}");
                Assert.Fail();
            }

            fs.DeleteDirectory(path);

            result = fs.DirectoryExists(path);
            if (result)
            {
                Trace.WriteLine($"Failed to delete directory: {path}");
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void DeleteFileTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            System.IO.File.WriteAllText(path, "test");

            var result = fs.FileExists(path);
            if (!result)
            {
                Trace.WriteLine($"Failed to create file: {path}");
                Assert.Fail();
            }

            fs.DeleteFile(path);
            result = fs.FileExists(path);
            if (result)
            {
                Trace.WriteLine($"Failed to delete file: {path}");
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void GetDirectoriesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetLastWriteTimeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsFileSameTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path1 = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            var path2 = fs.Combine(tempFolder, Guid.NewGuid().ToString());

            System.IO.File.WriteAllText(path1, "test");
            System.IO.File.WriteAllText(path2, "test");

            var result = fs.IsFileSame(path1, path2);
            if (result)
            {
                Trace.WriteLine($"Date signature: Files are the same: {path1}, {path2}");
                Assert.Fail();
            }

            var path3 = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            fs.CopyFile(path1, path3);
            result = fs.IsFileSame(path1, path3);
            if (!result)
            {
                Trace.WriteLine($"Date signature: Files are not the same: {path1}, {path3}");
                Assert.Fail();
            }

            fs.DeleteFile(path1);
            fs.DeleteFile(path2);
            fs.DeleteFile(path3);
        }

        [TestMethod()]
        public void CreateDirectoryTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var path = fs.Combine(tempFolder, Guid.NewGuid().ToString());
            fs.CreateDirectory(path);

            var result = fs.DirectoryExists(path);
            if (!result)
            {
                Trace.WriteLine($"Failed to create directory: {path}");
                Assert.Fail();
            }

            fs.DeleteDirectory(path);

            result = fs.DirectoryExists(path);
            if (result)
            {
                Trace.WriteLine($"Failed to delete directory: {path}");
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void CopyFileTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var testData = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(@"file1.txt", @"folder2\file2.txt"),
            };

            foreach (var data in testData)
            {
                var path1 = fs.Combine(tempFolder, data.Item1);
                var path2 = fs.Combine(tempFolder, data.Item2);

                //Delete leftovers
                try { fs.DeleteFile(path1); } catch { }
                try { fs.DeleteFile(path2); } catch { }

                System.IO.File.WriteAllText(path1, "test file");
                try
                {
                    fs.CopyFile(path1, path2);
                    Trace.WriteLine($"Copy to {path2} shuold fail to copy file - path error");
                    Assert.Fail();
                }
                catch { }

                try { fs.CreateDirectory(fs.GetDirectoryName(path2)); } catch { }

                fs.CopyFile(path1, path2, true);
                if (!fs.FileExists(path2))
                {
                    Trace.WriteLine($"failed to copy file {path2}");
                    Assert.Fail();
                }

                try
                {
                    fs.CopyFile(path1, path2);
                    Trace.WriteLine($"Copy to {path2} shuold fail to copy file - file exist");
                    Assert.Fail();
                }
                catch { }

                fs.CopyFile(path1, path2, true);

                if (!fs.FileExists(path1))
                {
                    Trace.WriteLine($"File was deleted{path1}");
                    Assert.Fail();
                }

                if (!fs.FileExists(path2))
                {
                    Trace.WriteLine($"File not copied - override {path2}");
                    Assert.Fail();
                }

                fs.DeleteFile(path1);
                fs.DeleteFile(path2);

                var targetFileName = fs.GetFileName(path2);
                var targetDirectoryName = path2.Substring(0, path2.Length - targetFileName.Length);
                fs.DeleteDirectory(targetDirectoryName);
            }
        }

        [TestMethod()]
        public void MoveFileTest()
        {
            var tempFolder = GetTempFolder();
            var fs = new FileSystemStorage();

            var testData = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(@"file1.txt", @"folder2\file2.txt"),
            };

            foreach (var data in testData)
            {
                var path1 = fs.Combine(tempFolder, data.Item1);
                var path2 = fs.Combine(tempFolder, data.Item2);

                //Delete leftovers
                try { fs.DeleteFile(path1); } catch { }
                try { fs.DeleteFile(path2); } catch { }
                try { fs.DeleteDirectory(fs.GetDirectoryName(path2)); } catch { }                

                System.IO.File.WriteAllText(path1, "test file");

                //simple move target folder no exists
                try
                {
                    fs.MoveFile(path1, path2);
                    Trace.WriteLine($"move to {path2} shuold fail to move file - path error");
                    Assert.Fail();
                }
                catch { }

                try { fs.CreateDirectory(fs.GetDirectoryName(path2)); } catch { }

                //simple move
                fs.MoveFile(path1, path2);
                if (!fs.FileExists(path2))
                {
                    Trace.WriteLine($"failed to move file {path2}");
                    Assert.Fail();
                }

                //file already exist
                fs.CopyFile(path2, path1);
                try
                {
                    fs.MoveFile(path1, path2);
                    Trace.WriteLine($"Moveto {path2} shuold fail- file exist");
                    Assert.Fail();
                }
                catch { }

                if (!fs.FileExists(path2))
                {
                    Trace.WriteLine($"File was deleted{path1}");
                    Assert.Fail();
                }

                if (!fs.FileExists(path2))
                {
                    Trace.WriteLine($"File not copied - override {path2}");
                    Assert.Fail();
                }

                fs.DeleteFile(path1);
                fs.DeleteFile(path2);

                var targetFileName = fs.GetFileName(path2);
                var targetDirectoryName = path2.Substring(0, path2.Length - targetFileName.Length);
                fs.DeleteDirectory(targetDirectoryName);
            }
        }

        [TestMethod()]
        public void MoveDirectoryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDirectoriesTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetFilesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNumberOfFilesTest()
        {
            Assert.Fail();
        }
    }
}