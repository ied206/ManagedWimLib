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
using System.IO;
using System.Linq;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class RenameTests
    {
        #region RenamePath
        [TestMethod]
        public void RenamePath()
        {
            RenamePathTemplate("XPRESS.wim", "ACDE.txt");
            RenamePathTemplate("LZX.wim", "ABCD");
            RenamePathTemplate("LZMS.wim", "ABDE");
        }

        public void RenamePathTemplate(string wimFileName, string srcPath)
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

                    Assert.IsTrue(wim.PathExists(1, srcPath));

                    wim.RenamePath(1, srcPath, "REN");
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);

                    Assert.IsTrue(_checked.All(x => x));

                    Assert.IsFalse(wim.PathExists(1, srcPath));
                    Assert.IsTrue(wim.PathExists(1, "REN"));
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
