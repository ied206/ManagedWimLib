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
    public class ReferenceTests
    {
        #region ReferenceTemplateImage
        [TestMethod]
        public void ReferenceTemplateImage()
        {
            ReferenceTemplateImageTemplate("MultiImage.wim", "Src02_2", SampleSet.Src02_2);
        }

        public void ReferenceTemplateImageTemplate(string wimFileName, string captureDir, SampleSet set)
        {
            string destDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(destDir);

                string srcDir = Path.Combine(TestSetup.SampleDir, captureDir);
                string srcWimFile = Path.Combine(TestSetup.SampleDir, wimFileName);
                string destWimFile = Path.Combine(destDir, wimFileName);
                File.Copy(srcWimFile, destWimFile, true);

                int imageCount;
                using (Wim wim = Wim.OpenWim(destWimFile, OpenFlags.WriteAccess))
                {
                    WimInfo wi = wim.GetWimInfo();
                    imageCount = (int)wi.ImageCount;

                    wim.AddImage(srcDir, "UnitTest", null, AddFlags.None);
                    wim.ReferenceTemplateImage(imageCount + 1, 1);

                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
                }

                List<Tuple<string, bool>> entries = new List<Tuple<string, bool>>();

                int IterateCallback(DirEntry dentry, object userData)
                {
                    string path = dentry.FullPath;
                    bool isDir = (dentry.Attributes & FileAttributes.Directory) != 0;
                    entries.Add(new Tuple<string, bool>(path, isDir));

                    return Wim.IterateCallbackSuccess;
                }

                string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);
                using (Wim wim = Wim.OpenWim(destWimFile, OpenFlags.None))
                {
                    wim.IterateDirTree(imageCount + 1, Wim.RootPath, IterateDirTreeFlags.Recursive, IterateCallback);
                }

                TestHelper.CheckPathList(set, entries);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }

        public void ReferenceTemplateImageTemplate(string[] splitWimNames, RefFlags refFlags = RefFlags.None, bool failure = false)
        {
            string[] splitWims = splitWimNames.Select(x => Path.Combine(TestSetup.SampleDir, x)).ToArray();
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
                        case ProgressMsg.ExtractImageBegin:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ExtractImageEnd:
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

                try
                {
                    using (Wim wim = Wim.OpenWim(splitWims[0], OpenFlags.None, ProgressCallback))
                    {
                        var leftSplitWims = splitWims.Skip(1);
                        wim.ReferenceResourceFiles(leftSplitWims, refFlags, OpenFlags.None);

                        wim.ExtractImage(1, destDir, ExtractFlags.NoAcls);
                    }
                }
                catch (WimLibException)
                {
                    if (failure)
                        return;
                    else
                        Assert.Fail();
                }

                Assert.IsTrue(_checked.All(x => x));

                TestHelper.CheckFileSystem(SampleSet.Src03, destDir);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region ReferenceResourceFiles
        [TestMethod]
        public void ReferenceResourceFiles()
        {
            ReferenceResourceFilesTemplate(new[] { "Split.swm", "Split2.swm" });
            ReferenceResourceFilesTemplate(new[] { "Split.swm", "Split*.swm" }, RefFlags.GlobEnable | RefFlags.GlobErrOnNoMatch);
            ReferenceResourceFilesTemplate(new[] { "Split.swm", "Split*.swm" }, RefFlags.GlobEnable | RefFlags.GlobErrOnNoMatch, true);
        }

        public void ReferenceResourceFilesTemplate(string[] splitWimNames, RefFlags refFlags = RefFlags.None, bool failure = false)
        {
            string[] splitWims = splitWimNames.Select(x => Path.Combine(TestSetup.SampleDir, x)).ToArray();
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

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ExtractImageEnd:
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

                try
                {
                    using (Wim wim = Wim.OpenWim(splitWims[0], OpenFlags.None))
                    {
                        wim.RegisterCallback(ProgressCallback);

                        var leftSplitWims = splitWims.Skip(1);
                        wim.ReferenceResourceFiles(leftSplitWims, refFlags, OpenFlags.None);

                        wim.ExtractImage(1, destDir, ExtractFlags.NoAcls);
                    }
                }
                catch (WimLibException)
                {
                    if (failure)
                        return;
                    else
                        Assert.Fail();
                }

                Assert.IsTrue(_checked.All(x => x));

                TestHelper.CheckFileSystem(SampleSet.Src03, destDir);
            }
            finally
            {
                if (Directory.Exists(destDir))
                    Directory.Delete(destDir, true);
            }
        }
        #endregion

        #region ReferenceResources
        [TestMethod]
        public void ReferenceResources()
        {
            ReferenceResourcesTemplate(new[] { "Split.swm", "Split2.swm" });
        }

        public void ReferenceResourcesTemplate(string[] splitWimNames, RefFlags refFlags = RefFlags.None, bool failure = false)
        {
            string[] splitWimPaths = splitWimNames.Select(x => Path.Combine(TestSetup.SampleDir, x)).ToArray();
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
                        case ProgressMsg.ExtractImageBegin:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                Assert.IsNotNull(m);

                                _checked[0] = true;
                            }
                            break;
                        case ProgressMsg.ExtractImageEnd:
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

                try
                {
                    using (Wim wim = Wim.OpenWim(splitWimPaths[0], OpenFlags.None, ProgressCallback))
                    {
                        Wim[] splitWims = new Wim[splitWimPaths.Length - 1];
                        try
                        {
                            for (int i = 0; i < splitWims.Length; i++)
                                splitWims[i] = Wim.OpenWim(splitWimPaths[i + 1], OpenFlags.None);

                            wim.ReferenceResources(splitWims);
                            wim.ExtractImage(1, destDir, ExtractFlags.NoAcls);
                        }
                        finally
                        {
                            foreach (var t in splitWims)
                                t?.Dispose();
                        }
                    }
                }
                catch (WimLibException)
                {
                    if (failure)
                        return;
                    else
                        Assert.Fail();
                }

                Assert.IsTrue(_checked.All(x => x));

                TestHelper.CheckFileSystem(SampleSet.Src03, destDir);
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
