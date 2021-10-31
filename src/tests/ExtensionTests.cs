using System;
using NUnit.Framework;
using StrongFilePath;

namespace StrongFileStructure.Tests
{
    public class ExtensionTests
    {
        [Test]
        public void TestFileExtensions()
        {
            FilePath fp = @"C:\Directory//To\File.ext".ToFilePath();
            FilePath re = fp.GetFileInfo().GetFilePath();
            using PathSegmentEnumerator fpEn = fp.GetEnumerator();
            using PathSegmentEnumerator reEn = re.GetEnumerator();
            while (fpEn.MoveNext())
            {
                Assert.IsTrue(reEn.MoveNext());

                string actual = reEn.Current.TrimEnd(FilePath.DirectorySeparators).ToString();
                string expected = fpEn.Current.TrimEnd(FilePath.DirectorySeparators).ToString();
                Assert.AreEqual(expected, actual);
            }
        }
    }
}