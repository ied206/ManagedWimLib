﻿/*
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

using Joveler.DynLoader;
using System;
using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable 649
#pragma warning disable IDE0044

namespace ManagedWimLib
{
    #region ProgressCallback delegate
    public delegate CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx);
    #endregion

    #region ManagedWimLibCallback
    internal class ManagedProgressCallback
    {
        private readonly ProgressCallback _callback;
        private readonly object _userData;

        internal WimLibLoader.NativeProgressFunc NativeFunc { get; }

        public ManagedProgressCallback(ProgressCallback callback, object userData)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _userData = userData;

            // Avoid GC by keeping ref here
            NativeFunc = NativeCallback;
        }

        private CallbackStatus NativeCallback(ProgressMsg msgType, IntPtr info, IntPtr progctx)
        {
            object pInfo = null;

            if (_callback == null)
                return CallbackStatus.Continue;

            switch (msgType)
            {
                case ProgressMsg.WriteStreams:
                    pInfo = Marshal.PtrToStructure<WriteStreamsProgress>(info);
                    break;
                case ProgressMsg.ScanBegin:
                case ProgressMsg.ScanDEntry:
                case ProgressMsg.ScanEnd:
                    pInfo = Marshal.PtrToStructure<ScanProgress>(info);
                    break;
                case ProgressMsg.ExtractSpwmPartBegin:
                case ProgressMsg.ExtractImageBegin:
                case ProgressMsg.ExtractTreeBegin:
                case ProgressMsg.ExtractFileStructure:
                case ProgressMsg.ExtractStreams:
                case ProgressMsg.ExtractMetadata:
                case ProgressMsg.ExtractTreeEnd:
                case ProgressMsg.ExtractImageEnd:
                    pInfo = Marshal.PtrToStructure<ExtractProgress>(info);
                    break;
                case ProgressMsg.Rename:
                    pInfo = Marshal.PtrToStructure<RenameProgress>(info);
                    break;
                case ProgressMsg.UpdateBeginCommand:
                case ProgressMsg.UpdateEndCommand:
                    UpdateProgressBase _base = Marshal.PtrToStructure<UpdateProgressBase>(info);
                    pInfo = _base.ToManaged();
                    break;
                case ProgressMsg.VerifyIntegrity:
                case ProgressMsg.CalcIntegrity:
                    pInfo = Marshal.PtrToStructure<IntegrityProgress>(info);
                    break;
                case ProgressMsg.SplitBeginPart:
                case ProgressMsg.SplitEndPart:
                    pInfo = Marshal.PtrToStructure<SplitProgress>(info);
                    break;
                case ProgressMsg.ReplaceFileInWim:
                    pInfo = Marshal.PtrToStructure<ReplaceProgress>(info);
                    break;
                case ProgressMsg.WimBootExclude:
                    pInfo = Marshal.PtrToStructure<WimBootExcludeProgress>(info);
                    break;
                case ProgressMsg.UnmountBegin:
                    pInfo = Marshal.PtrToStructure<UnmountProgress>(info);
                    break;
                case ProgressMsg.DoneWithFile:
                    pInfo = Marshal.PtrToStructure<DoneWithFileProgress>(info);
                    break;
                case ProgressMsg.BeginVerifyImage:
                case ProgressMsg.EndVerifyImage:
                    pInfo = Marshal.PtrToStructure<VerifyImageProgress>(info);
                    break;
                case ProgressMsg.VerifyStreams:
                    pInfo = Marshal.PtrToStructure<VerifyStreamsProgress>(info);
                    break;
                case ProgressMsg.TestFileExclusion:
                    pInfo = Marshal.PtrToStructure<TestFileExclusionProgress>(info);
                    break;
                case ProgressMsg.HandleError:
                    pInfo = Marshal.PtrToStructure<HandleErrorProgress>(info);
                    break;
            }

            return _callback(msgType, pInfo, _userData);
        }
    }
    #endregion

    #region ProgressInfo classes 
    #region WriteStreamsProgress
    /// <summary>
    /// Valid on the message WRITE_STREAMS.  
    /// This is the primary message for tracking the progress of writing a WIM file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class WriteStreamsProgress
    {
        /// <summary>
        /// An upper bound on the number of bytes of file data that will be written.
        /// This number is the uncompressed size; the actual size may be lower due to compression. 
        /// In addition, this number may decrease over time as duplicated file data is discovered.
        /// </summary>
        public ulong TotalBytes;
        /// <summary>
        /// An upper bound on the number of distinct file data "blobs" that will be written. 
        /// This will often be similar to the "number of files", but for several reasons 
        /// (hard links, named data streams, empty files, etc.) it can be different. 
        /// In addition, this number may decrease over time as duplicated file data is discovered.
        /// </summary>
        public ulong TotalStreams;
        /// <summary>
        /// The number of bytes of file data that have been written so far. 
        /// This starts at 0 and ends at TotalBytes.
        /// This number is the uncompressed size; the actual size may be lower due to compression.
        /// See <see cref="CompletedCompressedBytes"/> for the compressed size.
        /// </summary>
        public ulong CompletedBytes;
        /// <summary>
        /// The number of distinct file data "blobs" that have been written so far. 
        /// This starts at 0 and ends at total_streams.
        /// </summary>
        public ulong CompletedStreams;
        /// <summary>
        /// The number of threads being used for data compression; or, if no compression is being performed, this will be 1.
        /// </summary>
        public uint NumThreads;
        /// <summary>
        /// The compression type being used, as one of the CompressionType enums. 
        /// </summary>
        public CompressionType CompressionType;
        /// <summary>
        /// The number of on-disk WIM files from which file data is being exported into the output WIM file.
        /// This can be 0, 1, or more than 1, depending on the situation.
        /// </summary>
        public uint TotalParts;
        /// <summary>
        /// This is currently broken and will always be 0. 
        /// </summary>
        public uint CompletedParts;
        /// <summary>
        /// Since wimlib v1.13.4: Like <see cref="CompletedBytes"/>, but counts the compressed size.
        /// </summary>
        public ulong CompletedCompressedBytes;
    }
    #endregion

    #region ScanProgress
    /// <summary>
    /// Dentry scan status, valid on SCAN_DENTRY.
    /// </summary>
    public enum ScanDentryStatus : uint
    {
        /// <summary>
        /// File looks okay and will be captured.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// File is being excluded from capture due to the capture configuration.
        /// </summary>
        Excluded = 1,
        /// <summary>
        /// File is being excluded from capture due to being of an unsupported type. 
        /// </summary>
        Unsupported = 2,
        /// <summary>
        /// The file is an absolute symbolic link or junction that points into the capture directory, and
        /// reparse-point fixups are enabled, so its target is being adjusted. 
        /// (Reparse point fixups can be disabled with the flag <see cref="AddFlags.NoRpFix"/>.)
        /// </summary>
        FixedSymlink = 3,
        /// <summary>
        /// Reparse-point fixups are enabled, but the file is an absolute symbolic link or junction that does not
        /// point into the capture directory, so its target is <b>not</b> being adjusted.
        /// </summary>
        NotFixedSymlink = 4,
    }

    /// <summary>
    /// Valid on messages SCAN_BEGIN, SCAN_DENTRY, and SCAN_END.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ScanProgress
    {
        /// <summary>
        /// Top-level directory being scanned; or, when capturing an NTFS volume with AddFlags.NTFS, 
        /// this is instead the path to the file or block device that contains the NTFS volume being scanned. 
        /// </summary>
        public string Source => Wim.Lib.PtrToStringAuto(_sourcePtr);
        private IntPtr _sourcePtr;
        /// <summary>
        /// Path to the file (or directory) that has been scanned, valid on SCAN_DENTRY.
        /// When capturing an NTFS volume with ::WIMLIB_ADD_FLAG_NTFS, this path will be relative to the root of the NTFS volume. 
        /// </summary>
        public string CurPath => Wim.Lib.PtrToStringAuto(_curPathPtr);
        private IntPtr _curPathPtr;
        /// <summary>
        /// Dentry scan status, valid on SCAN_DENTRY. 
        /// </summary>
        public ScanDentryStatus Status;
        /// <summary>
        /// - wim_target_path
        /// Target path in the image.  Only valid on messages
        /// SCAN_BEGIN and
        /// SCAN_END.
        /// 
        /// - symlink_target
        /// For SCAN_DENTRY and a status of WIMLIB_SCAN_DENTRY_FIXED_SYMLINK or WIMLIB_SCAN_DENTRY_NOT_FIXED_SYMLINK,
        /// this is the target of the absolute symbolic link or junction.
        /// </summary>
        public string WimTargetPathSymlinkTarget => Wim.Lib.PtrToStringAuto(_wimTargetPathSymlinkTargetPtr);
        private IntPtr _wimTargetPathSymlinkTargetPtr;
        /// <summary>
        /// The number of directories scanned so far, not counting excluded/unsupported files.
        /// </summary>
        public ulong NumDirsScanned;
        /// <summary>
        /// The number of non-directories scanned so far, not counting excluded/unsupported files.
        /// </summary>
        public ulong NumNonDirsScanned;
        /// <summary>
        /// The number of bytes of file data detected so far, not counting excluded/unsupported files.
        /// </summary>
        public ulong NumBytesScanned;
    }
    #endregion

    #region ExtractProgress
    /// <summary>
    /// Valid on messages
    /// EXTRACT_SPWM_PART_BEGIN,
    /// EXTRACT_IMAGE_BEGIN,
    /// EXTRACT_TREE_BEGIN,
    /// EXTRACT_FILE_STRUCTURE,
    /// EXTRACT_STREAMS,
    /// EXTRACT_METADATA,
    /// EXTRACT_TREE_END, and
    /// EXTRACT_IMAGE_END.
    ///
    /// Note: most of the time of an extraction operation will be spent extracting file data, and the application will receive
    /// EXTRACT_STREAMS during this time. Using completed_bytes and @p total_bytes, the application can calculate a
    /// percentage complete.  However, there is no way for applications to know which file is currently being extracted.
    /// This is by design because the best way to complete the extraction operation is not necessarily file-by-file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ExtractProgress
    {
        /// <summary>
        /// The 1-based index of the image from which files are being extracted.
        /// </summary>
        public uint Image;
        /// <summary>
        /// Extraction flags being used. 
        /// </summary>
        public uint ExtractFlags;
        /// <summary>
        /// If the WimStruct from which the extraction being performed has a backing file, 
        /// then this is an absolute path to that backing file. Otherwise, this is null.
        /// </summary>
        public string WimFileName => Wim.Lib.PtrToStringAuto(_wimFileNamePtr);
        private IntPtr _wimFileNamePtr;
        /// <summary>
        /// Name of the image from which files are being extracted, or the empty string if the image is unnamed.
        /// </summary>
        public string ImageName => Wim.Lib.PtrToStringAuto(_imageNamePtr);
        private IntPtr _imageNamePtr;
        /// <summary>
        /// Path to the directory or NTFS volume to which the files are being extracted.
        /// </summary>
        public string Target => Wim.Lib.PtrToStringAuto(_targetPtr);
        private IntPtr _targetPtr;
        /// <summary>
        /// Reserved.
        /// </summary>
        private IntPtr _reserved;
        /// <summary>
        /// The number of bytes of file data that will be extracted. 
        /// </summary>
        public ulong TotalBytes;
        /// <summary>
        /// The number of bytes of file data that have been extracted so far.
        /// This starts at 0 and ends at TotalBytes.
        /// </summary>
        public ulong CompletedBytes;
        /// <summary>
        /// The number of file streams that will be extracted. This will often be similar to the "number of files", 
        /// but for several reasons (hard links, named data streams, empty files, etc.) it can be different.
        /// </summary>
        public ulong TotalStreams;
        /// <summary>
        /// The number of file streams that have been extracted so far.
        /// This starts at 0 and ends at @p total_streams.
        /// </summary>
        public ulong CompletedStreams;
        /// <summary>
        /// Currently only used for
        /// EXTRACT_SPWM_PART_BEGIN. 
        /// </summary>
        public uint PartNumber;
        /// <summary>
        /// Currently only used for
        /// EXTRACT_SPWM_PART_BEGIN.
        /// </summary>
        public uint TotalParts;
        /// <summary>
        /// Currently only used for
        /// EXTRACT_SPWM_PART_BEGIN.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Guid;
        /// <summary>
        /// For EXTRACT_FILE_STRUCTURE and EXTRACT_METADATA messages, 
        /// this is the number of files that have been processed so far.
        /// Once the corresponding phase of extraction is complete, this value will be equal to EndFileCount. 
        /// </summary>
        public ulong CurrentFileCount;
        /// <summary>
        /// For EXTRACT_FILE_STRUCTURE and EXTRACT_METADATA messages, 
        /// this is total number of files that will be processed.
        /// 
        /// This number is provided for informational purposes only, e.g. for a progress bar. 
        /// This number will not necessarily be equal to the number of files actually being extracted.
        /// This is because extraction backends are free to implement an extraction algorithm that might be more efficient than
        /// processing every file in the "extract file structure" and "extract file metadata" phases.
        /// For example, the current implementation of the UNIX extraction backend will create
        /// files on-demand during the "extract file data" phase.
        /// Therefore, when using that particular extraction backend, EndFileCount will only include directories and empty files.
        /// </summary>
        public ulong EndFileCount;
    }
    #endregion

    #region RenameProgress
    /// <summary>
    /// Valid on messages RENAME.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RenameProgress
    {
        /// <summary>
        /// Name of the temporary file that the WIM was written to.
        /// </summary>
        public string From => Wim.Lib.PtrToStringAuto(_fromPtr);
        private IntPtr _fromPtr;
        /// <summary>
        /// Name of the original WIM file to which the temporary file is
        /// being renamed.
        /// </summary>
        public string To => Wim.Lib.PtrToStringAuto(_toPtr);
        private IntPtr _toPtr;
    }
    #endregion

    #region UpdateProgress
    /// <summary>
    /// Valid on messages UPDATE_BEGIN_COMMAND and UPDATE_END_COMMAND.
    /// </summary>
    /// <remarks>
    /// Wrapper of ProgressInfo_UpdateBase
    /// </remarks>
    public class UpdateProgress
    {
        /// <summary>
        /// Name of the temporary file that the WIM was written to.
        /// </summary>
        public UpdateCommand Command;
        /// <summary>
        /// Number of update commands that have been completed so far.
        /// </summary>
        public ulong CompletedCommands => CompletedCommandsVal.ToUInt64();
        internal UIntPtr CompletedCommandsVal;
        /// <summary>
        /// Number of update commands that are being executed as part of this call to Wim.UpdateImage().
        /// </summary>
        public ulong TotalCommands => TotalCommandsVal.ToUInt64();
        internal UIntPtr TotalCommandsVal;
    }

    /// <summary>
    /// Valid on messages UPDATE_BEGIN_COMMAND and UPDATE_END_COMMAND.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class UpdateProgressBase
    {
        /// <summary>
        /// Name of the temporary file that the WIM was written to.
        /// </summary>
        private IntPtr _cmdPtr;
        private UpdateCommand32 Cmd32 => Marshal.PtrToStructure<UpdateCommand32>(_cmdPtr);
        private UpdateCommand64 Cmd64 => Marshal.PtrToStructure<UpdateCommand64>(_cmdPtr);

        public UpdateCommand Command
        {
            get
            {
                return (Wim.Lib.PlatformBitness) switch
                {
                    PlatformBitness.Bit32 => Cmd32.ToManagedClass(),
                    PlatformBitness.Bit64 => Cmd64.ToManagedClass(),
                    _ => throw new PlatformNotSupportedException(),
                };
            }
        }
        /// <summary>
        /// Number of update commands that have been completed so far.
        /// </summary>
        public UIntPtr CompletedCommandsVal;
        /// <summary>
        /// Number of update commands that are being executed as part of this call to Wim.UpdateImage().
        /// </summary>
        public UIntPtr TotalCommandsVal;

        public UpdateProgress ToManaged()
        {
            return new UpdateProgress
            {
                Command = this.Command,
                CompletedCommandsVal = this.CompletedCommandsVal,
                TotalCommandsVal = this.TotalCommandsVal,
            };
        }
    }
    #endregion

    #region IntegrityProgress
    /// <summary>
    /// Valid on messages VERIFY_INTEGRITY and CALC_INTEGRITY.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class IntegrityProgress
    {
        /// <summary>
        /// The number of bytes in the WIM file that are covered by integrity checks.
        /// </summary>
        public ulong TotalBytes;
        /// <summary>
        /// The number of bytes that have been checksummed so far.
        /// This starts at 0 and ends at TotalBytes.
        /// </summary>
        public ulong CompletedBytes;
        /// <summary>
        /// The number of individually checksummed "chunks" the integrity-checked region is divided into.
        /// </summary>
        public uint TotalChunks;
        /// <summary>
        /// The number of chunks that have been checksummed so far.
        /// This starts at 0 and ends at TotalChunks.
        /// </summary>
        public uint CompletedChunks;
        /// <summary>
        /// The size of each individually checksummed "chunk" in the integrity-checked region.
        /// </summary>
        public uint ChunkSize;
        /// <summary>
        /// For VERIFY_INTEGRITY messages, this is the path to the WIM file being checked.
        /// </summary>
        public string FileName => Wim.Lib.PtrToStringAuto(_fileNamePtr);
        private IntPtr _fileNamePtr;
    }
    #endregion

    #region SplitProgress
    /// <summary>
    /// Valid on messages SPLIT_BEGIN_PART and SPLIT_END_PART.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SplitProgress
    {
        /// <summary>
        /// Total size of the original WIM's file and metadata resources (compressed).
        /// </summary>
        public ulong TotalBytes;
        /// <summary>
        /// Number of bytes of file and metadata resources that have been copied out of the original WIM so far.
        /// Will be 0 initially, and equal to TotalBytes at the end.
        /// </summary>
        public ulong CompletedBytes;
        /// <summary>
        /// Number of the split WIM part that is about to be started (SPLIT_BEGIN_PART) or has just been finished (SPLIT_END_PART).
        /// </summary>
        public uint CurPartNumber;
        /// <summary>
        /// Total number of split WIM parts that are being written.
        /// </summary>
        public uint TotalParts;
        /// <summary>
        /// Name of the split WIM part that is about to be started (SPLIT_BEGIN_PART) or has just been finished (SPLIT_END_PART).
        /// Since wimlib v1.7.0, the library user may change this when receiving SPLIT_BEGIN_PART in order to
        /// cause the next split WIM part to be written to a different location.
        /// </summary>
        public string PartName => Wim.Lib.PtrToStringAuto(_partNamePtr);
        private IntPtr _partNamePtr;
    }
    #endregion

    #region ReplaceProgress
    /// <summary>
    /// Valid on messages REPLACE_FILE_IN_WIM
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ReplaceProgress
    {
        /// <summary>
        /// Path to the file in the image that is being replaced.
        /// </summary>
        public string PathInWim => Wim.Lib.PtrToStringAuto(_pathInWimPtr);
        private IntPtr _pathInWimPtr;
    }
    #endregion

    #region WimBootExcludeProgress
    /// <summary>
    /// Valid on messages WIMBOOT_EXCLUDE 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class WimBootExcludeProgress
    {
        /// <summary>
        /// Path to the file in the image.
        /// </summary>
        public string PathInWim => Wim.Lib.PtrToStringAuto(_pathInWimPtr);
        private IntPtr _pathInWimPtr;
        /// <summary>
        /// Path to which the file is being extracted .
        /// </summary>
        public string ExtractionInWim => Wim.Lib.PtrToStringAuto(_extractionInWimPtr);
        private IntPtr _extractionInWimPtr;
    }
    #endregion

    #region UnmountProgress
    /// <summary>
    /// Valid on messages UNMOUNT_BEGIN.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class UnmountProgress
    {
        /// <summary>
        /// Path to directory being unmounted.
        /// </summary>
        public string MountPoint => Wim.Lib.PtrToStringAuto(_mountPointPtr);
        private IntPtr _mountPointPtr;
        /// <summary>
        /// Path to WIM file being unmounted.
        /// </summary>
        public string MountedWim => Wim.Lib.PtrToStringAuto(_mountedWimPtr);
        private IntPtr _mountedWimPtr;
        /// <summary>
        /// 1-based index of image being unmounted.
        /// </summary>
        public uint MountedImage;
        /// <summary>
        /// Flags that were passed to Wim.MountImage() when the mountpoint was set up.
        /// </summary>
        public uint MountFlags;
        /// <summary>
        /// Flags passed to Wim.MountImage().
        /// </summary>
        public uint UnmountFlags;
    }
    #endregion

    #region DoneWithFileProgress
    /// <summary>
    /// Valid on messages DONE_WITH_FILE.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DoneWithFileProgress
    {
        /// <summary>
        /// Path to the file whose data has been written to the WIM file,
        /// or is currently being asynchronously compressed in memory,
        /// and therefore is no longer needed by wimlib.
        /// </summary>
        /// <remarks>
        /// WARNING: The file data will not actually be accessible in the WIM file until the WIM file has been completely written.
        /// Ordinarily you should not treat this message as a green light to go ahead and delete the specified file, since
        /// that would result in data loss if the WIM file cannot be successfully created for any reason.
        ///
        /// If a file has multiple names (hard links), DONE_WITH_FILE will only be received for one name.
        /// Also, this message will not be received for empty files or reparse points (or symbolic links),
        /// unless they have nonempty named data streams.
        /// </remarks>
        public string PathToFile => Wim.Lib.PtrToStringAuto(_pathToFilePtr);
        private IntPtr _pathToFilePtr;
    }
    #endregion

    #region VerifyImageProgress
    /// <summary>
    /// Valid on messages BEGIN_VERIFY_IMAGE and END_VERIFY_IMAGE. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class VerifyImageProgress
    {
        public string WimFile => Wim.Lib.PtrToStringAuto(_wimFilePtr);
        private IntPtr _wimFilePtr;
        public uint TotalImages;
        public uint CurrentImage;
    }
    #endregion

    #region VerifyStreamsProgress
    /// <summary>
    /// Valid on messages VERIFY_STREAMS.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class VerifyStreamsProgress
    {
        public string WimFile => Wim.Lib.PtrToStringAuto(_wimFilePtr);
        private IntPtr _wimFilePtr;
        public ulong TotalStreams;
        public ulong TotalBytes;
        public ulong CurrentStreams;
        public ulong CurrentBytes;
    }
    #endregion

    #region TestFileExclusionProgress
    /// <summary>
    /// Valid on messages TEST_FILE_EXCLUSION.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TestFileExclusionProgress
    {
        /// <summary>
        /// Path to the file for which exclusion is being tested.
        ///
        /// UNIX capture mode:  The path will be a standard relative or absolute UNIX filesystem path.
        /// NTFS-3G capture mode:  The path will be given relative to the root of the NTFS volume, with a leading slash.
        /// Windows capture mode:  The path will be a Win32 namespace path to the file.
        /// </summary>
        public string Path => Wim.Lib.PtrToStringAuto(_pathPtr);
        private IntPtr _pathPtr;
        /// <summary>
        /// Indicates whether the file or directory will be excluded from capture or not. 
        /// This will be false by default.
        /// The progress function can set this to true if it decides that the file needs to be excluded.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool WillExclude;
    }
    #endregion

    #region HandleErrorProgress
    /// <summary>
    /// Valid on messages HANDLE_ERROR. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class HandleErrorProgress
    {
        /// <summary>
        /// Path to the file for which the error occurred, or NULL if not relevant.
        /// </summary>
        public string Path => Wim.Lib.PtrToStringAuto(_pathPtr);
        private IntPtr _pathPtr;
        /// <summary>
        /// The wimlib error code associated with the error.
        /// </summary>
        public int ErrorCode;
        /// <summary>
        /// Indicates whether the error will be ignored or not.
        /// This will be false by default; the progress function may set it to true.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool WillIgnore;
    }
    #endregion
    #endregion
}
