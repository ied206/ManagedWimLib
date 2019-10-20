/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2019 Hajin Jang

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
    [TestCategory("WimLib")]
    public class ErrorTests
    {
        [TestMethod]
        public void GetLastError()
        {
            string[] paths = new string[] { @"\NOTEXIST.bin", @"NOTGLOB?.cue" };
            CheckErrorTemplate("XPRESS.wim", paths);
            CheckErrorTemplate("XPRESS.wim", paths);
        }

        public void CheckErrorTemplate(string fileName, string[] paths)
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
                        case ProgressMsg.EXTRACT_TREE_BEGIN:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.EXTRACT_TREE_END:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                Assert.IsNotNull(m);

                                _checked[1] = true;
                            }
                            break;
                        case ProgressMsg.EXTRACT_FILE_STRUCTURE:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                Assert.IsNotNull(m);

                                _checked[2] = true;
                            }
                            break;
                        case ProgressMsg.EXTRACT_STREAMS:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                Assert.IsNotNull(m);

                                _checked[3] = true;
                            }
                            break;
                        case ProgressMsg.EXTRACT_METADATA:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                Assert.IsNotNull(m);

                                _checked[4] = true;
                            }
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                string wimFile = Path.Combine(TestSetup.SampleDir, fileName);
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.RegisterCallback(ProgressCallback);

                    wim.ExtractPaths(1, destDir, paths, ExtractFlags.GLOB_PATHS);
                }

                // The callback must not have been called
                Assert.IsFalse(_checked.Any(x => x));

                // The files must not exist
                foreach (string path in paths.Select(x => TestHelper.NormalizePath(x.TrimStart('\\'))))
                {
                    if (path.IndexOfAny(new char[] { '*', '?' }) == -1)
                    { // No wlidcard
                        Assert.IsFalse(File.Exists(Path.Combine(destDir, path)));
                    }
                    else
                    { // With wildcard
                        string destFullPath = Path.Combine(destDir, path);
                        string[] files = Directory.GetFiles(Path.GetDirectoryName(destFullPath), Path.GetFileName(destFullPath), SearchOption.AllDirectories);
                        Assert.IsFalse(0 < files.Length);
                    }
                }

                // Read error message
                string[] errorMsgs = Wim.GetErrors();
                Assert.IsNotNull(errorMsgs);
                Assert.IsTrue(0 < errorMsgs.Length);
                foreach (string errorMsg in errorMsgs)
                    Console.WriteLine(errorMsg);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
    }
}
