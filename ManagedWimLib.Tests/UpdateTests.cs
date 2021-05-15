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
    public class UpdateTests
    {
        #region Update
        [TestMethod]
        public void Update()
        {
            string sampleDir = Path.Combine(TestSetup.SampleDir);

            UpdateTemplate("XPRESS.wim", new UpdateCommand[2]
            {
                UpdateCommand.SetAdd(Path.Combine(sampleDir, "Append01", "Z.txt"), "ADD", null, AddFlags.None),
                UpdateCommand.SetAdd(Path.Combine(sampleDir, "Src03", "가"), "유니코드", null, AddFlags.None),
            });

            UpdateTemplate("LZX.wim", new UpdateCommand[2]
            {
                UpdateCommand.SetDelete("ACDE.txt", DeleteFlags.None),
                UpdateCommand.SetDelete("ABCD", DeleteFlags.Recursive),
            });

            UpdateTemplate("LZMS.wim", new UpdateCommand[2]
            {
                UpdateCommand.SetRename("ACDE.txt", "FILE"),
                UpdateCommand.SetRename("ABCD", "DIR"),
            });
        }

        public static CallbackStatus UpdateProgressCallback(ProgressMsg msg, object info, object progctx)
        {
            CallbackTested tested = progctx as CallbackTested;
            Assert.IsNotNull(tested);

            switch (msg)
            {
                case ProgressMsg.UpdateBeginCommand:
                case ProgressMsg.UpdateEndCommand:
                    {
                        UpdateProgress m = (UpdateProgress)info;
                        Assert.IsNotNull(m);

                        tested.Set();

                        UpdateCommand cmd = m.Command;
                        Console.WriteLine($"Commands = {m.CompletedCommands}/{m.TotalCommands}");
                        switch (cmd.Op)
                        {
                            case UpdateOp.Add:
                                {
                                    AddCommand add = cmd.Add;
                                    Console.WriteLine($"ADD [{add.FsSourcePath}] -> [{add.WimTargetPath}]");
                                }
                                break;
                            case UpdateOp.Delete:
                                {
                                    DeleteCommand del = cmd.Delete;
                                    Console.WriteLine($"DELETE [{del.WimPath}]");
                                }
                                break;
                            case UpdateOp.Rename:
                                {
                                    RenameCommand ren = cmd.Rename;
                                    Console.WriteLine($"RENAME [{ren.WimSourcePath}] -> [{ren.WimTargetPath}]");
                                }
                                break;
                        }
                    }
                    break;
            }

            return CallbackStatus.Continue;
        }

        public void UpdateTemplate(string fileName, UpdateCommand[] cmds)
        {
            string destDir = TestHelper.GetTempDir();
            try
            {
                CallbackTested tested = new CallbackTested(false);
                Directory.CreateDirectory(destDir);

                string srcWimFile = Path.Combine(TestSetup.SampleDir, fileName);
                string destWimFile = Path.Combine(destDir, fileName);
                File.Copy(srcWimFile, destWimFile, true);

                using (Wim wim = Wim.OpenWim(destWimFile, OpenFlags.WriteAccess, UpdateProgressCallback, tested))
                {
                    wim.UpdateImage(1, cmds, UpdateFlags.SendProgress);

                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
                }

                List<string> entries = new List<string>();
                int IterateCallback(DirEntry dentry, object userData)
                {
                    entries.Add(dentry.FullPath);
                    return Wim.IterateCallbackSuccess;
                }

                using (Wim wim = Wim.OpenWim(destWimFile, OpenFlags.None))
                {
                    wim.IterateDirTree(1, Wim.RootPath, IterateDirTreeFlags.Recursive, IterateCallback, entries);
                }

                Assert.IsTrue(tested.Value);
                foreach (UpdateCommand cmd in cmds)
                {
                    switch (cmd.Op)
                    {
                        case UpdateOp.Add:
                            {
                                AddCommand add = cmd.Add;
                                Assert.IsTrue(entries.Contains(Path.Combine(Wim.RootPath, add.WimTargetPath), StringComparer.Ordinal));
                            }
                            break;
                        case UpdateOp.Delete:
                            {
                                DeleteCommand del = cmd.Delete;
                                Assert.IsFalse(entries.Contains(Path.Combine(Wim.RootPath, del.WimPath), StringComparer.Ordinal));
                            }
                            break;
                        case UpdateOp.Rename:
                            {
                                RenameCommand ren = cmd.Rename;
                                Assert.IsTrue(entries.Contains(Path.Combine(Wim.RootPath, ren.WimTargetPath), StringComparer.Ordinal));
                            }
                            break;
                    }
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
