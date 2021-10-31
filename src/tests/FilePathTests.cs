using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StrongFilePath;

namespace StrongFileStructure.Tests
{
    [TestFixture]
    public class FilePathTests
    {
        [Test]
        public void TestDefault()
        {
            FilePath fp = default;
            Assert.AreEqual(fp.ToString(), fp.FullFilePath);
            Assert.IsTrue(fp.IsEmpty);
            Assert.IsEmpty(fp.FullFilePath);
            Assert.IsEmpty(fp.DirectoryPath.ToString());
            Assert.IsEmpty(fp.FileName.ToString());
            Assert.IsEmpty(fp.FileNameWithoutExtension.ToString());
            Assert.IsEmpty(fp.Extension.ToString());
            Assert.IsEmpty(fp.ExtensionWithoutDot.ToString());
            Assert.IsFalse(fp.IsValid());
            Assert.IsFalse(fp.IsValidFileName());
            Assert.Catch<ArgumentException>(() => fp.Normalize());
            Assert.AreEqual(".ext", fp.ReplaceExtension("ext").FullFilePath);
        }

        [Test]
        public void TestFilePathFromAbsoluteFilePath()
        {
            FilePath fp = @"C:\Directory//To\File.ext".ToFilePath();
            Assert.AreEqual(@"C:\Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"File.ext", fp.FileName.ToString());
            Assert.AreEqual(@"File", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@".ext", fp.Extension.ToString());
            Assert.AreEqual(@"ext", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRoot | PathFlags.FileNameWithExtension, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsTrue(fp.HasFileExtension);
        }

        [Test]
        public void TestFilePathFromRelativeThisFilePath()
        {
            FilePath fp = @".\Directory//To\File.ext".ToFilePath();
            Assert.AreEqual(@".\Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"File.ext", fp.FileName.ToString());
            Assert.AreEqual(@"File", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@".ext", fp.Extension.ToString());
            Assert.AreEqual(@"ext", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRelative | PathFlags.FileNameWithExtension, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsTrue(fp.HasFileExtension);
        }

        [Test]
        public void TestFilePathFromRelativeParentFilePath()
        {
            FilePath fp = @"..\../Directory//To\File.ext".ToFilePath();
            Assert.AreEqual(@"..\../Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"File.ext", fp.FileName.ToString());
            Assert.AreEqual(@"File", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@".ext", fp.Extension.ToString());
            Assert.AreEqual(@"ext", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRelative | PathFlags.FileNameWithExtension, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsTrue(fp.HasFileExtension);
        }

        [Test]
        public void TestFilePathFromRelativeNoDotFilePath()
        {
            FilePath fp = @"\Directory//To\File.ext".ToFilePath();
            Assert.AreEqual(@"\Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"File.ext", fp.FileName.ToString());
            Assert.AreEqual(@"File", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@".ext", fp.Extension.ToString());
            Assert.AreEqual(@"ext", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRelative | PathFlags.FileNameWithExtension, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsTrue(fp.HasFileExtension);
        }

        [Test]
        public void TestFilePathFromRelativePrefixFilePath()
        {
            FilePath fp = @"Directory//To\File.ext".ToFilePath();
            Assert.AreEqual(@"Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"File.ext", fp.FileName.ToString());
            Assert.AreEqual(@"File", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@".ext", fp.Extension.ToString());
            Assert.AreEqual(@"ext", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRelative | PathFlags.FileNameWithExtension, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsTrue(fp.HasFileExtension);
        }
        
        [Test]
        public void TestNoExtension()
        {
            string path = @"Directory//To\File";
            FilePath fp = path.ToFilePath();
            Assert.AreEqual(path, fp.FullFilePath);
            Assert.AreEqual(@"Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"File", fp.FileName.ToString());
            Assert.AreEqual(@"File", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@"", fp.Extension.ToString());
            Assert.AreEqual(@"", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRelative | PathFlags.FileName, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsFalse(fp.HasFileExtension);
        }
        
        [Test]
        public void TestFileName()
        {
            FilePath fp = @"Directory//To\".ToFilePath();
            Assert.AreEqual(@"Directory//To\", fp.DirectoryPath.ToString());
            Assert.AreEqual(@"", fp.FileName.ToString());
            Assert.AreEqual(@"", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@"", fp.Extension.ToString());
            Assert.AreEqual(@"", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.DirectoryRelative, fp.Flags);
            Assert.IsTrue(fp.HasDirectory);
            Assert.IsFalse(fp.HasFileName);
            Assert.IsFalse(fp.HasFileExtension);
        }
        
        [Test]
        public void TestOnlyFileExtensions()
        {
            FilePath fp = @".gitignore".ToFilePath();
            Assert.AreEqual(@"", fp.DirectoryPath.ToString());
            Assert.AreEqual(@".gitignore", fp.FileName.ToString());
            Assert.AreEqual(@"", fp.FileNameWithoutExtension.ToString());
            Assert.AreEqual(@".gitignore", fp.Extension.ToString());
            Assert.AreEqual(@"gitignore", fp.ExtensionWithoutDot.ToString());
            Assert.AreEqual(PathFlags.FileNameWithExtension, fp.Flags);
            Assert.IsFalse(fp.HasDirectory);
            Assert.IsTrue(fp.HasFileName);
            Assert.IsTrue(fp.HasFileExtension);
        }

        [Test]
        public void TestValidateFilePath()
        {
            FilePath illegal = @"C||:/<Directory>\FIle".ToFilePath();
            Assert.IsFalse(illegal.IsValid());
            foreach (FilePath file in Environment.CurrentDirectory.ToFilePath().GetDirectoryInfo().EnumerateFiles().Select(fi => fi.GetFilePath()))
            {
                Assert.IsTrue(file.IsValid());
            }
        }

        [Test]
        public void TestValidateFileName()
        {
            Assert.IsFalse("".ToFilePath().IsValidFileName());
            FilePath illegal = @"<|sd-v.,efe34+<>""|"":".ToFilePath();
            Assert.IsFalse(illegal.IsValidFileName());
            foreach (FilePath file in Environment.CurrentDirectory.ToFilePath().GetDirectoryInfo().EnumerateFiles().Select(fi => fi.GetFilePath()))
            {
                Assert.IsFalse(file.IsValidFileName());
                Assert.IsTrue(file.FileName.ToFilePath().IsValidFileName());
            }
        }

        [Test]
        public void TestFileOpenShared()
        {
            FilePath fp = Path.GetTempFileName().ToFilePath();
            Assert.IsTrue(fp.Exists());
            using (FileStream write = fp.OpenWrite())
            {
                write.Write(Encoding.UTF8.GetBytes("Hello World"));
                write.Flush();
                
                using (FileStream read = fp.OpenWrite())
                {
                    Span<byte> byteBuffer = stackalloc byte[4096];
                    Span<byte> bytesRead = byteBuffer.Slice(0,  read.Read(byteBuffer));
                    Span<char> stringBuffer = stackalloc char[2048];
                    string stringRead = stringBuffer.Slice(0, Encoding.UTF8.GetChars(bytesRead, stringBuffer)).ToString();
                    
                    Assert.AreEqual("Hello World", stringRead);
                }
            }
            fp.GetFileInfo().Delete();
        }

        [Test]
        public void TestFileOpenLocked()
        {
            FilePath fp = Path.GetTempFileName().ToFilePath();
            using (FileStream write = fp.Create(shared: false))
            {
                Assert.Catch(() => fp.OpenRead().Dispose());
                Assert.Catch(() => fp.OpenRead(shared: false).Dispose());

                write.Write(Encoding.UTF8.GetBytes("Hello World"));
                write.Flush();
            }
            
            using (FileStream read = fp.OpenWrite())
            {
                Span<byte> byteBuffer = stackalloc byte[4096];
                Span<byte> bytesRead = byteBuffer.Slice(0,  read.Read(byteBuffer));
                Span<char> stringBuffer = stackalloc char[2048];
                string stringRead = stringBuffer.Slice(0, Encoding.UTF8.GetChars(bytesRead, stringBuffer)).ToString();
                    
                Assert.AreEqual("Hello World", stringRead);
            }

            fp.GetFileInfo().Delete();
        }

        [Test]
        public void TestExtensionEquals()
        {
            FilePath fp = @"\path\to\file.txt".ToFilePath();
            Assert.IsTrue(fp.ExtensionEquals(".txt"));
            Assert.IsTrue(fp.ExtensionEquals("txt"));
            
            FilePath fp2 = @".gitignore".ToFilePath();
            Assert.IsTrue(fp2.ExtensionEquals(".gitignore"));
            Assert.IsTrue(fp2.ExtensionEquals("gitignore"));
        }

        [Test]
        public void TestFilePathCombine()
        {
            FilePath fp = @"C:\path\to\file.txt".ToFilePath();
            Assert.AreEqual(fp.ToString(), @"C:\path\to".ToFilePath().CombineWith("file.txt").ToString());
            Assert.AreEqual(fp.ToString(), @"C:\path".ToFilePath().CombineWith("to", "file.txt").ToString());
            Assert.AreEqual(fp.ToString(), "C:".ToFilePath().CombineWith("path", "to", "file.txt").ToString());
            Assert.AreEqual(fp.ToString(), FilePath.CombineDirectoryNameExtension(@"C:\path\to\", "file", "txt").ToString());
        }
    }
}
