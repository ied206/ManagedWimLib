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

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class SetOutputTests
    {
        #region SetOutputChunkSize
        [TestMethod]
        public void SetOutputChunkSize()
        {
            SetOutputChunkSizeTemplate("XPRESS.wim", CompressionType.XPRESS, 16384, true);
            SetOutputChunkSizeTemplate("XPRESS.wim", CompressionType.XPRESS, 1024, false);
        }

        public void SetOutputChunkSizeTemplate(string wimFileName, CompressionType compType, uint chunkSize, bool success)
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
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(info);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(info);

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

                // Capture Wim
                string srcDir = Path.Combine(TestSetup.SampleDir, "Src01");
                string wimFile = Path.Combine(destDir, wimFileName);
                using (Wim wim = Wim.CreateNewWim(compType))
                {
                    wim.RegisterCallback(ProgressCallback);
                    try
                    {
                        wim.SetOutputChunkSize(chunkSize);
                    }
                    catch (WimException)
                    {
                        if (success)
                            Assert.Fail();
                        else
                            return;
                    }

                    wim.AddImage(srcDir, "UnitTest", null, AddFlags.None);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);
                }

                TestHelper.CheckWimPath(SampleSet.Src01, wimFile);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region SetOutputPackChunkSize
        [TestMethod]
        public void SetOutputPackChunkSize()
        {
            SetOutputPackChunkSizeTemplate("LZMS.wim", CompressionType.LZMS, 65536, true);
            SetOutputPackChunkSizeTemplate("LZMS.wim", CompressionType.LZMS, 1024, false);
        }

        public void SetOutputPackChunkSizeTemplate(string wimFileName, CompressionType compType, uint chunkSize, bool success)
        {
            string destDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(info);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                Assert.IsNotNull(info);

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

                // Capture Wim
                string srcDir = Path.Combine(TestSetup.SampleDir, "Src01");
                string wimFile = Path.Combine(destDir, wimFileName);
                using (Wim wim = Wim.CreateNewWim(compType))
                {
                    wim.RegisterCallback(ProgressCallback);
                    try
                    {
                        wim.SetOutputPackChunkSize(chunkSize);
                    }
                    catch (WimException)
                    {
                        if (success)
                            Assert.Fail();
                        else
                            return;
                    }

                    wim.AddImage(srcDir, "UnitTest", null, AddFlags.None);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.Solid, Wim.DefaultThreads);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);
                }

                TestHelper.CheckWimPath(SampleSet.Src01, wimFile);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region SetOutputCompressionType
        [TestMethod]
        public void SetOutputCompressionType()
        {
            SetOutputCompressionTypeTemplate("XPRESS.wim", CompressionType.XPRESS);
            SetOutputCompressionTypeTemplate("LZX.wim", CompressionType.LZX);
            SetOutputCompressionTypeTemplate("LZMS.wim", CompressionType.LZMS);
        }

        public void SetOutputCompressionTypeTemplate(string wimFileName, CompressionType compType)
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

                // Capture Wim
                string srcDir = Path.Combine(TestSetup.SampleDir, "Src01");
                string wimFile = Path.Combine(destDir, wimFileName);
                using (Wim wim = Wim.CreateNewWim(CompressionType.None))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.SetOutputCompressionType(compType);

                    wim.AddImage(srcDir, "UnitTest", null, AddFlags.None);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    WimInfo wi = wim.GetWimInfo();
                    Assert.AreEqual(wi.ImageCount, 1u);
                    Assert.AreEqual(wi.CompressionType, compType);
                }

                TestHelper.CheckWimPath(SampleSet.Src01, wimFile);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region SetOutputPackCompressionType
        [TestMethod]
        public void SetOutputPackCompressionType()
        {
            SetOutputPackCompressionTypeTemplate("XPRESS.wim", CompressionType.XPRESS);
            SetOutputPackCompressionTypeTemplate("LZX.wim", CompressionType.LZX);
            SetOutputPackCompressionTypeTemplate("LZMS.wim", CompressionType.LZMS);
        }

        public void SetOutputPackCompressionTypeTemplate(string wimFileName, CompressionType compType)
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

                // Capture Wim
                string srcDir = Path.Combine(TestSetup.SampleDir, "Src01");
                string wimFile = Path.Combine(destDir, wimFileName);
                using (Wim wim = Wim.CreateNewWim(CompressionType.None))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.SetOutputCompressionType(compType);
                    wim.SetOutputPackCompressionType(compType);

                    wim.AddImage(srcDir, "UnitTest", null, AddFlags.None);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.Solid, Wim.DefaultThreads);
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    WimInfo wi = wim.GetWimInfo();
                    Assert.AreEqual(wi.ImageCount, 1u);
                    Assert.AreEqual(wi.CompressionType, compType);
                }

                TestHelper.CheckWimPath(SampleSet.Src01, wimFile);
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
