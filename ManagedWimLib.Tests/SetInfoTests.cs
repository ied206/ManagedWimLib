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

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class SetInfoTests
    {
        #region SetImageInfo
        [TestMethod]
        public void SetImageInfo()
        {
            SetImageInfoTemplate("LZX.wim", 1);
            SetImageInfoTemplate("MultiImage.wim", 2);
            SetImageInfoTemplate("MultiImage.wim", 3);
        }

        public static void SetImageInfoTemplate(string wimFileName, int imageIndex)
        {
            string srcWim = Path.Combine(TestSetup.SampleDir, wimFileName);
            string destWim = Path.GetTempFileName();
            try
            {
                File.Copy(srcWim, destWim, true);

                using (Wim wim = Wim.OpenWim(destWim, OpenFlags.WriteAccess))
                {
                    string imageName = wim.GetImageName(imageIndex);
                    string imageDesc = wim.GetImageDescription(imageIndex);
                    string imageFlags = wim.GetImageProperty(imageIndex, "FLAGS");

                    Assert.IsNotNull(imageName);
                    Assert.IsNull(imageDesc);
                    Assert.IsNull(imageFlags);

                    wim.SetImageName(imageIndex, "NEW_IMAGE");
                    wim.SetImageDescription(imageIndex, "NEW_DESCRIPTION");
                    wim.SetImageFlags(imageIndex, "NEW_FLAGS");

                    Assert.IsFalse(imageName.Equals(wim.GetImageName(imageIndex), StringComparison.Ordinal));
                    Assert.IsTrue("NEW_IMAGE".Equals(wim.GetImageName(imageIndex), StringComparison.Ordinal));
                    Assert.IsTrue("NEW_DESCRIPTION".Equals(wim.GetImageDescription(imageIndex)));
                    Assert.IsTrue("NEW_FLAGS".Equals(wim.GetImageProperty(imageIndex, "FLAGS")));
                }
            }
            finally
            {
                if (File.Exists(destWim))
                    File.Delete(destWim);
            }
        }
        #endregion

        #region SetWimInfo
        [TestMethod]
        public void SetWimInfo()
        {
            SetWimInfoTemplate("MultiImage.wim", 2u);
        }

        public static void SetWimInfoTemplate(string wimFileName, uint bootIndex)
        {
            string srcWim = Path.Combine(TestSetup.SampleDir, wimFileName);
            string destWim = Path.GetTempFileName();
            try
            {
                File.Copy(srcWim, destWim, true);

                using (Wim wim = Wim.OpenWim(destWim, OpenFlags.WriteAccess))
                {
                    WimInfo info = new WimInfo
                    {
                        BootIndex = bootIndex,
                    };

                    wim.SetWimInfo(info, ChangeFlags.BootIndex);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
                }

                using (Wim wim = Wim.OpenWim(destWim, OpenFlags.None))
                {
                    WimInfo info = wim.GetWimInfo();

                    Assert.IsTrue(info.BootIndex == bootIndex);
                }
            }
            finally
            {
                if (File.Exists(destWim))
                    File.Delete(destWim);
            }
        }
        #endregion
    }
}


