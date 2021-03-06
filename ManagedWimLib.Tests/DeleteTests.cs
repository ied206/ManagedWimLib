﻿/*
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

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class DeleteTests
    {
        #region DeleteImage
        [TestMethod]
        public void DeleteImage()
        {
            DeleteImageTemplate("MultiImage.wim", 1, "Base");
            DeleteImageTemplate("MultiImage.wim", 2, "Changes");
            DeleteImageTemplate("MultiImage.wim", 3, "Delta");
        }

        public void DeleteImageTemplate(string wimFileName, int deleteIndex, string deleteImageName)
        {
            string srcWim = Path.Combine(TestSetup.SampleDir, wimFileName);
            string destDir = TestHelper.GetTempDir();
            string destWim = Path.Combine(destDir, wimFileName);
            try
            {
                Directory.CreateDirectory(destDir);
                File.Copy(srcWim, destWim, true);

                bool[] _checked = new bool[2];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                Assert.IsNotNull(m);

                                Assert.AreEqual(m.CompressionType, CompressionType.LZX);
                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.Rename:
                            {
                                RenameProgress m = (RenameProgress)info;
                                Assert.IsNotNull(m);

                                Assert.IsNotNull(m.From);
                                Assert.IsNotNull(m.To);
                                _checked[1] = true;
                            }
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(destWim, OpenFlags.WriteAccess))
                {
                    wim.RegisterCallback(ProgressCallback);

                    WimInfo swi = wim.GetWimInfo();
                    Assert.IsTrue(swi.ImageCount == 3);

                    string imageName = wim.GetImageName(deleteIndex);
                    Assert.IsTrue(imageName.Equals(deleteImageName, StringComparison.Ordinal));

                    wim.DeleteImage(deleteIndex);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);

                    for (int i = 0; i < _checked.Length; i++)
                        _checked[i] = false;

                    WimInfo dwi = wim.GetWimInfo();
                    Assert.IsTrue(dwi.ImageCount == 2);
                    for (int i = 1; i <= dwi.ImageCount; i++)
                    {
                        imageName = wim.GetImageName(i);
                        Assert.IsFalse(imageName.Equals(deleteImageName, StringComparison.Ordinal));
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

        #region DeletePath
        [TestMethod]
        public void DeletePath()
        {
            DeletePathTemplate("XPRESS.wim", "ACDE.txt");
            DeletePathTemplate("LZX.wim", "ABCD");
            DeletePathTemplate("LZMS.wim", "ABDE");
        }

        public void DeletePathTemplate(string wimFileName, string deletePath)
        {
            string srcWim = Path.Combine(TestSetup.SampleDir, wimFileName);
            string destDir = TestHelper.GetTempDir();
            string destWim = Path.Combine(destDir, wimFileName);
            try
            {
                Directory.CreateDirectory(destDir);
                File.Copy(srcWim, destWim, true);

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

                using (Wim wim = Wim.OpenWim(destWim, OpenFlags.WriteAccess))
                {
                    wim.RegisterCallback(ProgressCallback);

                    Assert.IsTrue(wim.PathExists(1, deletePath));

                    wim.DeletePath(1, deletePath, DeleteFlags.Recursive);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);

                    Assert.IsTrue(_checked.All(x => x));

                    Assert.IsFalse(wim.PathExists(1, deletePath));
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
