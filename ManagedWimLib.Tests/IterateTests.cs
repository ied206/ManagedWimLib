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

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class IterateTests
    {
        #region IterateDirTree
        [TestMethod]
        public void IterateDirTree()
        {
            IterateDirTree_Template("XPRESS.wim");
            IterateDirTree_Template("LZX.wim");
            IterateDirTree_Template("LZMS.wim");
        }

        public void IterateDirTree_Template(string wimFileName)
        {
            List<Tuple<string, bool>> entries = new List<Tuple<string, bool>>();

            int IterateCallback(DirEntry dentry, object userData)
            {
                string path = dentry.FullPath;
                bool isDir = (dentry.Attributes & FileAttributes.Directory) != 0;
                entries.Add(new Tuple<string, bool>(path, isDir));

                return Wim.IterateCallbackSuccess;
            }

            string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);
            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                wim.IterateDirTree(1, Wim.RootPath, IterateDirTreeFlags.Recursive, IterateCallback);
            }

            TestHelper.CheckPathList(SampleSet.Src01, entries);
        }
        #endregion

        #region IterateLookupTable
        [TestMethod]
        public void IterateLookupTable()
        {
            IterateLookupTableTemplate("XPRESS.wim", false);
            IterateLookupTableTemplate("LZX.wim", false);
            IterateLookupTableTemplate("LZMS.wim", true);
        }

        public void IterateLookupTableTemplate(string wimFileName, bool compSolid)
        {
            bool isSolid = false;
            int IterateCallback(ResourceEntry resource, object userData)
            {
                if (resource.Packed)
                    isSolid = true;

                return Wim.IterateCallbackSuccess;
            }

            string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);
            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                wim.IterateLookupTable(IterateLookupTableFlags.None, IterateCallback);
            }

            Assert.AreEqual(compSolid, isSolid);
        }
        #endregion
    }
}
