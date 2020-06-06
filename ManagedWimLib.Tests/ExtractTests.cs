/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2017-2020 Hajin Jang

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
using System.IO;
using System.Linq;
using System.Text;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class ExtractTests
    {
        #region ExtractImage
        [TestMethod]
        public void ExtractImage()
        {
            ExtractImageTemplate("XPRESS.wim");
            ExtractImageTemplate("LZX.wim");
            ExtractImageTemplate("LZMS.wim");
        }

        public void ExtractImageTemplate(string wimFileName)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);
            string destDir = TestHelper.GetTempDir();
            try
            {
                Directory.CreateDirectory(destDir);

                bool[] _checked = new bool[5];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ExtractImageBegin:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                Assert.IsTrue(m.ImageName.Equals("Sample", StringComparison.Ordinal));
                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ExtractImageEnd:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                Assert.IsTrue(m.ImageName.Equals("Sample", StringComparison.Ordinal));
                                _checked[1] = true;
                            }
                            break;
                        case ProgressMsg.ExtractFileStructure:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                Assert.IsTrue(m.ImageName.Equals("Sample", StringComparison.Ordinal));
                                _checked[2] = true;
                            }
                            break;
                        case ProgressMsg.ExtractStreams:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                Assert.IsTrue(m.ImageName.Equals("Sample", StringComparison.Ordinal));
                                _checked[3] = true;
                            }
                            break;
                        case ProgressMsg.ExtractMetadata:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                Assert.IsTrue(m.ImageName.Equals("Sample", StringComparison.Ordinal));
                                _checked[4] = true;
                            }
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);

                    wim.ExtractImage(1, destDir, ExtractFlags.None);
                }

                Assert.IsTrue(_checked.All(x => x));

                TestHelper.CheckFileSystem(SampleSet.Src01, destDir);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region ExtractPath, ExtractPaths
        [TestMethod]
        public void ExtractPath()
        {
            ExtractPathTemplate("XPRESS.wim", @"\ACDE.txt");
            ExtractPathTemplate("LZX.wim", @"\ABCD\*.txt");
            ExtractPathTemplate("LZMS.wim", @"\ABDE\Z\Y.ini");
            ExtractPathTemplate("BootXPRESS.wim", @"\?CDE.txt");
            ExtractPathTemplate("BootLZX.wim", @"\ACDE.txt");
        }

        public void ExtractPathTemplate(string fileName, string path)
        {
            ExtractPathsTemplate(fileName, new string[] { path });
        }

        [TestMethod]
        [TestCategory("WimLib")]
        public void ExtractPaths()
        {
            string[] paths = new string[] { @"\ACDE.txt", @"\ABCD\*.txt", @"\?CDE.txt" };
            ExtractPathsTemplate("XPRESS.wim", paths);
            ExtractPathsTemplate("LZX.wim", paths);
            ExtractPathsTemplate("LZMS.wim", paths);
            ExtractPathsTemplate("BootXPRESS.wim", paths);
            ExtractPathsTemplate("BootLZX.wim", paths);
        }

        public void ExtractPathsTemplate(string fileName, string[] paths)
        {
            string destDir = TestHelper.GetTempDir();
            try
            {
                string srcDir = Path.Combine(TestSetup.SampleDir);
                string wimFile = Path.Combine(srcDir, fileName);

                bool[] _checked = new bool[5];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ExtractTreeBegin:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ExtractTreeEnd:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[1] = true;
                            }
                            break;
                        case ProgressMsg.ExtractFileStructure:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[2] = true;
                            }
                            break;
                        case ProgressMsg.ExtractStreams:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[3] = true;
                            }
                            break;
                        case ProgressMsg.ExtractMetadata:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[4] = true;
                            }
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);

                    wim.ExtractPaths(1, destDir, paths, ExtractFlags.GlobPaths);
                }

                Assert.IsTrue(_checked.All(x => x));

                foreach (string path in paths.Select(x => TestHelper.NormalizePath(x.TrimStart('\\'))))
                {
                    if (path.IndexOfAny(new char[] { '*', '?' }) == -1)
                    { // No wlidcard
                        Assert.IsTrue(File.Exists(Path.Combine(destDir, path)));
                    }
                    else
                    { // With wildcard
                        string destFullPath = Path.Combine(destDir, path);
                        string[] files = Directory.GetFiles(Path.GetDirectoryName(destFullPath), Path.GetFileName(destFullPath), SearchOption.AllDirectories);
                        Assert.IsTrue(0 < files.Length);
                    }
                }
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region ExtractList
        [TestMethod]
        public void ExtractList()
        {
            string[] paths = new string[] { @"\ACDE.txt", @"\ABCD\*.txt", @"\?CDE.txt" };
            ExtractListTemplate("XPRESS.wim", paths);
            ExtractListTemplate("LZX.wim", paths);
            ExtractListTemplate("LZMS.wim", paths);
            ExtractListTemplate("BootXPRESS.wim", paths);
            ExtractListTemplate("BootLZX.wim", paths);
        }

        public void ExtractListTemplate(string fileName, string[] paths)
        {
            string destDir = TestHelper.GetTempDir();
            try
            {
                Directory.CreateDirectory(destDir);

                bool[] _checked = new bool[5];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ExtractTreeBegin:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ExtractTreeEnd:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[1] = true;
                            }
                            break;
                        case ProgressMsg.ExtractFileStructure:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[2] = true;
                            }
                            break;
                        case ProgressMsg.ExtractStreams:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[3] = true;
                            }
                            break;
                        case ProgressMsg.ExtractMetadata:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[4] = true;
                            }
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                string listFile = Path.Combine(destDir, "ListFile.txt");
                using (StreamWriter w = new StreamWriter(listFile, false, Encoding.Unicode))
                {
                    foreach (string path in paths)
                        w.WriteLine(path);
                }

                string wimFile = Path.Combine(TestSetup.SampleDir, fileName);
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);

                    wim.ExtractPathList(1, destDir, listFile, ExtractFlags.GlobPaths);
                }

                Assert.IsTrue(_checked.All(x => x));

                foreach (string path in paths.Select(x => TestHelper.NormalizePath(x.TrimStart('\\'))))
                {
                    if (path.IndexOfAny(new char[] { '*', '?' }) == -1)
                    { // No wlidcard
                        Assert.IsTrue(File.Exists(Path.Combine(destDir, path)));
                    }
                    else
                    { // With wildcard
                        string destFullPath = Path.Combine(destDir, path);
                        string[] files = Directory.GetFiles(Path.GetDirectoryName(destFullPath), Path.GetFileName(destFullPath), SearchOption.AllDirectories);
                        Assert.IsTrue(0 < files.Length);
                    }
                }
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion
    }
}
