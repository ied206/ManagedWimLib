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
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class GetInfoTests
    {
        #region GetImageInfo
        [TestMethod]
        public void GetImageInfo()
        {
            GetImageInfoTemplate("MultiImage.wim", 1, "Base", null);
            GetImageInfoTemplate("MultiImage.wim", 2, "Changes", null);
            GetImageInfoTemplate("MultiImage.wim", 3, "Delta", null);
        }

        public static void GetImageInfoTemplate(string wimFileName, int imageIndex, string imageName, string imageDesc)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);

            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                Assert.IsTrue(imageName.Equals(wim.GetImageName(imageIndex), StringComparison.Ordinal));
                Assert.IsNull(wim.GetImageDescription(imageIndex));
                Assert.IsNull(wim.GetImageProperty(imageIndex, "DESCRIPTION"));
            }
        }
        #endregion

        #region GetWimInfo
        [TestMethod]
        public void GetWimInfo()
        {
            GetWimInfoTemplate("XPRESS.wim", CompressionType.XPRESS, false);
            GetWimInfoTemplate("LZX.wim", CompressionType.LZX, false);
            GetWimInfoTemplate("LZMS.wim", CompressionType.LZMS, false);
            GetWimInfoTemplate("BootXPRESS.wim", CompressionType.XPRESS, true);
            GetWimInfoTemplate("BootLZX.wim", CompressionType.LZX, true);
        }

        public static void GetWimInfoTemplate(string fileName, CompressionType compType, bool boot)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, fileName);
            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                WimInfo info = wim.GetWimInfo();

                if (boot)
                    Assert.AreEqual(1u, info.BootIndex);
                else
                    Assert.AreEqual(0u, info.BootIndex);
                Assert.AreEqual(1u, info.ImageCount);
                Assert.AreEqual(compType, info.CompressionType);
            }
        }
        #endregion

        #region GetXmlData
        [TestMethod]
        public void GetXmlData()
        {
            GetXmlDataTemplate("XPRESS.wim", @"<WIM><IMAGE INDEX=""1""><NAME>Sample</NAME><DIRCOUNT>5</DIRCOUNT><FILECOUNT>10</FILECOUNT><TOTALBYTES>13</TOTALBYTES><HARDLINKBYTES>0</HARDLINKBYTES><CREATIONTIME><HIGHPART>0x01D386AC</HIGHPART><LOWPART>0xFC4DC1F4</LOWPART></CREATIONTIME><LASTMODIFICATIONTIME><HIGHPART>0x01D386AC</HIGHPART><LOWPART>0xFC4E2202</LOWPART></LASTMODIFICATIONTIME></IMAGE><TOTALBYTES>1411</TOTALBYTES></WIM>");
            GetXmlDataTemplate("LZX.wim", @"<WIM><IMAGE INDEX=""1""><NAME>Sample</NAME><DIRCOUNT>5</DIRCOUNT><FILECOUNT>10</FILECOUNT><TOTALBYTES>13</TOTALBYTES><HARDLINKBYTES>0</HARDLINKBYTES><CREATIONTIME><HIGHPART>0x01D386AD</HIGHPART><LOWPART>0x036C2DDA</LOWPART></CREATIONTIME><LASTMODIFICATIONTIME><HIGHPART>0x01D386AD</HIGHPART><LOWPART>0x036C6899</LOWPART></LASTMODIFICATIONTIME></IMAGE><TOTALBYTES>1239</TOTALBYTES></WIM>");
            GetXmlDataTemplate("LZMS.wim", @"﻿<WIM><IMAGE INDEX=""1""><NAME>Sample</NAME><DIRCOUNT>5</DIRCOUNT><FILECOUNT>10</FILECOUNT><TOTALBYTES>13</TOTALBYTES><HARDLINKBYTES>0</HARDLINKBYTES><CREATIONTIME><HIGHPART>0x01D3A8A8</HIGHPART><LOWPART>0xF8F70E9B</LOWPART></CREATIONTIME><LASTMODIFICATIONTIME><HIGHPART>0x01D3A8A8</HIGHPART><LOWPART>0xF8F7D129</LOWPART></LASTMODIFICATIONTIME></IMAGE><TOTALBYTES>1251</TOTALBYTES></WIM>");
            GetXmlDataTemplate("MultiImage.wim", @"<WIM><IMAGE INDEX=""1""><NAME>Base</NAME><DIRCOUNT>2</DIRCOUNT><FILECOUNT>3</FILECOUNT><TOTALBYTES>3</TOTALBYTES><HARDLINKBYTES>0</HARDLINKBYTES><CREATIONTIME><HIGHPART>0x01D3A74E</HIGHPART><LOWPART>0x97C28976</LOWPART></CREATIONTIME><LASTMODIFICATIONTIME><HIGHPART>0x01D3A74E</HIGHPART><LOWPART>0x97C5056B</LOWPART></LASTMODIFICATIONTIME></IMAGE><IMAGE INDEX=""2""><NAME>Changes</NAME><DIRCOUNT>2</DIRCOUNT><FILECOUNT>3</FILECOUNT><TOTALBYTES>3</TOTALBYTES><HARDLINKBYTES>0</HARDLINKBYTES><CREATIONTIME><HIGHPART>0x01D3A74E</HIGHPART><LOWPART>0xBC468ECE</LOWPART></CREATIONTIME><LASTMODIFICATIONTIME><HIGHPART>0x01D3A74E</HIGHPART><LOWPART>0xBC470453</LOWPART></LASTMODIFICATIONTIME></IMAGE><IMAGE INDEX=""3""><NAME>Delta</NAME><DIRCOUNT>2</DIRCOUNT><FILECOUNT>4</FILECOUNT><TOTALBYTES>4</TOTALBYTES><HARDLINKBYTES>0</HARDLINKBYTES><CREATIONTIME><HIGHPART>0x01D3A74E</HIGHPART><LOWPART>0xC58E4622</LOWPART></CREATIONTIME><LASTMODIFICATIONTIME><HIGHPART>0x01D3A74E</HIGHPART><LOWPART>0xC58E947B</LOWPART></LASTMODIFICATIONTIME></IMAGE><TOTALBYTES>4332</TOTALBYTES></WIM>");
        }

        public static void GetXmlDataTemplate(string fileName, string compXml)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, fileName);
            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                string wimXml = wim.GetXmlData();

                Assert.IsTrue(wimXml.Equals(compXml, StringComparison.InvariantCulture));
            }
        }
        #endregion

        #region IsImageNameInUse
        [TestMethod]
        public void IsImageNameInUse()
        {
            IsImageNameInUseTemplate("LZX.wim", "Sample", true);
            IsImageNameInUseTemplate("LZX.wim", "", false);
            IsImageNameInUseTemplate("LZX.wim", null, false);
            IsImageNameInUseTemplate("MultiImage.wim", "Delta", true);
            IsImageNameInUseTemplate("MultiImage.wim", "", false);
            IsImageNameInUseTemplate("MultiImage.wim", null, false);
            IsImageNameInUseTemplate("MultiImage.wim", "None", false);
            IsImageNameInUseTemplate("MultiImage.wim", "Alpha", false);
        }

        public static void IsImageNameInUseTemplate(string fileName, string imageName, bool expected)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, fileName);
            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                bool ret = wim.IsImageNameInUse(imageName);
                Assert.AreEqual(expected, ret);
            }
        }
        #endregion

        #region ResolveImage
        [TestMethod]
        public void ResolveImage()
        {
            ResolveImageTemplate("LZX.wim", "Sample", 1);
            ResolveImageTemplate("LZX.wim", "1", 1);
            ResolveImageTemplate("LZX.wim", "2", Wim.NoImage);
            ResolveImageTemplate("MultiImage.wim", "Delta", 3);
            ResolveImageTemplate("MultiImage.wim", "Alpha", Wim.NoImage);
            ResolveImageTemplate("MultiImage.wim", "all", Wim.AllImages);
            ResolveImageTemplate("MultiImage.wim", "*", Wim.AllImages);
        }

        public void ResolveImageTemplate(string fileName, string imageNameOrNum, int expected)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, fileName);
            using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
            {
                int imageIndex = wim.ResolveImage(imageNameOrNum);
                Assert.AreEqual(expected, imageIndex);
            }
        }
        #endregion
    }
}
