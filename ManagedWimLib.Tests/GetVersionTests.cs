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
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class GetVersionTests
    {
        #region GetVersion
        [TestMethod]
        public void GetVersion()
        {
            Version ver = Wim.Version;
            Console.WriteLine($"Wim.Version: {ver}");

            string str = Wim.VersionStr;
            Console.WriteLine($"Wim.VersionStr: {str}");
        }
        #endregion

        #region LogEnvironment
        [TestMethod]
        public void LogEnvironment()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine($"OS = {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}");
            b.AppendLine($"Dotnet Runtime = {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine(b.ToString());
        }
        #endregion
    }
}
