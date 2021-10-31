using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using StrongFilePath;

namespace StrongFileStructure.Tests
{
    [TestFixture]
    public class PathSegmentEnumeratorTests
    {
        [Test]
        public void TestEnumerateEmptyFilePath()
        {
            FilePath fp = FilePath.Empty;
            using PathSegmentEnumerator en = fp.GetEnumerator();
            Assert.IsFalse(en.MoveNext());
            Assert.IsEmpty(en.Current.ToString());
        }

        [Test]
        public void TestEnumerateSinglePortion()
        {
            FilePath fp = @"File.ext".ToFilePath();
            using PathSegmentEnumerator en = fp.GetEnumerator();
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"File.ext");
            Assert.IsFalse(en.MoveNext());
        }

        [Test]
        public void TestEnumerateFullFilePath()
        {
            FilePath fp = @"C:\Directory//To\File.ext".ToFilePath();
            using PathSegmentEnumerator en = fp.GetEnumerator();
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"C:\");
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"Directory//");
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"To\");
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"File.ext");
            Assert.IsFalse(en.MoveNext());

            en.Reset();
            Assert.IsFalse(en.MovePrevious());
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"C:\");
        }


        [Test]
        public void TestEnumerateReverseFullFilePath()
        {
            FilePath fp = @"C:\Directory//To\File.ext".ToFilePath();
            using PathSegmentEnumerator en = fp.GetEnumerator();

            en.ResetToEnd();
            Assert.IsFalse(en.MoveNext());
            Assert.IsTrue(en.MovePrevious());
            Assert.AreEqual(en.Current.ToString(), @"File.ext");
            Assert.IsTrue(en.MovePrevious());
            Assert.AreEqual(en.Current.ToString(), @"To\");
            Assert.IsTrue(en.MovePrevious());
            Assert.AreEqual(en.Current.ToString(), @"Directory//");
            Assert.IsTrue(en.MovePrevious());
            Assert.AreEqual(en.Current.ToString(), @"C:\");
            Assert.IsFalse(en.MovePrevious());
            
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"C:\");
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"Directory//");
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"To\");
            Assert.IsTrue(en.MoveNext());
            Assert.AreEqual(en.Current.ToString(), @"File.ext");
            Assert.IsFalse(en.MoveNext());
            
            Assert.IsTrue(en.MovePrevious());
            Assert.AreEqual(en.Current.ToString(), @"File.ext");            
        }

        [Test]
        public void TestBoxedEnumerate()
        {
            FilePath fp = @"C:\Directory//To\File.ext".ToFilePath();
            using (IEnumerator<string> en = fp.GetEnumerator())
            {
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"C:\");
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"Directory//");
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"To\");
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"File.ext");
                Assert.IsFalse(en.MoveNext());   
            }

            {
                IEnumerator en = fp.GetEnumerator();
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"C:\");
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"Directory//");
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"To\");
                Assert.IsTrue(en.MoveNext());
                Assert.AreEqual(en.Current, @"File.ext");
                Assert.IsFalse(en.MoveNext());   
            }
        }
    }
}