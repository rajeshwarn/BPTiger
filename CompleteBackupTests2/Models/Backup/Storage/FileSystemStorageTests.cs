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
            System.IO.Directory.CreateDirectory(path);

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
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteDirectoryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteFileTest()
        {
            Assert.Fail();
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
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateDirectoryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CopyFileTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void MoveFileTest()
        {
            Assert.Fail();
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