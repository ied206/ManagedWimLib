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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class SplitTests
    {
        #region SplitImage
        [TestMethod]
        public void Split()
        {
            SplitTemplate("Src03", (1024 + 512) * 1024);
        }

        public void SplitTemplate(string testSet, ulong partSize)
        {
            string srcDir = Path.Combine(TestSetup.SampleDir, testSet);
            string destDir = TestHelper.GetTempDir();
            string wimFile = Path.Combine(destDir, "LZX.wim");
            string splitWimFile = Path.Combine(destDir, "Split.swm");
            string splitWildcard = Path.Combine(destDir, "Split*.swm");

            try
            {
                Directory.CreateDirectory(destDir);

                bool[] _checked = new bool[2];
                for (int i = 0; i < _checked.Length; i++)
                    _checked[i] = false;
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.SplitBeginPart:
                            {
                                SplitProgress m = (SplitProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.SplitEndPart:
                            {
                                SplitProgress m = (SplitProgress)info;
                                Assert.IsNotNull(m);

                                _checked[1] = true;
                            }
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.CreateNewWim(CompressionType.LZX))
                {
                    wim.AddImage(srcDir, "UnitTest", null, AddFlags.NoAcls);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);
                }

                TestHelper.CheckWimPath(SampleSet.Src03, wimFile);

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None, ProgressCallback))
                {
                    wim.Split(splitWimFile, partSize, WriteFlags.None);
                }

                Assert.IsTrue(_checked.All(x => x));

                List<Tuple<string, bool>> entries = new List<Tuple<string, bool>>();
                int IterateCallback(DirEntry dentry, object userData)
                {
                    string path = dentry.FullPath;
                    bool isDir = (dentry.Attributes & FileAttributes.Directory) != 0;
                    entries.Add(new Tuple<string, bool>(path, isDir));

                    return Wim.IterateCallbackSuccess;
                }

                using (Wim wim = Wim.OpenWim(splitWimFile, OpenFlags.None))
                {
                    wim.ReferenceResourceFile(splitWildcard, RefFlags.GlobEnable | RefFlags.GlobErrOnNoMatch, OpenFlags.None);

                    WimInfo wi = wim.GetWimInfo();
                    Assert.IsTrue(wi.ImageCount == 1);

                    wim.IterateDirTree(1, Wim.RootPath, IterateDirTreeFlags.Recursive, IterateCallback);
                }

                TestHelper.CheckPathList(SampleSet.Src03, entries);
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
