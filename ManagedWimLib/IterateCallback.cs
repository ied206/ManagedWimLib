/*
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

using System;
using System.Runtime.InteropServices;

namespace ManagedWimLib
{
    #region IterateDirTreeCallback
    /// <summary>
    /// Type of a callback function to wimlib_iterate_dir_tree().  Must return 0 on success.
    /// </summary>
    public delegate CallbackStatus IterateDirTreeCallback(DirEntry dentry, object userData);

    public class ManagedIterateDirTreeCallback
    {
        private readonly IterateDirTreeCallback _callback;
        private readonly object _userData;

        internal NativeMethods.NativeIterateDirTreeCallback NativeFunc { get; }

        public ManagedIterateDirTreeCallback(IterateDirTreeCallback callback, object userData)
        {
            _callback = callback;
            _userData = userData;

            // Avoid GC by keeping ref here
            NativeFunc = NativeCallback;
        }

        private CallbackStatus NativeCallback(IntPtr entryPtr, IntPtr userCtx)
        {
            CallbackStatus ret = CallbackStatus.CONTINUE;
            if (_callback == null)
                return ret;
            
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
    /// Type of a callback function to wimlib_iterate_lookup_table().  Must return 0 on success.
    /// </summary>
    public delegate CallbackStatus IterateLookupTableCallback(ResourceEntry resoure, object userCtx);

    public class ManagedIterateLookupTableCallback
    {
        private readonly IterateLookupTableCallback _callback;
        private readonly object _userData;

        internal NativeMethods.NativeIterateLookupTableCallback NativeFunc { get; }

        public ManagedIterateLookupTableCallback(IterateLookupTableCallback callback, object userData)
        {
            _callback = callback;
            _userData = userData;

            // Avoid GC by keeping ref here
            NativeFunc = NativeCallback;
        }

        private CallbackStatus NativeCallback(ResourceEntry resource, IntPtr userCtx)
        {
            if (_callback == null)
                return CallbackStatus.CONTINUE;
            
            return _callback(resource, _userData);
        }
    }
    #endregion
}
