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
using System.IO;
using System.Linq;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class VerifyTests
    {
        #region Verify
        [TestMethod]
        public void Verify()
        {
            VerifyTemplate("VerifySuccess.wim", true);
            VerifyTemplate("VerifyFail.wim", false);
            VerifySplitTemplate("Split.swm", "Split*.swm", true);
        }

        public void VerifyTemplate(string wimFileName, bool result)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);

            bool[] _checked = new bool[3];
            for (int i = 0; i < _checked.Length; i++)
                _checked[i] = false;
            CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
            {
                switch (msg)
                {
                    case ProgressMsg.BeginVerifyImage:
                        {
                            VerifyImageProgress m = (VerifyImageProgress)info;
                            Assert.IsNotNull(info);

                            _checked[0] = true;
                        }
                        break;
                    case ProgressMsg.EndVerifyImage:
                        {
                            VerifyImageProgress m = (VerifyImageProgress)info;
                            Assert.IsNotNull(info);

                            _checked[1] = true;
                        }
                        break;
                    case ProgressMsg.VerifyStreams:
                        {
                            VerifyStreamsProgress m = (VerifyStreamsProgress)info;
                            Assert.IsNotNull(info);

                            _checked[2] = true;
                        }
                        break;
                }
                return CallbackStatus.Continue;
            }

            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);

                    wim.VerifyWim();
                }
            }
            catch (WimException)
            {
                if (result)
                    Assert.Fail();
                else
                    return;
            }

            Assert.IsTrue(_checked.All(x => x));
        }

        public void VerifySplitTemplate(string wimFileName, string splitWildcard, bool result)
        {
            string wimFile = Path.Combine(TestSetup.SampleDir, wimFileName);
            string splitWimFiles = Path.Combine(TestSetup.SampleDir, splitWildcard);

            bool[] _checked = new bool[3];
            for (int i = 0; i < _checked.Length; i++)
                _checked[i] = false;
            CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
            {
                switch (msg)
                {
                    case ProgressMsg.BeginVerifyImage:
                        {
                            VerifyImageProgress m = (VerifyImageProgress)info;
                            Assert.IsNotNull(info);

                            _checked[0] = true;
                        }
                        break;
                    case ProgressMsg.EndVerifyImage:
                        {
                            VerifyImageProgress m = (VerifyImageProgress)info;
                            Assert.IsNotNull(info);

                            _checked[1] = true;
                        }
                        break;
                    case ProgressMsg.VerifyStreams:
                        {
                            VerifyStreamsProgress m = (VerifyStreamsProgress)info;
                            Assert.IsNotNull(info);

                            _checked[2] = true;
                        }
                        break;
                }
                return CallbackStatus.Continue;
            }

            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);

                    wim.ReferenceResourceFile(splitWimFiles, RefFlags.GlobEnable | RefFlags.GlobErrOnNoMatch, OpenFlags.None);

                    wim.VerifyWim();
                }
            }
            catch (WimException)
            {
                if (result)
                    Assert.Fail();
                else
                    return;
            }

            Assert.IsTrue(_checked.All(x => x));
        }
        #endregion
    }
}
