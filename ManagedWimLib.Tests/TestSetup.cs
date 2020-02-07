/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2017-2019 Hajin Jang

    This file is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free
    Software Foundation; either version 3 of the License, or (at your option) any
    later version.

    This file is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
    details.

    You should have received a copy of the GNU Lesser General Public License
    along with this file; if not, see http://www.gnu.org/licenses/.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace ManagedWimLib.Tests
{
    [TestClass]
    public class TestSetup
    {
        public static string BaseDir;
        public static string SampleDir;

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            string absPath = TestHelper.GetProgramAbsolutePath();
            BaseDir = Path.GetFullPath(Path.Combine(absPath, "..", "..", ".."));
            SampleDir = Path.Combine(BaseDir, "Samples");

            string arch = null;
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    arch = "x86";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                case Architecture.Arm:
                    arch = "armhf";
                    break;
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
            }
            
            string libPath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libPath = Path.Combine(absPath, arch, "libwim-15.dll");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libPath = Path.Combine(absPath, arch, "libwim.so");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libPath = Path.Combine(absPath, arch, "libwim.dylib");

            if (libPath == null || !File.Exists(libPath))
                throw new PlatformNotSupportedException();

            Wim.GlobalInit(libPath);
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            Wim.GlobalCleanup();
        }
    }

    #region Helper
    public enum SampleSet
    {
        // TestSet Src01 is created for basic test and compresstion type test
        Src01,
        // TestSet Src02 is created for multi image and delta image test 
        Src02,
        Src02_1,
        Src02_2,
        Src02_3,
        // TestSet Src03 is created for split wim test and unicode test
        Src03,
    }

    public static class TestHelper
    {
        #region Temp Path
        private static int _tempPathCounter = 0;
        private static readonly object TempPathLock = new object();
        private static readonly RNGCryptoServiceProvider SecureRandom = new RNGCryptoServiceProvider();

        private static FileStream _lockFileStream = null;
        private static string _baseTempDir = null;
        public static string BaseTempDir()
        {
            lock (TempPathLock)
            {
                if (_baseTempDir != null)
                    return _baseTempDir;

                byte[] randBytes = new byte[4];
                string systemTempDir = Path.GetTempPath();

                do
                {
                    // Get 4B of random 
                    SecureRandom.GetBytes(randBytes);
                    uint randInt = BitConverter.ToUInt32(randBytes, 0);

                    _baseTempDir = Path.Combine(systemTempDir, $"PEBakery_{randInt:X8}");
                }
                while (Directory.Exists(_baseTempDir) || File.Exists(_baseTempDir));

                // Create base temp directory
                Directory.CreateDirectory(_baseTempDir);

                // Lock base temp directory
                string lockFilePath = Path.Combine(_baseTempDir, "f.lock");
                _lockFileStream = new FileStream(lockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

                return _baseTempDir;
            }
        }

        /// <summary>
        /// Delete BaseTempDir from disk. Call this method before termination of an application.
        /// </summary>
        public static void CleanBaseTempDir()
        {
            lock (TempPathLock)
            {
                if (_baseTempDir == null)
                    return;

                _lockFileStream?.Dispose();

                if (Directory.Exists(_baseTempDir))
                    Directory.Delete(_baseTempDir, true);
                _baseTempDir = null;
            }
        }

        /// <summary>
        /// Create temp directory with synchronization.
        /// Returned temp directory path is virtually unique per call.
        /// </summary>
        /// <remarks>
        /// Returned temp file path is unique per call unless this method is called uint.MaxValue times.
        /// </remarks>
        public static string GetTempDir()
        {
            // Never call BaseTempDir in the _tempPathLock, it would cause a deadlock!
            string baseTempDir = BaseTempDir();

            lock (TempPathLock)
            {
                string tempDir;
                do
                {
                    int counter = Interlocked.Increment(ref _tempPathCounter);
                    tempDir = Path.Combine(baseTempDir, $"d{counter:X8}");
                }
                while (Directory.Exists(tempDir) || File.Exists(tempDir));

                Directory.CreateDirectory(tempDir);
                return tempDir;
            }
        }

        /// <summary>
        /// Create temp file with synchronization.
        /// Returned temp file path is virtually unique per call.
        /// </summary>
        /// <remarks>
        /// Returned temp file path is unique per call unless this method is called uint.MaxValue times.
        /// </remarks>
        public static string GetTempFile(string ext = null)
        {
            // Never call BaseTempDir in the _tempPathLock, it would cause a deadlock!
            string baseTempDir = BaseTempDir();

            // Use tmp by default / Remove '.' from ext
            ext = ext == null ? "tmp" : ext.Trim('.');

            lock (TempPathLock)
            {
                string tempFile;
                do
                {
                    int counter = Interlocked.Increment(ref _tempPathCounter);
                    tempFile = Path.Combine(baseTempDir, ext.Length == 0 ? $"f{counter:X8}" : $"f{counter:X8}.{ext}");
                }
                while (Directory.Exists(tempFile) || File.Exists(tempFile));

                File.Create(tempFile).Dispose();
                return tempFile;
            }
        }

        /// <summary>
        /// Reserve temp file path with synchronization.
        /// Returned temp file path is virtually unique per call.
        /// </summary>
        /// <remarks>
        /// Returned temp file path is unique per call unless this method is called uint.MaxValue times.
        /// </remarks>
        public static string ReserveTempFile(string ext = null)
        {
            // Never call BaseTempDir in the _tempPathLock, it would cause a deadlock!
            string baseTempDir = BaseTempDir();

            // Use tmp by default / Remove '.' from ext
            ext = ext == null ? "tmp" : ext.Trim('.');

            lock (TempPathLock)
            {
                string tempFile;
                do
                {
                    int counter = Interlocked.Increment(ref _tempPathCounter);
                    tempFile = Path.Combine(baseTempDir, ext.Length == 0 ? $"f{counter:X8}" : $"f{counter:X8}.{ext}");
                }
                while (Directory.Exists(tempFile) || File.Exists(tempFile));
                return tempFile;
            }
        }
        #endregion

        #region File and Path
        public static string GetProgramAbsolutePath()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (Path.GetDirectoryName(path) != null)
                path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return path;
        }

        public static string NormalizePath(string str)
        {
            char[] newStr = new char[str.Length];
            for (int i = 0; i < newStr.Length; i++)
            {
                switch (str[i])
                {
                    case '\\':
                    case '/':
                        newStr[i] = Path.DirectorySeparatorChar;
                        break;
                    default:
                        newStr[i] = str[i];
                        break;
                }
            }
            return new string(newStr);
        }

        public static string[] NormalizePaths(IEnumerable<string> strs)
        {
            return strs.Select(NormalizePath).ToArray();
        }

        public static Tuple<string, bool>[] NormalizePaths(IEnumerable<Tuple<string, bool>> tuples)
        {
            return tuples.Select(x => new Tuple<string, bool>(NormalizePath(x.Item1), x.Item2)).ToArray();
        }
        #endregion

        #region File Check
        public static void CheckWimPath(SampleSet set, string wimFile)
        {
            switch (set)
            {
                case SampleSet.Src01:
                    using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                    {
                        Assert.IsTrue(wim.DirExists(1, Path.Combine(@"\", "ABCD")));
                        Assert.IsTrue(wim.DirExists(1, Path.Combine(@"\", "ABCD", "Z")));
                        Assert.IsTrue(wim.DirExists(1, Path.Combine(@"\", "ABDE")));
                        Assert.IsTrue(wim.DirExists(1, Path.Combine(@"\", "ABDE", "Z")));

                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ACDE.txt")));

                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABCD", "A.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABCD", "B.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABCD", "C.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABCD", "D.ini")));

                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABCD", "Z", "X.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABCD", "Z", "Y.ini")));

                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABDE", "A.txt")));

                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABDE", "Z", "X.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "ABDE", "Z", "Y.ini")));
                    }
                    break;
                case SampleSet.Src02:
                    using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                    {
                        Assert.IsTrue(wim.DirExists(1, Path.Combine(@"\", "B")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "A.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "B", "C.txt")));
                        Assert.IsTrue(wim.FileExists(1, Path.Combine(@"\", "B", "D.ini")));

                        Assert.IsTrue(wim.DirExists(2, Path.Combine(@"\", "B")));
                        Assert.IsTrue(wim.FileExists(2, Path.Combine(@"\", "Z.txt")));
                        Assert.IsTrue(wim.FileExists(2, Path.Combine(@"\", "B", "C.txt")));
                        Assert.IsTrue(wim.FileExists(2, Path.Combine(@"\", "B", "D.ini")));

                        Assert.IsTrue(wim.DirExists(3, Path.Combine(@"\", "B")));
                        Assert.IsTrue(wim.FileExists(3, Path.Combine(@"\", "Y.txt")));
                        Assert.IsTrue(wim.FileExists(3, Path.Combine(@"\", "Z.txt")));
                        Assert.IsTrue(wim.FileExists(3, Path.Combine(@"\", "B", "C.txt")));
                        Assert.IsTrue(wim.FileExists(3, Path.Combine(@"\", "B", "D.ini")));
                    }
                    break;
                case SampleSet.Src03:
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        public static void CheckFileSystem(SampleSet set, string dir)
        {
            switch (set)
            {
                case SampleSet.Src01:
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "ABCD")));
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "ABCD", "Z")));
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "ABDE")));
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "ABDE", "Z")));

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ACDE.txt")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ACDE.txt")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABCD", "A.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABCD", "B.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABCD", "C.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABCD", "D.ini")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABCD", "A.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABCD", "B.txt")).Length == 2);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABCD", "C.txt")).Length == 3);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABCD", "D.ini")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABCD", "Z", "X.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABCD", "Z", "Y.ini")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABCD", "Z", "X.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABCD", "Z", "Y.ini")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABDE", "A.txt")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABDE", "A.txt")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABDE", "Z", "X.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "ABDE", "Z", "Y.ini")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABDE", "Z", "X.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "ABDE", "Z", "Y.ini")).Length == 1);
                    break;
                case SampleSet.Src02_1:
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "B")));

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "A.txt")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "A.txt")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "B", "C.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "B", "D.ini")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "B", "C.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "B", "D.ini")).Length == 1);
                    break;
                case SampleSet.Src02_2:
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "B")));

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "Z.txt")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "Z.txt")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "B", "C.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "B", "D.ini")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "B", "C.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "B", "D.ini")).Length == 1);
                    break;
                case SampleSet.Src02_3:
                    Assert.IsTrue(Directory.Exists(Path.Combine(dir, "B")));

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "Y.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "Z.txt")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "Y.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "Z.txt")).Length == 1);

                    Assert.IsTrue(File.Exists(Path.Combine(dir, "B", "C.txt")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "B", "D.ini")));
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "B", "C.txt")).Length == 1);
                    Assert.IsTrue(new FileInfo(Path.Combine(dir, "B", "D.ini")).Length == 1);
                    break;
                case SampleSet.Src03:
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "가")));
                    Assert.IsTrue(File.Exists(Path.Combine(dir, "나")));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void CheckPathList(SampleSet set, List<Tuple<string, bool>> paths)
        {
            Tuple<string, bool>[] checkList;
            switch (set)
            {
                case SampleSet.Src01:
                    checkList = new[]
                    {
                        new Tuple<string, bool>(Path.Combine(@"\ABCD"), true),
                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "Z"), true),
                        new Tuple<string, bool>(Path.Combine(@"\ABDE"), true),
                        new Tuple<string, bool>(Path.Combine(@"\ABDE", "Z"), true),

                        new Tuple<string, bool>(Path.Combine(@"\ACDE.txt"), false),

                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "A.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "B.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "C.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "D.ini"), false),

                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "Z", "X.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\ABCD", "Z", "Y.ini"), false),

                        new Tuple<string, bool>(Path.Combine(@"\ABDE", "A.txt"), false),

                        new Tuple<string, bool>(Path.Combine(@"\ABDE", "Z", "X.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\ABDE", "Z", "Y.ini"), false),
                    };
                    break;
                case SampleSet.Src02_1:
                    checkList = new[]
                    {
                        new Tuple<string, bool>(Path.Combine(@"\B"), true),
                        new Tuple<string, bool>(Path.Combine(@"\A.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\B", "C.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\B", "D.ini"), false),
                    };
                    break;
                case SampleSet.Src02_2:
                    checkList = new[]
                    {
                        new Tuple<string, bool>(Path.Combine(@"\B"), true),
                        new Tuple<string, bool>(Path.Combine(@"\Z.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\B", "C.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\B", "D.ini"), false),
                    };
                    break;
                case SampleSet.Src02_3:
                    checkList = new[]
                    {
                        new Tuple<string, bool>(Path.Combine(@"\B"), true),
                        new Tuple<string, bool>(Path.Combine(@"\Y.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\Z.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\B", "C.txt"), false),
                        new Tuple<string, bool>(Path.Combine(@"\B", "D.ini"), false),
                    };
                    break;
                case SampleSet.Src03:
                    checkList = new[]
                    {
                        new Tuple<string, bool>(Path.Combine(@"\가"), false),
                        new Tuple<string, bool>(Path.Combine(@"\나"), false),
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }

            checkList = NormalizePaths(checkList);
            paths = NormalizePaths(paths).ToList();
            foreach (Tuple<string, bool> tup in checkList)
                Assert.IsTrue(paths.Contains(tup, new CheckWimPathComparer()));
        }

        public static List<Tuple<string, bool>> GenerateWimPathList(string wimFile)
        {
            List<Tuple<string, bool>> entries = new List<Tuple<string, bool>>();

            CallbackStatus IterateCallback(DirEntry dentry, object userData)
            {
                string path = dentry.FullPath;
                bool isDir = (dentry.Attributes & FileAttribute.DIRECTORY) != 0;
                entries.Add(new Tuple<string, bool>(path, isDir));

                return CallbackStatus.CONTINUE;
            }

            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
            {
                wim.IterateDirTree(1, Wim.RootPath, IterateFlags.RECURSIVE, IterateCallback);
            }

            return entries;
        }

        public class CheckWimPathComparer : IEqualityComparer<Tuple<string, bool>>
        {
            public bool Equals(Tuple<string, bool> x, Tuple<string, bool> y)
            {
                bool path = x.Item1.Equals(y.Item1, StringComparison.Ordinal);
                bool isDir = x.Item2 == y.Item2;
                return path && isDir;
            }

            public int GetHashCode(Tuple<string, bool> x)
            {
                return x.Item1.GetHashCode() ^ x.Item2.GetHashCode();
            }
        }
        #endregion
    }
    #endregion

    #region CallbackTested
    public class CallbackTested
    {
        public bool Value = false;

        public CallbackTested(bool initValue)
        {
            Value = initValue;
        }

        public void Set()
        {
            Value = true;
        }

        public void Reset()
        {
            Value = false;
        }
    }
    #endregion
}
