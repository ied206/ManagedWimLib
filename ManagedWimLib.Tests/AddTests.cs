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
using System.IO;
using System.Linq;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class AddTests
    {
        #region AddEmptyImage
        [TestMethod]
        public void AddEmptyImage()
        {
            AddEmptyImageTemplate(CompressionType.XPRESS, "XPRESS.wim");
        }

        public void AddEmptyImageTemplate(CompressionType compType, string wimFileName)
        {
            string destDir = TestHelper.GetTempDir();
            try
            {
                bool[] _checked = new bool[2];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.WriteMetadataBegin:
                            Assert.IsNull(info);
                            _checked[0] = true;
                            break;
                        case ProgressMsg.WriteMetadataEnd:
                            Assert.IsNull(info);
                            _checked[1] = true;
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                // Capture Wim
                string wimFile = Path.Combine(destDir, wimFileName);
                using (Wim wim = Wim.CreateNewWim(compType))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.AddEmptyImage("UnitTest");
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);
                }

                for (int i = 0; i < _checked.Length; i++)
                    Assert.IsTrue(_checked[i]);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region AddImage
        [TestMethod]
        public void AddImage()
        {
            AddImageTemplate("NONE.wim", CompressionType.None);
            AddImageTemplate("XPRESS.wim", CompressionType.XPRESS);
            AddImageTemplate("LZX.wim", CompressionType.LZX);
            AddImageTemplate("LZMS.wim", CompressionType.LZMS);

            AddImageTemplate("NONE.wim", CompressionType.None, AddFlags.Boot);
            AddImageTemplate("XPRESS.wim", CompressionType.XPRESS, AddFlags.Boot);
            AddImageTemplate("LZX.wim", CompressionType.LZX, AddFlags.Boot);
            AddImageTemplate("LZMS.wim", CompressionType.LZMS, AddFlags.Boot);
        }

        public void AddImageTemplate(string wimFileName, CompressionType compType, AddFlags addFlags = AddFlags.None)
        {
            string srcDir = Path.Combine(TestSetup.SampleDir, "Src01");
            string destDir = TestHelper.GetTempDir();
            string wimFile = Path.Combine(destDir, wimFileName);
            try
            {
                bool[] _checked = new bool[5];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(m);

                                _checked[1] = true;
                            }
                            break;
                        case ProgressMsg.WriteMetadataBegin:
                            Assert.IsNull(info);
                            _checked[2] = true;
                            break;
                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                Assert.IsNotNull(m);

                                _checked[3] = true;
                            }
                            break;
                        case ProgressMsg.WriteMetadataEnd:
                            Assert.IsNull(info);
                            _checked[4] = true;
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.CreateNewWim(compType))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.AddImage(srcDir, "UnitTest", null, addFlags);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);
                }

                Assert.IsTrue(_checked.All(x => x));

                TestHelper.CheckWimPath(SampleSet.Src01, wimFile);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region AddImageMultiSource
        [TestMethod]
        public void AddImageMultiSource()
        {
            AddImageMultiSourceTemplate(CompressionType.None, "NONE.wim");
            AddImageMultiSourceTemplate(CompressionType.XPRESS, "XPRESS.wim");
            AddImageMultiSourceTemplate(CompressionType.LZX, "LZX.wim");
            AddImageMultiSourceTemplate(CompressionType.LZMS, "LZMS.wim");
        }

        public void AddImageMultiSourceTemplate(CompressionType compType, string wimFileName, AddFlags addFlags = AddFlags.None)
        {
            string destDir = TestHelper.GetTempDir();
            try
            {
                bool[] _checked = new bool[5];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(m);

                                _checked[1] = true;
                            }
                            break;
                        case ProgressMsg.WriteMetadataBegin:
                            Assert.IsNull(info);
                            _checked[2] = true;
                            break;
                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                Assert.IsNotNull(m);

                                _checked[3] = true;
                            }
                            break;
                        case ProgressMsg.WriteMetadataEnd:
                            Assert.IsNull(info);
                            _checked[4] = true;
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                string srcDir1 = Path.Combine(TestSetup.SampleDir, "Src01");
                string srcDir3 = Path.Combine(TestSetup.SampleDir, "Src03");
                string wimFile = Path.Combine(destDir, wimFileName);
                using (Wim wim = Wim.CreateNewWim(compType))
                {
                    wim.RegisterCallback(ProgressCallback);

                    CaptureSource[] srcs = new CaptureSource[]
                    {
                        new CaptureSource(srcDir1, @"\A"),
                        new CaptureSource(srcDir3, @"\Z"),
                    };

                    wim.AddImageMultiSource(srcs, "UnitTest", null, addFlags);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);
                    Assert.IsTrue(wim.DirExists(1, "A"));
                    Assert.IsTrue(wim.DirExists(1, "Z"));
                }

                for (int i = 0; i < _checked.Length; i++)
                    Assert.IsTrue(_checked[i]);
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
