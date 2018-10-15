﻿/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2017-2018 Hajin Jang

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

namespace ManagedWimLib.Tests
{
    [TestClass]
    public class GetVersionTests
    {
        #region GetVersion
        [TestMethod]
        [TestCategory("WimLib")]
        public void GetImageInfo()
        {
            Version ver = Wim.GetVersion();
            Assert.AreEqual(new Version(1, 12, 0), ver);

            Tuple<ushort, ushort, ushort> tuple = Wim.GetVersionTuple();
            Assert.AreEqual(new Tuple<ushort, ushort, ushort>(1, 12, 0), tuple);

            string str = Wim.GetVersionString();
            Assert.IsTrue(str.Equals("1.13.0-BETA5", StringComparison.Ordinal));
        }
        #endregion
    }
}