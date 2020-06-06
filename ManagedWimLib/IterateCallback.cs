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

using System;
using System.Runtime.InteropServices;

namespace ManagedWimLib
{
    #region IterateDirTreeCallback
    /// <summary>
    /// Type of a callback function to <see cref="Wim.IterateDirTree()"/>.
    /// Must return <see cref="Wim.IterateCallbackSuccess"/> (0) on success. 
    /// 
    /// Use negative integer for custom non-sucess return value.
    /// <see cref="Wim.IterateDirTree()"/> may return positive integer when the error occured,
    /// and it is hard to distinct it from user-returned positive integer.
    /// </summary>
    public delegate int IterateDirTreeCallback(DirEntry dentry, object userData);

    internal class ManagedIterateDirTreeCallback
    {
        private readonly IterateDirTreeCallback _callback;
        private readonly object _userData;

        internal WimLibLoader.NativeIterateDirTreeCallback NativeFunc { get; }

        public ManagedIterateDirTreeCallback(IterateDirTreeCallback callback, object userData)
        {
            _callback = callback;
            _userData = userData;

            // Avoid GC by keeping ref here
            NativeFunc = NativeCallback;
        }

        private int NativeCallback(IntPtr entryPtr, IntPtr userCtx)
        {
            if (_callback == null)
                return Wim.IterateCallbackSuccess; // Default return value is a value represents Success/Continue.

            int ret;
            DirEntryBase b = Marshal.PtrToStructure<DirEntryBase>(entryPtr);
            DirEntry dentry = new DirEntry
            {
                FileName = b.FileName,
                DosName = b.DosName,
                FullPath = b.FullPath,
                Depth = b.Depth,
                SecurityDescriptor = b.SecurityDescriptor,
                Attributes = b.Attributes,
                ReparseTag = b.ReparseTag,
                NumLinks = b.NumLinks,
                NumNamedStreams = b.NumNamedStreams,
                HardLinkGroupId = b.HardLinkGroupId,
                CreationTime = b.CreationTime,
                LastWriteTime = b.LastWriteTime,
                LastAccessTime = b.LastAccessTime,
                UnixUserId = b.UnixUserId,
                UnixGroupId = b.UnixGroupId,
                UnixMode = b.UnixMode,
                UnixRootDevice = b.UnixRootDevice,
                ObjectId = b.ObjectId,
                Streams = new StreamEntry[b.NumNamedStreams + 1],
            };

            IntPtr baseOffset = IntPtr.Add(entryPtr, Marshal.SizeOf<DirEntryBase>());
            for (int i = 0; i < dentry.Streams.Length; i++)
            {
                IntPtr offset = IntPtr.Add(baseOffset, i * Marshal.SizeOf<StreamEntry>());
                dentry.Streams[i] = Marshal.PtrToStructure<StreamEntry>(offset);
            }

            ret = _callback(dentry, _userData);

            return ret;
        }
    }
    #endregion

    #region IterateLookupTableCallback
    /// <summary>
    /// Type of a callback function to <see cref="Wim.IterateLookupTable()"/>.
    /// Must return <see cref="Wim.IterateCallbackSuccess"/> (0) on success. 
    /// </summary>
    public delegate int IterateLookupTableCallback(ResourceEntry resource, object userCtx);

    internal class ManagedIterateLookupTableCallback
    {
        private readonly IterateLookupTableCallback _callback;
        private readonly object _userData;

        internal WimLibLoader.NativeIterateLookupTableCallback NativeFunc { get; }

        public ManagedIterateLookupTableCallback(IterateLookupTableCallback callback, object userData)
        {
            _callback = callback;
            _userData = userData;

            // Avoid GC by keeping ref here
            NativeFunc = NativeCallback;
        }

        private int NativeCallback(ResourceEntry resource, IntPtr userCtx)
        {
            if (_callback == null)
                return Wim.IterateCallbackSuccess;

            return _callback(resource, _userData);
        }
    }
    #endregion
}
