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

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable EnumUnderlyingTypeIsInt
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

using Joveler.DynLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable 169
#pragma warning disable 414
#pragma warning disable 649
#pragma warning disable IDE0044
#pragma warning disable IDE0052 // 읽지 않은 private 멤버 제거

namespace ManagedWimLib
{
    #region Native wimlib enums
    #region enum CompressionType
    /// <summary>
    /// Specifies a compression type.
    ///
    /// A WIM file has a default compression type, indicated by its file header.
    /// Normally, each resource in the WIM file is compressed with this compression type.
    /// However, resources may be stored as uncompressed; for example,
    /// wimlib may do so if a resource does not compress to less than its original size. 
    /// In addition, a WIM with the new version number of 3584, or "ESD file",
    /// might contain solid resources with different compression types.
    /// </summary>
    public enum CompressionType : int
    {
        /// <summary>
        /// No compression.
        /// <para>This is a valid argument to <see cref="Wim.CreateNewWim(CompressionType)"/> and <see cref="Wim.SetOutputCompressionType(CompressionType)"/>,
        /// but not to the functions in the compression API such as <see cref="Compressor.Compressor.CreateCompressor(CompressionType, uint, uint, CompressorFlags)"/>.</para>
        /// </summary>
        None = 0,
        /// <summary>
        /// The XPRESS compression format.
        /// <para>This format combines Lempel-Ziv factorization with Huffman encoding.
        /// Compression and decompression are both fast. </para>
        /// 
        /// <para>This format supports chunk sizes that are powers of 2 between 2^12 and 2^16, inclusively.</para>
        /// <para>If using <see cref="Compressors.Compressor.Create(CompressionType, ulong, uint, CompressorFlags)"/> to create
        /// an XPRESS compressor directly, the maxBlockSize parameter may be any positive value up to and including 2^16.</para>
        /// </summary>
        XPRESS = 1,
        /// <summary>
        /// The LZX compression format.
        /// <para>This format combines Lempel-Ziv factorization with Huffman encoding, but with more features and complexity than XPRESS.
        /// Compression is slow to somewhat fast, depending on the settings.
        /// Decompression is fast but slower than XPRESS.</para>
        /// 
        /// <para>This format supports chunk sizes that are powers of 2 between 2^15 and 2^21, inclusively.
        /// Note: chunk sizes other than 2^15 are not compatible with the Microsoft implementation.</para>
        /// <para>If using <see cref="Compressors.Compressor.Create(CompressionType, ulong, uint, CompressorFlags)"/> to create
        /// an LZX compressor directly, the maxBlockSize parameter may be any positive value up to and including 2^21.</para>
        /// </summary>
        LZX = 2,
        /// <summary>
        /// The LZMS compression format.
        /// <para>This format combines Lempel-Ziv factorization with adaptive Huffman encoding and range coding.
        /// Compression and decompression are both fairly slow.</para>
        /// 
        /// <para>This format supports chunk sizes that are powers of 2 between 2^15 and 2^30, inclusively.
        /// This format is best used for large chunk sizes.</para>
        /// <para>If using <see cref="Compressors.Compressor.Create(CompressionType, ulong, uint, CompressorFlags)"/> to create
        /// an LZMS compressor directly, the maxBlockSize parameter may be any positive value up to and including 2^30.</para>
        /// </summary>
        LZMS = 3,
    }
    #endregion

    #region enum ProgressMsg
    /// <summary>
    /// Possible values of the first parameter to the user-supplied ProgressFunc callback
    /// </summary>
    public enum ProgressMsg
    {
        /// <summary>
        /// A WIM image is about to be extracted. 
        /// info param will point to <see cref="ExtractProgress"/>.
        /// This message is received once per image for calls to <see cref="Wim.ExtractImage()"/>.
        /// </summary>
        ExtractImageBegin = 0,
        /// <summary>
        /// One or more file or directory trees within a WIM image is about to be extracted.
        /// info param will point to <see cref="ExtractProgress"/>.
        /// This message is received only once per <see cref="Wim.ExtractPaths()"/> and <see cref="Wim.ExtractPathList()"/>, 
        /// since wimlib combines all paths into a single extraction operation for optimization purposes.
        /// </summary>
        ExtractTreeBegin = 1,
        /// <summary>
        /// This message may be sent periodically (not for every file) while files and directories are being created, prior to file data extraction.
        /// info param will point to <see cref="ExtractProgress"/>.
        /// In particular, the CurrentFileCount and EndFileCount members may be used to track the progress of this phase of extraction.
        /// </summary>
        ExtractFileStructure = 3,
        /// <summary>
        /// File data is currently being extracted.
        /// info param will point to <see cref="ExtractProgress"/>. 
        /// This is the main message to track the progress of an extraction operation.
        /// </summary>
        ExtractStreams = 4,
        /// <summary>
        /// Starting to read a new part of a split pipable WIM over the pipe.
        /// info param will point to <see cref="ExtractProgress"/>.
        /// </summary>
        ExtractSpwmPartBegin = 5,
        /// <summary>
        /// This message may be sent periodically (not necessarily for every file) while file and directory metadata is being extracted,
        /// following file data extraction.
        /// info param will point to <see cref="ExtractProgress"/>.
        /// The <see cref="ExtractProgress.CurrentFileCount"/> and <see cref="ExtractProgress.EndFileCount"/> members
        /// may be used to track the progress of this phase of extraction.
        /// </summary>
        ExtractMetadata = 6,
        /// <summary>
        /// The image has been successfully extracted.
        /// info param will point to <see cref="ExtractProgress"/>.
        /// This is paired with <see cref="ExtractImageBegin"/>.
        /// </summary>
        ExtractImageEnd = 7,
        /// <summary>
        /// The files or directory trees have been successfully extracted.
        /// info param will point to <see cref="ExtractProgress"/>.
        /// This is paired with <see cref="ExtractTreeBegin"/>.
        /// </summary>
        ExtractTreeEnd = 8,
        /// <summary>
        /// The directory or NTFS volume is about to be scanned for metadata.
        /// info param will point to <see cref="ScanProgress"/>.
        /// This message is received once per call to <see cref="Wim.AddImage()"/>, 
        /// or once per capture source passed to <see cref="Wim.AddImageMultisource()"/>, 
        /// or once per add command passed to <see cref="Wim.UpdateImage()"/>.
        /// </summary>
        ScanBegin = 9,
        /// <summary>
        /// A directory or file has been scanned. 
        /// info param will point to <see cref="ScanProgress"/>, and its <see cref="ScanProgress.CurPath"/> member will be valid.
        /// This message is only sent if <see cref="AddFlags.Verbose"/> has been specified.
        /// </summary>
        ScanDEntry = 10,
        /// <summary>
        /// The directory or NTFS volume has been successfully scanned.
        /// info param will point to <see cref="ScanProgress"/>.
        /// This is paired with a previous <see cref="ScanBegin"/> message, 
        /// possibly with many intervening <see cref="ScanDEntry"/> messages.
        /// </summary>
        ScanEnd = 11,
        /// <summary>
        /// File data is currently being written to the WIM.
        /// info param will point to <see cref="WriteStreamsProgress"/>.
        /// This message may be received many times while the WIM file is being written or appended
        /// to with <see cref="Wim.Write()"/>, <see cref="Wim.Overwrite()"/>.
        /// </summary>
        WriteStreams = 12,
        /// <summary>
        /// Per-image metadata is about to be written to the WIM file.
        /// info param will not be valid.
        /// </summary>
        WriteMetadataBegin = 13,
        /// <summary>
        /// The per-image metadata has been written to the WIM file. 
        /// info param will not be valid. 
        /// This message is paired with a preceding <see cref="WriteMetadataBegin"/> message.
        /// </summary>
        WriteMetadataEnd = 14,
        /// <summary>
        /// <see cref="Wim.Overwrite()"/> has successfully renamed the temporary file to the original WIM file, 
        /// thereby committing the changes to the WIM file. 
        /// info param will point to <see cref="RenameProgress"/>. 
        /// Note: this message is not received if <see cref="Wim.Overwrite()"/> chose to append to the WIM file in-place.
        /// </summary>
        Rename = 15,
        /// <summary>
        /// The contents of the WIM file are being checked against the integrity table.
        /// info param will point to <see cref="IntegrityProgress"/>.
        /// This message is only received (and may be received many times) when <see cref="Wim.OpenWim()"/> with <see cref="ProgressCallback"/>
        /// is called with the <see cref="OpenFlags.CheckIntegrity"/> flag.
        /// </summary>
        VerifyIntegrity = 16,
        /// <summary>
        /// An integrity table is being calculated for the WIM being written.
        /// info param will point to <see cref="IntegrityProgress"/>.
        /// This message is only received (and may be received many times)
        /// when a WIM file is being written with the flag <see cref="WriteFlags.CheckIntegrity"/>.
        /// </summary>
        CalcIntegrity = 17,
        /// <summary>
        /// A <see cref="Wim.Split()"/> operation is in progress, and a new split part is about to be started.
        /// info param will point to <see cref="SplitProgress"/>.
        /// </summary>
        SplitBeginPart = 19,
        /// <summary>
        /// A <see cref="Wim.Split()"/> operation is in progress, and a split part has been finished.
        /// info param will point to <see cref="SplitProgress"/>.
        /// </summary>
        SplitEndPart = 20,
        /// <summary>
        /// A WIM update command is about to be executed. 
        /// info param will point to <see cref="UpdateProgress"/>. 
        /// This message is received once per update command when <see cref="Wim.UpdateImage()"/> is called with the flag <see cref="UpdateFlags.SendProgress"/>.
        /// </summary>
        UpdateBeginCommand = 21,
        /// <summary>
        /// A WIM update command has been executed. 
        /// info param will point to <see cref="UpdateProgress"/>.
        /// This message is received once per update command when <see cref="Wim.UpdateImage()"/> is called with the flag <see cref="UpdateFlags.SendProgress"/>.
        /// </summary>
        UpdateEndCommand = 22,
        /// <summary>
        /// A file in the image is being replaced as a result of a <see cref="AddCommand"/> without <see cref="AddFlags.NoReplace"/> specified.
        /// info param will point to <see cref="ReplaceProgress"/>.
        /// This is only received when <see cref="AddFlags.Verbose"/> is also specified in the add command.
        /// </summary>
        ReplaceFileInWim = 23,
        /// <summary>
        /// An image is being extracted with <see cref="ExtractFlags.WimBoot"/>, 
        /// and a file is being extracted normally (not as a "WIMBoot pointer file")
        /// due to it matching a pattern in the PrepopulateList section of the configuration file
        /// /Windows/System32/WimBootCompress.ini in the WIM image.
        /// info param will point to <see cref="WimBootExcludeProgress"/>.
        /// </summary>
        WimBootExclude = 24,
        /// <summary>
        /// Starting to unmount an image. 
        /// info param will point to <see cref="UnmountProgress"/>.
        /// </summary>
        UnmountBegin = 25,
        /// <summary>
        /// wimlib has used a file's data for the last time (including all data streams, if it has multiple).
        /// info param will point to <see cref="DoneWithFileProgress"/>.
        /// This message is only received if <see cref="WriteFlags.SendDoneWithFileMessages"/> was provided.
        /// </summary>
        DoneWithFile = 26,
        /// <summary>
        /// <see cref="Wim.VerifyWim()"/> is starting to verify the metadata for an image.
        /// info param will point to <see cref="VerifyImageProgress"/>.
        /// </summary>
        BeginVerifyImage = 27,
        /// <summary>
        /// <see cref="Wim.VerifyWim()"/> has finished verifying the metadata for an image. 
        /// info param will point to <see cref="VerifyImageProgress"/>.
        /// </summary>
        EndVerifyImage = 28,
        /// <summary>
        /// <see cref="Wim.VerifyWim()"/> is verifying file data integrity.
        /// info param will point to <see cref="VerifyStreamsProgress"/>.
        /// </summary>
        VerifyStreams = 29,
        /// <summary>
        /// The progress function is being asked whether a file should be excluded from capture or not. 
        /// info param will point to <see cref="TestFileExclusionProgress"/>.
        /// This is a bidirectional message that allows the progress function to set a flag if the file should be excluded.
        ///
        /// This message is only received if the flag <see cref="AddFlags.TestFileExclusion"/> is used.
        /// This method for file exclusions is independent of the "capture configuration file" mechanism.
        /// </summary>
        TestFileExclusion = 30,
        /// <summary>
        /// An error has occurred and the progress function is being asked whether to ignore the error or not.
        /// info param will point to <see cref="HandleErrorProgress"/>.
        /// This is a bidirectional message.
        /// 
        /// This message provides a limited capability for applications to recover from "unexpected" errors
        /// (i.e. those with no in-library handling policy) arising from the underlying operating system.
        /// Normally, any such error will cause the library to abort the current operation. 
        /// By implementing a handler for this message, the application can instead choose to ignore a given error.
        /// 
        /// Currently, only the following types of errors will result in this progress message being sent:
        /// 
        /// 	- Directory tree scan errors, e.g. from <see cref="Wim.AddImage()"/>
        /// 	- Most extraction errors; currently restricted to the Windows build of the library only.
        /// </summary>
        HandleError = 31,
    }

    /// <summary>
    /// A pointer to this union is passed to the user-supplied ProgressFunc callback.
    /// One (or none) of the structures contained in this union will be applicable for the operation (ProgressMsg) 
    /// indicated in the first argument to the callback.
    /// </summary>
    public enum CallbackStatus : int
    {
        /// <summary>
        /// The operation should be continued. 
        /// This is the normal return value.
        /// </summary>
        Continue = 0,
        /// <summary>
        /// The operation should be aborted.
        /// This will cause the current operation to fail with ErrorCode.AbortedByProgress.
        /// </summary>
        Abort = 1,
    }
    #endregion

    #region enum ErrorCode 
    /// <summary>
    /// Possible values of the error code returned by many functions in wimlib.
    /// See the documentation for each wimlib function to see specifically what error codes can be returned by a given function, and what they mean.
    /// </summary>
    public enum ErrorCode : int
    {
        Success = 0,
        /// <summary>
        /// Another process is currently modifying the WIM file.
        /// </summary>
        AlreadyLocked = 1,
        Decompression = 2,
        /// <summary>
        /// A non-zero status code was returned by fuse_main().
        /// </summary>
        Fuse = 6,
        GlobHadNoMatches = 8,
        /// <summary>
        /// The number of metadata resources found in the WIM did not match the image count specified in the WIM header, 
        /// or the number of &lt;IMAGE&gt; elements in the XML data of the WIM did not match the image count specified in the WIM header.
        /// </summary>
        ImageCount = 10,
        ImageNameCollision = 11,
        InsufficientPrivileges = 12,
        /// <summary>
        /// <see cref="OpenFlags.CheckIntegrity"/> was specified in openFlags, and the WIM file failed the integrity check.
        /// </summary>
        Integrity = 13,
        InvalidCaptureConfig = 14,
        /// <summary>
        /// The library did not recognize the compression chunk size of the WIM as valid for its compression type.
        /// </summary>
        InvalidChunkSize = 15,
        /// <summary>
        /// The library did not recognize the compression type of the WIM.
        /// </summary>
        InvalidCompressionType = 16,
        /// <summary>
        /// The header of the WIM was otherwise invalid.
        /// </summary>
        InvalidHeader = 17,
        /// <summary>
        /// An image does not exist in a <see cref="Wim">.
        /// </summary>
        InvalidImage = 18,
        /// <summary>
        /// <see cref="OpenFlags.CheckIntegrity"/> was specified in openFlags and the WIM contained an integrity table,
        /// but the integrity table was invalid.
        /// </summary>
        InvalidIntegrityTable = 19,
        /// <summary>
        /// The lookup table of the WIM was invalid.
        /// </summary>
        InvalidLookupTableEntry = 20,
        InvalidMetadataResource = 21,
        InvalidOverlay = 23,
        /// <summary>
        /// <see cref="Wim"> was null; or, wimFile was not a nonempty string.
        /// </summary>
        InvalidParam = 24,
        InvalidPartNumber = 25,
        InvalidPipableWim = 26,
        InvalidReparseData = 27,
        InvalidResourceHash = 28,
        InvalidUtf16String = 30,
        InvalidUtf8String = 31,
        IsDirectory = 32,
        /// <summary>
        /// The WIM was a split WIM and <see cref="OpenFlags.ErrorIfSplit"/> was specified in openFlags.
        /// </summary>
        IsSplitWim = 33,
        Link = 35,
        MetadataNotFound = 36,
        /// <summary>
        /// <see cref="MountFlag.READWRITE"/> was specified in mountFlags, but the staging directory could not be created.
        /// </summary>
        MkDir = 37,
        MQueue = 38,
        NoMem = 39,
        NotDir = 40,
        NotEmpty = 41,
        NotARegularFile = 42,
        /// <summary>
        /// The file did not begin with the magic characters that identify a WIM file.
        /// </summary>
        NotAWimFile = 43,
        NotPipable = 44,
        NoFilename = 45,
        Ntfs3G = 46,
        /// <summary>
        /// Failed to open the WIM file for reading. 
        /// Some possible reasons: the WIM file does not exist, or the calling process does not have permission to open it.
        /// </summary>
        Open = 47,
        /// <summary>
        /// Failed to read data from the WIM file.
        /// </summary>
        OpenDir = 48,
        PathDoesNotExist = 49,
        Read = 50,
        Readlink = 51,
        Rename = 52,
        ReparsePointFixupFailed = 54,
        ResourceNotFound = 55,
        ResourceOrder = 56,
        SetAttributes = 57,
        SetReparseData = 58,
        SetSecurity = 59,
        SetShortName = 60,
        SetTimestamps = 61,
        SplitInvalid = 62,
        Stat = 63,
        /// <summary>
        /// Unexpected end-of-file while reading data from the WIM file.
        /// </summary>
        UnexpectedEndOfFile = 65,
        UnicodeStringNotRepresentable = 66,
        /// <summary>
        /// The WIM version number was not recognized. (May be a pre-Vista WIM.)
        /// </summary>
        UnknownVersion = 67,
        Unsupported = 68,
        UnsupportedFile = 69,
        /// <summary>
        /// <see cref="OpenFlags.WriteAccess"/> was specified but the WIM file was considered read-only because of
        /// any of the reasons mentioned in the documentation for the <see cref="OpenFlags.WriteAccess"/> flag.
        /// </summary>
        WimIsReadOnly = 71,
        Write = 72,
        /// <summary>
        /// The XML data of the WIM was invalid.
        /// </summary>
        XML = 73,
        /// <summary>
        /// The WIM cannot be opened because it contains encrypted segments. (It may be a Windows 8+ "ESD" file.)
        /// </summary>
        WimIsEncrypted = 74,
        WimBoot = 75,
        AbortedByProgress = 76,
        UnknownProgressStatus = 77,
        MkNod = 78,
        MountedImageIsBusy = 79,
        NotAMountPoint = 80,
        NotPermittedToUnmount = 81,
        FveLockedVolume = 82,
        UnableToReadCaptureConfig = 83,
        /// <summary>
        /// The WIM file is not complete (e.g. the program which wrote it was terminated before it finished)
        /// </summary>
        WimIsIncomplete = 84,
        CompactionNotPossible = 85,
        /// <summary>
        /// There are currently multiple references to the image as a result of a call to <see cref="Wim.ExportImage()"/>.
        /// Free one before attempting the read-write mount.
        /// </summary>
        ImageHasMultipleReferences = 86,
        DuplicateExportedImage = 87,
        ConcurrentModificationDetected = 88,
        SnapshotFailure = 89,
        InvalidXAttr = 90,
        SetXAttr = 91,
    }
    #endregion

    #region enum IterateDirTreeFlags
    [Flags]
    public enum IterateDirTreeFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// For <see cref="Wim.IterateDirTree()"/>:
        /// Iterate recursively on children rather than just on the specified path.
        /// </summary>
        Recursive = 0x00000001,
        /// <summary>
        /// For <see cref="Wim.IterateDirTree()"/>:
        /// Don't iterate on the file or directory itself; only its children (in the case of a non-empty directory)
        /// </summary>
        Children = 0x00000002,
        /// <summary>
        /// Return <see cref="ErrorCode.ResourceNotFound"/> if any file data blobs needed to fill in the <see cref="ResourceEntry"/>'s for the iteration
        /// cannot be found in the blob lookup table of the <see cref="Wim">.
        /// The default behavior without this flag is to fill in the <see cref="ResourceEntry.SHA1"/> and set <see cref="ResourceEntry.IsMissing"/>" flag.
        /// </summary>
        ResourcesNeeded = 0x00000004,
    }
    #endregion

    #region enum IterateLookupTableFlags
    /// <summary>
    /// Reserved for <see cref="Wim.IterateLookupTable()"/>
    /// </summary>
    [Flags]
    public enum IterateLookupTableFlags : uint
    {
        None = 0x00000000,
    }
    #endregion

    #region enum AddFlags
    [Flags]
    public enum AddFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// UNIX-like systems only:
        /// Directly capture an NTFS volume rather than a generic directory.
        /// This requires that wimlib was compiled with support for libntfs-3g.
        ///
        /// This flag cannot be combined with <see cref="Dereference"/> or <see cref="UnixData"/>.
        ///
        /// Do not use this flag on Windows,
        /// where wimlib already supports all Windows-native filesystems, including NTFS, through the Windows APIs.
        /// </summary>
        Ntfs = 0x00000001,
        /// <summary>
        /// Follow symbolic links when scanning the directory tree.
        /// Currently only supported on UNIX-like systems.
        /// </summary>
        Dereference = 0x00000002,
        /// <summary>
        /// Call the progress function with the message <see cref="ProgressMsg.ScanDEntry"/> when each directory or file has been scanned.
        /// </summary>
        Verbose = 0x00000004,
        /// <summary>
        /// Mark the image being added as the bootable image of the WIM.
        /// This flag is valid only for <see cref="Wim.AddImage()"/> and <see cref="Wim.AddImageMultisource()"/>.
        ///
        /// Note that you can also change the bootable image of a WIM using <see cref="Wim.SetWimInfo()"/>.
        ///
        /// Note: <see cref="Boot"/> does something different from, and independent from, <see cref="WimBoot"/>.
        /// </summary>
        Boot = 0x00000008,
        /// <summary>
        /// UNIX-like systems only:
        /// Store the UNIX owner, group, mode, and device ID (major and minor number) of each file.
        /// In addition, capture special files such as device nodes and FIFOs. 
        /// Since wimlib v1.11.0, on Linux also capture extended attributes.
        /// See the documentation for the "--unix-data" option to wimcapture for more information.
        /// </summary>
        UnixData = 0x00000010,
        /// <summary>
        /// Do not capture security descriptors.
        /// Only has an effect in NTFS-3G capture mode, or in Windows native builds.
        /// </summary>
        NoAcls = 0x00000020,
        /// <summary>
        /// Fail immediately if the full security descriptor of any file or directory cannot be accessed.  
        /// Only has an effect in Windows native builds.
        /// The default behavior without this flag is to first try omitting the SACL from the security descriptor,
        /// then to try omitting the security descriptor entirely.
        /// </summary>
        StrictAcls = 0x00000040,
        /// <summary>
        /// Call the progress function with the message <see cref="ProgressMsg.ScanDEntry"/> when a directory or file is excluded from capture.
        /// This is a subset of the messages provided by <see cref="Verbose"/>.
        /// </summary>
        ExcludeVerbose = 0x00000080,
        /// <summary>
        /// Reparse-point fixups:
        /// Modify absolute symbolic links (and junctions, in the case of Windows) that point inside the directory
        /// being captured to instead be absolute relative to the directory being captured.
        ///
        /// Without this flag, the default is to do reparse-point fixups if WIM_HDR_FLAG_RP_FIX is set in the WIM header
        /// or if this is the first image being added.
        /// </summary>
        RpFix = 0x00000100,
        /// <summary>
        /// Don't do reparse point fixups. See <see cref="RpFix"/>.
        /// </summary>
        NoRpFix = 0x00000200,
        /// <summary>
        /// Do not automatically exclude unsupported files or directories from capture,
        /// such as encrypted files in NTFS-3G capture mode, or device files and FIFOs on
        /// UNIX-like systems when not also using <see cref="UnixData"/>.  
        /// Instead, fail with <see cref="ErrorCode.UnsupportedFile"/> when such a file is encountered.
        /// </summary>
        NoUnsupportedExclude = 0x00000400,
        /// <summary>
        /// Automatically select a capture configuration appropriate for capturing filesystems containing Windows operating systems.
        /// For example, "/pagefile.sys" and "/System Volume Information" will be excluded.
        ///
        /// When this flag is specified, the corresponding config parameter (for <see cref="Wim.AddImage()"/>) or member (for <see cref="Wim.UpdateImage()"/>) must be null.
        /// Otherwise, <see cref="ErrorCode.InvalidParam"/> will be returned.
        ///
        /// Note that the default behavior ---that is, when neither <see cref="WinConfig"/> nor <see cref="WimBoot"/> is specified and config is null---
        /// is to use no capture configuration, meaning that no files are excluded from capture.
        /// </summary>
        WinConfig = 0x00000800,
        /// <summary>
        /// Capture image as "WIMBoot compatible". 
        /// In addition, if no capture configuration file is explicitly specified use the capture configuration file
        /// "$SOURCE/Windows/System32/WimBootCompress.ini" if it exists, where "$SOURCE" is the directory being captured;
        /// or, if a capture configuration file is explicitly specified, use it and also place it at
        /// "/Windows/System32/WimBootCompress.ini" in the WIM image.
        ///
        /// This flag does not, by itself, change the compression type or chunk size.
        /// Before writing the WIM file, you may wish to set the compression format to be the same as that used by WIMGAPI and DISM:
        ///
        /// <see cref="Wim.SetOutputCompressionType(CompressionType.XPRESS)"/>;
        /// <see cref="Wim.SetOutputPackChunkSize(4096)"/>;
        ///
        /// However, "WIMBoot" also works with other XPRESS chunk sizes as well as LZX with 32768 byte chunks.
        ///
        /// Note: AddFlags.WIMBOOT does something different from, and independent from, AddFlags.BOOT.
        ///
        /// Since wimlib v1.8.3, <see cref="WimBoot"/> also causes offline WIM-backed files to be added as the "real" files
        /// rather than as their reparse points, provided that their data is already present in the WIM. 
        /// This feature can be useful when updating a backing WIM file in an "offline" state.
        /// </summary>
        WimBoot = 0x00001000,
        /// <summary>
        /// If the add command involves adding a non-directory file to a location at which there already exists
        /// a nondirectory file in the image, issue <see cref="ErrorCode.InvalidOverlay"/> instead of replacing the file.
        /// This was the default behavior before wimlib v1.7.0.
        /// </summary>
        NoReplace = 0x00002000,
        /// <summary>
        /// Send <see cref="ProgressMsg.TestFileExclusion"/> messages to the progress function.
        ///
        /// Note: This method for file exclusions is independent from the capture configuration file mechanism.
        /// </summary>
        TestFileExclusion = 0x00004000,
        /// <summary>
        /// Since wimlib v1.9.0: create a temporary filesystem snapshot of the source directory and add the files from it.
        /// Currently, this option is only supported on Windows, where it uses the Volume Shadow Copy Service (VSS).
        /// Using this option, you can create a consistent backup of the system volume of
        /// a running Windows system without running into problems with locked files.
        /// For the VSS snapshot to be successfully created, your application must be run as an Administrator, 
        /// and it cannot be run in WoW64 mode (i.e. if Windows is 64-bit, then your application must be 64-bit as well).
        /// </summary>
        Snapshot = 0x00008000,
        /// <summary>
        /// Since wimlib v1.9.0: permit the library to discard file paths after the initial scan. 
        /// If the application won't use <see cref="WriteFlags.SendDoneWithFileMessages"/> while writing the WIM archive, 
        /// this flag can be used to allow the library to enable optimizations such as opening files by inode number rather than by path.
        /// Currently this only makes a difference on Windows.
        /// </summary>
        FilePathsUnneeded = 0x00010000,
    }
    #endregion

    #region enum ChangeFlags
    [Flags]
    public enum ChangeFlags : int
    {
        None = 0x00000000,
        /// <summary>
        /// Set or unset the "readonly" WIM header flag (WIM_HDR_FLAG_READONLY in Microsoft's documentation),
        /// based on the <see cref="WimInfo.IsMarkedReadOnly"/> member of the info parameter.
        /// This is distinct from basic file permissions; this flag can be set on a WIM file that is physically writable.
        ///
        /// wimlib disallows modifying on-disk WIM files with the readonly flag set.
        /// However, <see cref="Wim.Overwrite()"/> with <see cref="WriteFlags.IgnoreReadOnlyFlag"/> will override this ---
        /// and in fact, this is necessary to set the readonly flag persistently on an existing WIM file.
        /// </summary>
        ReadOnly = 0x00000001,
        /// <summary>
        /// Set the GUID (globally unique identifier) of the WIM file to the value specified in <see cref="WimInfo.Guid"/> of the info parameter.
        /// </summary>
        Guid = 0x00000002,
        /// <summary>
        /// Change the bootable image of the WIM to the value specified in <see cref="WimInfo.BootIndex"/> of the info parameter.
        /// </summary>
        BootIndex = 0x00000004,
        /// <summary>
        /// Change the WIM_HDR_FLAG_RP_FIX flag of the WIM file to the value specified in <see cref="WimInfo.HasRpfix"/> of the info parameter.
        /// This flag generally indicates whether an image in the WIM has been captured with reparse-point fixups enabled.
        /// wimlib also treats this flag as specifying whether to do reparse-point fixups by default
        /// when capturing or applying WIM images.
        /// </summary>
        RpFix = 0x00000008
    }
    #endregion

    #region enum DeleteFlags
    [Flags]
    public enum DeleteFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Do not issue an error if the path to delete does not exist.
        /// </summary>
        Force = 0x00000001,
        /// <summary>
        /// Delete the file or directory tree recursively; if not specified, an error is issued if the path to delete is a directory.
        /// </summary>
        Recursive = 0x00000002,
    }
    #endregion

    #region enum ExportFlags
    [Flags]
    public enum ExportFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// If a single image is being exported, mark it bootable in the destination WIM.
        /// Alternatively, if <see cref="Wim.AllImages"/> is specified as the image to export,
        /// the image in the source WIM (if any) that is marked as bootable is also
        /// marked as bootable in the destination WIM.
        /// </summary>
        Boot = 0x00000001,
        /// <summary>
        /// Give the exported image(s) no names. 
        /// Avoids problems with image name collisions.
        /// </summary>
        NoNames = 0x00000002,
        /// <summary>
        /// Give the exported image(s) no descriptions.
        /// </summary>
        NoDescriptions = 0x00000004,
        /// <summary>
        /// This advises the library that the program is finished with the source
        /// WIMStruct and will not attempt to access it after the call to
        /// <see cref="Wim.ExportImage()"/>, with the exception of the call to <see cref="Wim.Free()"/>.
        /// </summary>
        Gift = 0x00000008,
        /// <summary>
        /// Mark each exported image as WIMBoot-compatible.
        ///
        /// Note: by itself, this does change the destination WIM's compression type, nor
        /// does it add the file "\Windows\System32\WimBootCompress.ini" in the WIM image.  
        /// </summary>
        /// <remarks>
        /// Before writing the destination WIM, it's recommended to do something like:
        ///
        /// using (Wim wim = ...)
        /// {
        ///     wim.SetOutputCompressionType(wim, CompressType.XPRESS);
        ///     wim.SetOutputChunkSize(wim, 4096);
        ///     wim.AddTree(image, "myconfig.ini", @"\Windows\System32\WimBootCompress.ini", AddFlags.DEFAULT);
        /// }
        /// </remarks>
        WimBoot = 0x00000010,
    }
    #endregion

    #region enum ExtractFlags
    [Flags]
    public enum ExtractFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Extract the image directly to an NTFS volume rather than a generic directory.
        /// This mode is only available if wimlib was compiled with libntfs-3g support;
        /// if not, <see cref="ErrorCode.Unsupported"/> will be returned.
        /// In this mode, the extraction target will be interpreted as the path to an NTFS volume image
        /// (as a regular file or block device) rather than a directory.
        /// It will be opened using libntfs-3g, and the image will be extracted to the NTFS filesystem's root directory.
        /// Note: this flag cannot be used when <see cref="Wim.ExtractImage()"/> is called with <see cref="Wim.AllImages"/> as the image,
        /// nor can it be used with <see cref="Wim.ExtractPaths()"/> when passed multiple paths.
        /// </summary>
        Ntfs = 0x00000001,
        /// <summary>
        /// UNIX-like systems only:
        /// Extract UNIX-specific metadata captured with <see cref="AddFlags.UnixData"/>.
        /// </summary>
        UnixData = 0x00000020,
        /// <summary>
        /// Do not extract security descriptors.
        /// This flag cannot be combined with <see cref="StrictAcls"/>.
        /// </summary>
        NoAcls = 0x00000040,
        /// <summary>
        /// Fail immediately if the full security descriptor of any file or directory
        /// cannot be set exactly as specified in the WIM image.
        /// On Windows, the default behavior without this flag when wimlib does not have permission to set the
        /// correct security descriptor is to fall back to setting the security descriptor with the SACL omitted,
        /// then with the DACL omitted, then with the owner omitted, then not at all.
        /// This flag cannot be combined with <see cref="NoAcls"/>.
        /// </summary>
        StrictAcls = 0x00000080,
        /// <summary>
        /// This is the extraction equivalent to <see cref="AddFlags.RpFix"/>.
        /// This forces reparse-point fixups on, so absolute symbolic links or junction points will
        /// be fixed to be absolute relative to the actual extraction root.
        /// Reparse-point fixups are done by default for <see cref="Wim.ExtractImage()"/> and <see cref="Wim.ExtractImageFromPipe()"/>
        /// if WIM_HDR_FLAG_RP_FIX is set in the WIM header.
        /// This flag cannot be combined with <see cref="NoRpFix"/>.
        /// </summary>
        RpFix = 0x00000100,
        /// <summary>
        /// Force reparse-point fixups on extraction off, regardless of the state of the WIM_HDR_FLAG_RP_FIX flag in the WIM header.
        /// This flag cannot be combined with <see cref="RpFix"/>.
        /// </summary>
        NoRpFix = 0x00000200,
        /// <summary>
        /// For <see cref="Wim.ExtractPaths()"/> and <see cref="Wim.ExtractPathList()"/> only:
        /// Extract the paths, each of which must name a regular file, to standard output.
        /// </summary>
        ToStdOut = 0x00000400,
        /// <summary>
        /// Instead of ignoring files and directories with names that cannot be represented on the current platform
        /// (note: Windows has more restrictions on filenames than POSIX-compliant systems),
        /// try to replace characters or append junk to the names so that they can be extracted in some form.
        ///
        /// Note: this flag is unlikely to have any effect when extracting a WIM image that was captured on Windows.
        /// </summary>
        ReplaceInvalidFileNames = 0x00000800,
        /// <summary>
        /// On Windows, when there exist two or more files with the same case insensitive name but different case sensitive names,
        /// try to extract them all by appending junk to the end of them, rather than arbitrarily extracting only one.
        ///
        /// Note: this flag is unlikely to have any effect when extracting a WIM image that was captured on Windows.
        /// </summary>
        AllCaseConflicts = 0x00001000,
        /// <summary>
        /// Do not ignore failure to set timestamps on extracted files.
        /// This flag currently only has an effect when extracting to a directory on UNIX-like systems.
        /// </summary>
        StrictTimestamps = 0x00002000,
        /// <summary>
        /// Do not ignore failure to set short names on extracted files.
        /// This flag currently only has an effect on Windows.
        /// </summary>
        StrictShortNames = 0x00004000,
        /// <summary>
        /// Do not ignore failure to extract symbolic links and junctions due to permissions problems.
        /// This flag currently only has an effect on Windows. 
        /// By default, such failures are ignored since the default configuration of Windows 
        /// only allows the Administrator to create symbolic links.
        /// </summary>
        StrictSymlinks = 0x00008000,
        /// <summary>
        /// For <see cref="Wim.ExtractPaths()"/> and <see cref="Wim.ExtractPathList()"/> only:
        /// Treat the paths to extract as wildcard patterns ("globs") which may contain the wildcard characters '?' and '*'.
        /// The '?' character matches any non-path-separator character, whereas the '*' character matches zero or more
        /// non-path-separator characters.
        /// Consequently, each glob may match zero or more actual paths in the WIM image.
        ///
        /// By default, if a glob does not match any files, a warning but not an error will be issued.
        /// This is the case even if the glob did not actually contain wildcard characters. 
        /// Use <see cref="StrictGlob"/> to get an error instead.
        /// </summary>
        GlobPaths = 0x00040000,
        /// <summary>
        /// In combination with <see cref="GlobPaths"/>, causes an error (<see cref="ErrorCode.PathDoesNotExist"/>)
        /// rather than a warning to be issued when one of the provided globs did not match a file.
        /// </summary>
        StrictGlob = 0x00080000,
        /// <summary>
        /// Do not extract Windows file attributes such as readonly, hidden, etc.
        ///
        /// This flag has an effect on Windows as well as in the NTFS-3G extraction mode.
        /// </summary>
        NoAttributes = 0x00100000,
        /// <summary>
        /// For <see cref="Wim.ExtractPaths()"/> and <see cref="Wim.ExtractPathList()"/> only: 
        /// Do not preserve the directory structure of the archive when extracting --- that is,
        /// place each extracted file or directory tree directly in the target directory.
        /// The target directory will still be created if it does not already exist.
        /// </summary>
        NoPreserveDirStructure = 0x00200000,
        /// <summary>
        /// Windows only: Extract files as "pointers" back to the WIM archive.
        ///
        /// The effects of this option are fairly complex.
        /// See the documentation for the "--wimboot" option of "wimapply" for more information.
        /// </summary>
        WimBoot = 0x00400000,
        /// <summary>
        /// Since wimlib v1.8.2 and Windows-only:
        /// compress the extracted files using System Compression, when possible. 
        /// This only works on either Windows 10 or later, or on an older Windows to which Microsoft's wofadk.sys driver has been added.
        /// Several different compression formats may be used with System Compression;
        /// this particular flag selects the XPRESS compression format with 4096 byte chunks.
        /// </summary>
        CompactXPRESS4K = 0x01000000,
        /// <summary>
        /// Like E<see cref="ExtractFlags.CompactXPRESS4K"/>, but use XPRESS compression with 8192 byte chunks.
        /// </summary>
        CompactXPRESS8K = 0x02000000,
        /// <summary>
        /// Like <see cref="ExtractFlags.CompactXPRESS4K"/>, but use XPRESS compression with 16384 byte chunks.
        /// </summary>
        CompactXPRESS16K = 0x04000000,
        /// <summary>
        /// Like <see cref="ExtractFlags.CompactXPRESS4K"/>, but use LZX compression with 32768 byte chunks.
        /// </summary>
        CompactLZX = 0x08000000,
    }
    #endregion

    #region enum MountFlags (Linux Only)
    [Flags]
    public enum MountFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Mount the WIM image read-write rather than the default of read-only.
        /// </summary>
        ReadWrite = 0x00000001,
        /// <summary>
        /// Enable FUSE debugging by passing the -d option to fuse_main().
        /// </summary>
        Debug = 0x00000002,
        /// <summary>
        /// Do not allow accessing named data streams in the mounted WIM image.
        /// </summary>
        StreamInterfaceNone = 0x00000004,
        /// <summary>
        /// Access named data streams in the mounted WIM image through extended file
        /// attributes named "user.X", where X is the name of a data stream. 
        /// This is the default mode.
        /// </summary>
        StreamInterfaceXAttr = 0x00000008,
        /// <summary>
        /// Access named data streams in the mounted WIM image by specifying the file
        /// name, a colon, then the name of the data stream.
        /// </summary>
        StreamInterfaceWindows = 0x00000010,
        /// <summary>
        /// Support UNIX owners, groups, modes, and special files.
        /// </summary>
        UnixData = 0x00000020,
        /// <summary>
        /// Allow other users to see the mounted filesystem.
        /// This passes the allow_other option to fuse_main().
        /// </summary>
        AllowOther = 0x00000040,
    }
    #endregion

    #region enum OpenFlags
    [Flags]
    public enum OpenFlags : int
    {
        None = 0x00000000,
        /// <summary>
        /// Verify the WIM contents against the WIM's integrity table, if present.
        /// The integrity table stores checksums for the raw data of the WIM file, divided into fixed size chunks.
        /// Verification will compute checksums and compare them with the stored values.
        /// If there are any mismatches, then <see cref="ErrorCode.Integrity"/> will be issued. 
        /// If the WIM file does not contain an integrity table, then this flag has no effect.
        /// </summary>
        CheckIntegrity = 0x00000001,
        /// <summary>
        /// Issue an error (<see cref="ErrorCode.IsSplitWim"/>) if the WIM is part of a split WIM. 
        /// Software can provide this flag for convenience if it explicitly does not want to support split WIMs.
        /// </summary>
        ErrorIfSplit = 0x00000002,
        /// <summary>
        /// Check if the WIM is writable and issue an error (<see cref="ErrorCode.WimIsReadOnly"/>) if it is not.
        /// A WIM is considered writable only if it is writable at the filesystem level, does not have the
        /// "WIM_HDR_FLAG_READONLY" flag set in its header, and is not part of a spanned set. 
        /// It is not required to provide this flag before attempting to make changes to the WIM,
        /// but with this flag you get an error immediately rather than potentially much later,
        /// when <see cref="Wim.Overwrite(WriteFlags, uint)"/> is finally called.
        /// </summary>
        WriteAccess = 0x00000004,
    }
    #endregion

    #region enum UnmountFlags (Linux Only)
    [Flags]
    public enum UnmountFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Provide <see cref="WriteFlags.CheckIntegrity"/> when committing the WIM image.
        /// Ignored if <see cref="Commit"/> not also specified.
        /// </summary>
        CheckIntegrity = 0x00000001,
        /// <summary>
        /// Commit changes to the read-write mounted WIM image.
        /// If this flag is not specified, changes will be discarded.
        /// </summary>
        Commit = 0x00000002,
        /// <summary>
        /// Provide <see cref="WriteFlags.Rebuild"/> when committing the WIM image.
        /// Ignored if <see cref="Commit"/> not also specified.
        /// </summary>
        Rebuild = 0x00000004,
        /// <summary>
        /// Provide <see cref="WriteFlags.Recompress"/> when committing the WIM image.
        /// Ignored if <see cref="Commit"/> not also specified.
        /// </summary>
        Recompress = 0x00000008,
        /// <summary>
        /// In combination with <see cref="Commit"/> for a read-write mounted WIM image,
        /// forces all file descriptors to the open WIM image to be closed before committing it.
        /// </summary>
        /// <remarks>
        /// Without <see cref="Commit"/> or with a read-only mounted WIM image, this flag has no effect.
        /// </remarks>
        Force = 0x00000010,
        /// <summary>
        /// In combination with <see cref="Commit"/> for a read-write mounted WIM image,
        /// causes the modified image to be committed to the WIM file as a new, unnamed image appended to the archive.
        /// The original image in the WIM file will be unmodified.
        /// </summary>
        NewImage = 0x00000020,
    }
    #endregion

    #region enum UpdateFlags
    [Flags]
    public enum UpdateFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Send <see cref="ProgressMsg.UpdateBeginCommand"/ and <see cref="ProgressMsg.UpdateEndCommand"/ messages.
        /// </summary>
        SendProgress = 0x00000001,
    }
    #endregion

    #region enum WriteFlags
    [Flags]
    public enum WriteFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Include an integrity table in the resulting WIM file.
        ///
        /// For <see cref="Wim">'s created with <see cref="Wim.OpenWim()"/>, the default behavior is to
        /// include an integrity table if and only if one was present before. 
        /// For <see cref="Wim">'s created with <see cref=">Wim.CreateNewWim()"/, the default behavior is to not include an integrity table.
        /// </summary>
        CheckIntegrity = 0x00000001,
        /// <summary>
        /// Do not include an integrity table in the resulting WIM file.
        /// This is the default behavior, unless the <see cref="Wim"> was created by opening a WIM with an integrity table.
        /// </summary>
        NoCheckIntegrity = 0x00000002,
        /// <summary>
        /// Write the WIM as "pipable".
        /// After writing a WIM with this flag specified, images from it can be applied directly from a pipe.
        /// See the documentation for the "--pipable" option of "wimcapture" for more information.
        /// Beware: WIMs written with this flag will not be compatible with Microsoft's software.
        ///
        /// For <see cref="Wim">'s created with <see cref="Wim.OpenWim()"/>, the default behavior is to write the WIM as pipable if and only if it was pipable before.
        /// For <see cref="Wim">'s created with <see cref="Wim.CreateNewWim(CompressionType)"/>, the default behavior is to write the WIM as non-pipable.
        /// </summary>
        Pipable = 0x00000004,
        /// <summary>
        /// Do not write the WIM as "pipable".
        /// This is the default behavior, unless the <see cref="Wim"> was created by opening a pipable WIM.
        /// </summary>
        NotPipable = 0x00000008,
        /// <summary>
        /// When writing data to the WIM file, recompress it, even if the data is already available in the desired compressed form
        /// (for example, in a WIM file from which an image has been exported using <see cref="Wim.ExportImage(int, Wim, string, string, ExportFlags)"/>).
        ///
        /// <see cref="Recompress"/> can be used to recompress with a higher compression ratio for the same compression type and chunk size.
        /// Simply using the default compression settings may suffice for this, especially if the WIM
        /// file was created using another program/library that may not use as sophisticated compression algorithms.
        /// Or, <see cref="Wim.SetDefaultCompressionLevel()"/> can be called beforehand to set an even higher compression level than the default.
        ///
        /// If the WIM contains solid resources, then WriteFlags.RECOMPRESS can be used in
        /// combination with WriteFlags.SOLID to prevent any solid resources from being re-used.
        /// Otherwise, solid resources are re-used somewhat more liberally than normal compressed resources.
        ///
        /// <see cref="Recompress"/> does not cause recompression of data that would not otherwise be written.
        /// For example, a call to Wim.Overwrite() with WriteFlags.RECOMPRESS will not, by itself,
        /// cause already-existing data in the WIM file to be recompressed.
        /// To force the WIM file to be fully rebuilt and recompressed, combine <see cref="Recompress"/> with <see cref="Rebuild"/>.
        /// </summary>
        Recompress = 0x00000010,
        /// <summary>
        /// Immediately before closing the WIM file, sync its data to disk.
        ///
        /// This flag forces the function to wait until the data is safely on disk before returning success.
        /// Otherwise, modern operating systems tend to cache data for some time (in some cases, 30+ seconds)
        /// before actually writing it to disk, even after reporting to the application that the writes have succeeded.
        ///
        /// <see cref="Wim.Overwrite()"/> will set this flag automatically if it decides to overwrite the WIM file via a temporary file instead of in-place.
        /// This is necessary on POSIX systems; it will, for example, avoid problems with delayed allocation on ext4.
        /// </summary>
        FSync = 0x00000020,
        /// <summary>
        /// For Wim.Overwrite():
        /// rebuild the entire WIM file, even if it otherwise could be updated in-place by appending to it.
        /// Any data that existed in the original WIM file but is not actually needed by any of the remaining images will not be included.
        /// This can free up space left over after previous in-place modifications to the WIM file.
        ///
        /// This flag can be combined with <see cref="Recompress"/> to force all data to be recompressed. 
        /// Otherwise, compressed data is re-used if possible.
        ///
        /// <see cref="Wim.Write(string, int, WriteFlags, uint)"/> ignores this flag.
        /// </summary>
        Rebuild = 0x00000040,
        /// <summary>
        /// For Wim.Overwrite():
        /// override the default behavior after one or more calls to <see cref="Wim.DeleteImage(int)"/>, which is to rebuild the entire WIM file.
        /// With this flag, only minimal changes to correctly remove the image from the WIM file will be taken. 
        /// This can be much faster, but it will result in the WIM file getting larger rather than smaller.
        ///
        /// <see cref="Wim.Write(string, int, WriteFlags, uint)"/> ignores this flag.
        /// </summary>
        SoftDelete = 0x00000080,
        /// <summary>
        /// For <see cref="Wim.Overwrite(WriteFlags, uint)"/>, allow overwriting the WIM file even if the readonly flag (WIM_HDR_FLAG_READONLY) is set in the WIM header.
        /// This can be used following a call to <see cref="Wim.SetWimInfo(WimInfo, ChangeFlags)"/> with the <see cref="ChangeFlags.ReadOnly"/> flag to 
        /// actually set the readonly flag on the on-disk WIM file.
        ///
        /// <see cref="Wim.Write(string, int, WriteFlags, uint)"/> ignores this flag.
        /// </summary>
        IgnoreReadOnlyFlag = 0x00000100,
        /// <summary>
        /// Do not include file data already present in other WIMs.
        /// This flag can be used to write a "delta" WIM after the WIM files on which the delta is to be
        /// based were referenced with <see cref="Wim.ReferenceResourceFiles(IEnumerable{string}, RefFlags, OpenFlags)"/> 
        /// or <see cref="Wim.ReferenceResources(IEnumerable{Wim})"/>.
        /// </summary>
        SkipExternalWims = 0x00000200,
        /// <summary>
        /// For <see cref="Wim.Write(string, int, WriteFlags, uint)"/>, retain the WIM's GUID instead of generating a new one.
        ///
        /// <see cref="Wim.Overwrite(WriteFlags, uint)"/> sets this by default, since the WIM remains, logically, the same file.
        /// </summary>
        RetainGuid = 0x00000800,
        /// <summary>
        /// Concatenate files and compress them together, rather than compress each file independently.
        /// This is also known as creating a "solid archive".
        /// This tends to produce a better compression ratio at the cost of much slower random access.
        ///
        /// WIM files created with this flag are only compatible with wimlib v1.6.0 or later, 
        /// WIMGAPI Windows 8 or later, and DISM Windows 8.1 or later.
        /// WIM files created with this flag use a different version number in their header
        /// (3584 instead of 68864) and are also called "ESD files".
        ///
        /// Note that providing this flag does not affect the "append by default" behavior of <see cref="Wim.Overwrite(WriteFlags, uint)"/>. 
        /// In other words, <see cref="Wim.Overwrite(WriteFlags, uint)"/> with just <see cref="Solid"/> can be used to append solid-compressed data to a
        /// WIM file that originally did not contain any solid-compressed data. 
        /// But if you instead want to rebuild and recompress an entire WIM file in solid mode,
        /// then also provide <see cref="Rebuild"/> and <see cref="Recompress"/>.
        ///
        /// Currently, new solid resources will, by default, be written using LZMS compression with 64 MiB (67108864 byte) chunks. 
        /// Use <see cref="Wim.SetOutputPackCompressionType(CompressionType)"/> and/or <see cref="Wim.SetOutputPackChunkSize(uint)"/> to change this.
        /// This is independent of the WIM's main compression type and chunk size;
        /// you can have a WIM that nominally uses LZX compression and 32768 byte chunks but actually contains
        /// LZMS-compressed solid resources, for example.
        /// However, if including solid resources, I suggest that you set the WIM's main compression type to LZMS as well,
        /// either by creating the WIM with <see cref="Wim.CreateNewWim(CompressType.LZMS, ...)"/>
        /// or by calling <see cref="Wim.SetOutputCompressionType(..., CompressType.LZMS)"/>.
        ///
        /// This flag will be set by default when writing or overwriting a WIM file that
        /// either already contains solid resources, or has had solid resources exported
        /// into it and the WIM's main compression type is LZMS.
        /// </summary>
        Solid = 0x00001000,
        /// <summary>
        /// Send <see cref="ProgressMsg.DoneWithFile"/> messages while writing the WIM file.
        /// This is only needed in the unusual case that the library user needs to
        /// know exactly when wimlib has read each file for the last time.
        /// </summary>
        SendDoneWithFileMessages = 0x00002000,
        /// <summary>
        /// Do not consider content similarity when arranging file data for solid compression. 
        /// Providing this flag will typically worsen the compression ratio,
        /// so only provide this flag if you know what you are doing.
        /// </summary>
        NoSolidSort = 0x00004000,
        /// <summary>
        /// Since wimlib v1.8.3 and for <see cref="Wim.Overwrite(WriteFlags, uint)"/> only: "unsafely" compact the WIM file in-place, without appending.
        /// Existing resources are shifted down to fill holes and new resources are appended as needed.
        /// The WIM file is truncated to its final size, which may shrink the on-disk file.
        /// 
        /// This operation cannot be safely interrupted.
        /// If the operation is interrupted, then the WIM file will be corrupted,
        /// and it may be impossible (or at least very difficult) to recover any data from it.
        /// Users of this flag are expected to know what they are doing and assume responsibility for any data corruption that may result.
        ///
        /// If the WIM file cannot be compacted in-place because of its structure, its layout, or other requested write parameters,
        /// then <see cref="Wim.Overwrite(WriteFlags, uint)"/> fails with <see cref="ErrorCode.CompactionNotPossible"/>, 
        /// and the caller may wish to retry the operation without this flag.
        /// </summary>
        UnsafeCompact = 0x00008000,
    }
    #endregion

    #region enum InitFlags
    [Flags]
    public enum InitFlags : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Windows-only:
        /// Do not attempt to acquire additional privileges (currently SeBackupPrivilege, SeRestorePrivilege, 
        /// SeSecurityPrivilege, SeTakeOwnershipPrivilege, and SeManageVolumePrivilege) when initializing the library.
        /// 
        /// This flag is intended for the case where the calling program manages these privileges itself. 
        /// Note: by default, no error is issued if privileges cannot be acquired, although related errors may be reported later,
        /// depending on if the operations performed actually require additional privileges or not.
        /// </summary>
        DontAcquirePrivileges = 0x00000002,
        /// <summary>
        /// Windows only:
        /// If <see cref="DontAcquirePrivileges"/> not specified, return <see cref="INSUFFICIENT_PRIVILEGES"/> if privileges 
        /// that may be needed to read all possible data and metadata for a capture operation could not be acquired. 
        /// Can be combined with <see cref="StrictApplyPrivileges"/>.
        /// </summary>
        StrictCapturePrivileges = 0x00000004,
        /// <summary>
        /// Windows only:
        /// If <see cref="DontAcquirePrivileges"/> not specified, return <see cref="INSUFFICIENT_PRIVILEGES"/> if privileges
        /// that may be needed to restore all possible data and metadata for an apply operation could not be acquired. 
        /// Can be combined with <see cref="StrictApplyPrivileges"/>.
        /// </summary>
        StrictApplyPrivileges = 0x00000008,
        /// <summary>
        /// Default to interpreting WIM paths case sensitively (default on UNIX-like systems).
        /// </summary>
        DefaultCaseSensitive = 0x00000010,
        /// <summary>
        /// Default to interpreting WIM paths case insensitively (default on Windows).
        /// This does not apply to mounted images.
        /// </summary>
        DefaultCaseInsensitive = 0x00000020,
    }
    #endregion

    #region enum RefFlags
    [Flags]
    public enum RefFlags : int
    {
        None = 0x00000000,
        /// <summary>
        /// For <see cref="Wim.ReferenceResourceFiles(IEnumerable{string}, RefFlags, OpenFlags)"/>, enable shell-style filename globbing.
        /// Ignored by <see cref="Wim.ReferenceResources(IEnumerable{Wim})"/>.
        /// </summary>
        GlobEnable = 0x00000001,
        /// <summary>
        /// For <see cref="Wim.ReferenceResourceFiles(IEnumerable{string}, RefFlags, OpenFlags)"/>, issue an error (<see cref="ErrorCode.GlobHadNoMatches"/>) if a glob did not match any files. 
        /// The default behavior without this flag is to issue no error at that point, but then attempt to open
        /// the glob as a literal path, which of course will fail anyway if no file exists at that path. 
        /// No effect if <see cref="GlobEnable"/> is not also specified.
        /// Ignored by <see cref="Wim.ReferenceResources(IEnumerable{Wim})"/>.
        /// </summary>
        GlobErrOnNoMatch = 0x00000002,
    }
    #endregion

    #region enum CompressorFlags
    [Flags]
    public enum CompressorFlags : uint
    {
        None = 0x0,
        Destructive = 0x80000000,
    }
    #endregion
    #endregion

    #region Native wimlib structures
    #region WimInfo
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class WimInfo
    {
        /// <summary>
        /// The globally unique identifier for this WIM. 
        /// (Note: all parts of a split WIM normally have identical GUIDs.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Guid;
        /// <summary>
        /// The number of images in this WIM file.
        /// </summary>
        public uint ImageCount;
        /// <summary>
        /// The 1-based index of the bootable image in this WIM file, or 0 if no image is bootable.
        /// </summary>
        public uint BootIndex;
        /// <summary>
        /// The version of the WIM file format used in this WIM file.
        /// </summary>
        public uint WimVersion;
        /// <summary>
        /// The default compression chunk size of resources in this WIM file.
        /// </summary>
        public uint ChunkSize;
        /// <summary>
        /// For split WIMs, the 1-based index of this part within the split WIM; otherwise 1.
        /// </summary>
        public ushort PartNumber;
        /// <summary>
        /// For split WIMs, the total number of parts in the split WIM; otherwise 1.
        /// </summary>
        public ushort TotalParts;
        /// <summary>
        /// The default compression type of resources in this WIM file, as <see cref="CompressionType"/> enum.
        /// </summary>
        public CompressionType CompressionType
        {
            get => (CompressionType)CompressionTypeInt;
            set => CompressionTypeInt = (int)value;
        }
        private int CompressionTypeInt;
        /// <summary>
        /// The size of this WIM file in bytes, excluding the XML data and integrity table.
        /// </summary>
        public ulong TotalBytes;
        /// <summary>
        /// Bit 0 - 9 : Information Flags
        /// Bit 10 - 31 : Reserved
        /// </summary>
        private uint _bitFlag;
        /// <summary>
        /// 1 iff this WIM file has an integrity table.
        /// </summary>
        public bool HasIntegrityTable
        {
            get => WimLibLoader.GetBitField(_bitFlag, 0);
            set => WimLibLoader.SetBitField(ref _bitFlag, 0, value);
        }
        /// <summary>
        /// 1 iff this info struct is for a <see cref="Wim"> that has a backing file.
        /// </summary>
        public bool OpenedFromFile
        {
            get => WimLibLoader.GetBitField(_bitFlag, 1);
            set => WimLibLoader.SetBitField(ref _bitFlag, 1, value);
        }
        /// <summary>
        /// 1 iff this WIM file is considered readonly for any reason
        /// (e.g. the "readonly" header flag is set, or this is part of a split WIM, or filesystem permissions deny writing)
        /// </summary>
        public bool IsReadOnly
        {
            get => WimLibLoader.GetBitField(_bitFlag, 2);
            set => WimLibLoader.SetBitField(ref _bitFlag, 2, value);
        }
        /// <summary>
        /// 1 iff the "reparse point fix" flag is set in this WIM's header
        /// </summary>
        public bool HasRpfix
        {
            get => WimLibLoader.GetBitField(_bitFlag, 3);
            set => WimLibLoader.SetBitField(ref _bitFlag, 3, value);
        }
        /// <summary>
        /// 1 iff the "readonly" flag is set in this WIM's header
        /// </summary>
        public bool IsMarkedReadOnly
        {
            get => WimLibLoader.GetBitField(_bitFlag, 4);
            set => WimLibLoader.SetBitField(ref _bitFlag, 4, value);
        }
        /// <summary>
        /// 1 iff the "spanned" flag is set in this WIM's header
        /// </summary>
        public bool Spanned
        {
            get => WimLibLoader.GetBitField(_bitFlag, 5);
            set => WimLibLoader.SetBitField(ref _bitFlag, 5, value);
        }
        /// <summary>
        /// 1 iff the "write in progress" flag is set in this WIM's header
        /// </summary>
        public bool WriteInProgress
        {
            get => WimLibLoader.GetBitField(_bitFlag, 6);
            set => WimLibLoader.SetBitField(ref _bitFlag, 6, value);
        }
        /// <summary>
        /// 1 iff the "metadata only" flag is set in this WIM's header
        /// </summary>
        public bool MetadataOnly
        {
            get => WimLibLoader.GetBitField(_bitFlag, 7);
            set => WimLibLoader.SetBitField(ref _bitFlag, 7, value);
        }
        /// <summary>
        /// 1 iff the "resource only" flag is set in this WIM's header
        /// </summary>
        public bool ResourceOnly
        {
            get => WimLibLoader.GetBitField(_bitFlag, 8);
            set => WimLibLoader.SetBitField(ref _bitFlag, 8, value);
        }
        /// <summary>
        /// 1 iff this WIM file is pipable (see WriteFlags.PIPABLE).
        /// </summary>
        public bool Pipable
        {
            get => WimLibLoader.GetBitField(_bitFlag, 9);
            set => WimLibLoader.SetBitField(ref _bitFlag, 9, value);
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        private uint[] _reserved;
    }
    #endregion

    #region CaptureSource
    /// <summary>
    /// An array of these structures is passed to <see cref="Wim.AddImageMultiSource(IEnumerable{CaptureSource}, string, string, AddFlags)"/> 
    /// to specify the sources from which to create a WIM image. 
    /// </summary>
    /// <remarks>
    /// Wrapper class of Utf{16,8}.CaptureSourceBase.
    /// Instance of the class is converted to <see cref="CaptureSourceBase"/> on-the-fly.
    /// </remarks>
    public class CaptureSource
    {
        /// <summary>
        /// Absolute or relative path to a file or directory on the external filesystem to be included in the image.
        /// </summary>
        public string FsSourcePath;
        /// <summary>
        /// Destination path in the image.
        /// To specify the root directory of the image, use @"\". 
        /// </summary>
        public string WimTargetPath;

        public CaptureSource(string fsSourcePath, string wimTargetPath)
        {
            FsSourcePath = fsSourcePath;
            WimTargetPath = wimTargetPath;
        }
    };

    #region UpdateCommand
    public class UpdateCommand
    {
        #region Field - UpdateCommandOp
        public UpdateOp Op;
        #endregion

        #region Field - Related to Add
        /// <summary>
        /// Filesystem path to the file or directory tree to add.
        /// </summary>
        public string AddFsSourcePath;
        /// <summary>
        /// Destination path in the image.
        /// To specify the root directory of the image, use Wim.RootPath.
        /// </summary>
        public string AddWimTargetPath;
        /// <summary>
        /// Path to capture configuration file to use, or null if not specified.
        /// </summary>
        public string AddConfigFile;
        /// <summary>
        /// Bitwise OR of AddFlags.
        /// </summary>
        public AddFlags AddFlags;
        #endregion

        #region Field - Related to Delete
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        public string DelWimPath;
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        public DeleteFlags DeleteFlags;
        #endregion

        #region Field - Related to Rename
        /// <summary>
        /// The path to the source file or directory within the image.
        /// </summary>
        public string RenWimSourcePath;
        /// <summary>
        /// The path to the destination file or directory within the image.
        /// </summary>
        public string RenWimTargetPath;
        /// <summary>
        /// Reserved; set to 0. 
        /// </summary>
        private int _renameFlags;
        #endregion

        #region Properties - Add, Delete, Rename
        public AddCommand Add
        {
            get
            {
                if (Op != UpdateOp.Add)
                    throw new InvalidOperationException("Field [Op] should be [UpdateOp.ADD]");
                return new AddCommand(AddFsSourcePath, AddWimTargetPath, AddConfigFile, AddFlags);
            }
            set
            {
                Op = UpdateOp.Add;
                AddFsSourcePath = value.FsSourcePath;
                AddWimTargetPath = value.WimTargetPath;
                AddConfigFile = value.ConfigFile;
                AddFlags = value.AddFlags;
            }
        }

        public DeleteCommand Delete
        {
            get
            {
                if (Op != UpdateOp.Delete)
                    throw new InvalidOperationException("Field [Op] should be [UpdateOp.DELETE]");
                return new DeleteCommand(DelWimPath, DeleteFlags);
            }
            set
            {
                Op = UpdateOp.Delete;
                DelWimPath = value.WimPath;
                DeleteFlags = value.DeleteFlags;
            }
        }

        public RenameCommand Rename
        {
            get
            {
                if (Op != UpdateOp.Rename)
                    throw new InvalidOperationException("Field [Op] should be [UpdateOp.DELETE]");
                return new RenameCommand(RenWimSourcePath, RenWimTargetPath);
            }
            set
            {
                Op = UpdateOp.Rename;
                RenWimSourcePath = value.WimSourcePath;
                RenWimTargetPath = value.WimTargetPath;
                _renameFlags = 0;
            }
        }
        #endregion

        #region Factory Methods
        public static UpdateCommand SetAdd(string fsSourcePath, string wimTargetPath, string configFile, AddFlags addFlags)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.Add,
                AddFsSourcePath = fsSourcePath,
                AddWimTargetPath = wimTargetPath,
                AddConfigFile = configFile,
                AddFlags = addFlags,
            };
        }

        public static UpdateCommand SetAdd(AddCommand add)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.Add,
                AddFsSourcePath = add.FsSourcePath,
                AddWimTargetPath = add.WimTargetPath,
                AddConfigFile = add.ConfigFile,
                AddFlags = add.AddFlags,
            };
        }

        public static UpdateCommand SetDelete(string wimPath, DeleteFlags deleteFlags)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.Delete,
                DelWimPath = wimPath,
                DeleteFlags = deleteFlags,
            };
        }

        public static UpdateCommand SetDelete(DeleteCommand del)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.Delete,
                DelWimPath = del.WimPath,
                DeleteFlags = del.DeleteFlags,
            };
        }

        public static UpdateCommand SetRename(string wimSourcePath, string wimTargetPath)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.Rename,
                RenWimSourcePath = wimSourcePath,
                RenWimTargetPath = wimTargetPath,
                _renameFlags = 0,
            };
        }

        public static UpdateCommand SetRename(RenameCommand ren)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.Rename,
                RenWimSourcePath = ren.WimSourcePath,
                RenWimTargetPath = ren.WimTargetPath,
                _renameFlags = 0,
            };
        }
        #endregion

        #region ToNativeStruct
        internal UpdateCommand32 ToNativeStruct32()
        {
            return Op switch
            {
                UpdateOp.Add => new UpdateCommand32
                {
                    Op = UpdateOp.Add,
                    AddFsSourcePath = AddFsSourcePath,
                    AddWimTargetPath = AddWimTargetPath,
                    AddConfigFile = AddConfigFile,
                    AddFlags = AddFlags,
                },
                UpdateOp.Delete => new UpdateCommand32
                {
                    Op = UpdateOp.Delete,
                    DelWimPath = DelWimPath,
                    DeleteFlags = DeleteFlags,
                },
                UpdateOp.Rename => new UpdateCommand32
                {
                    Op = UpdateOp.Rename,
                    RenWimSourcePath = RenWimSourcePath,
                    RenWimTargetPath = RenWimTargetPath,
                },
                _ => throw new InvalidOperationException("Internal Logic Error at UpdateCommand.ToNativeStruct32()"),
            };
        }

        internal UpdateCommand64 ToNativeStruct64()
        {
            return Op switch
            {
                UpdateOp.Add => new UpdateCommand64
                {
                    Op = UpdateOp.Add,
                    AddFsSourcePath = AddFsSourcePath,
                    AddWimTargetPath = AddWimTargetPath,
                    AddConfigFile = AddConfigFile,
                    AddFlags = AddFlags,
                },
                UpdateOp.Delete => new UpdateCommand64
                {
                    Op = UpdateOp.Delete,
                    DelWimPath = DelWimPath,
                    DeleteFlags = DeleteFlags,
                },
                UpdateOp.Rename => new UpdateCommand64
                {
                    Op = UpdateOp.Rename,
                    RenWimSourcePath = RenWimSourcePath,
                    RenWimTargetPath = RenWimTargetPath,
                },
                _ => throw new InvalidOperationException("Internal Logic Error at UpdateCommand.ToNativeStruct64()"),
            };
        }
        #endregion
    }
    #endregion

    #region enum UpdateOp
    [Flags]
    public enum UpdateOp : uint
    {
        /// <summary>
        /// Add a new file or directory tree to the image.
        /// </summary>
        Add = 0,
        /// <summary>
        /// Delete a file or directory tree from the image.
        /// </summary>
        Delete = 1,
        /// <summary>
        /// Rename a file or directory tree in the image.
        /// </summary>
        Rename = 2,
    }
    #endregion

    #region AddCommand, DeleteCommand, RenameCommand
    public class AddCommand
    {
        /// <summary>
        /// Filesystem path to the file or directory tree to add.
        /// </summary>
        public string FsSourcePath;
        /// <summary>
        /// Destination path in the image.
        /// To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>. 
        /// </summary>
        public string WimTargetPath;
        /// <summary>
        /// Path to capture configuration file to use, or null if not specified.
        /// </summary>
        public string ConfigFile;
        /// <summary>
        /// Bitwise OR of AddFlags.
        /// </summary>
        public AddFlags AddFlags;

        public AddCommand(string fsSourcePath, string wimTargetPath, string configFile, AddFlags addFlags)
        {
            FsSourcePath = fsSourcePath;
            WimTargetPath = wimTargetPath;
            ConfigFile = configFile;
            AddFlags = addFlags;
        }
    }

    public class DeleteCommand
    {
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        public string WimPath;
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        public DeleteFlags DeleteFlags;

        public DeleteCommand(string wimPath, DeleteFlags deleteFlags)
        {
            WimPath = wimPath;
            DeleteFlags = deleteFlags;
        }
    }

    public class RenameCommand
    {
        /// <summary>
        /// The path to the source file or directory within the image.
        /// </summary>
        public string WimSourcePath;
        /// <summary>
        /// The path to the destination file or directory within the image.
        /// </summary>
        public string WimTargetPath;

        public RenameCommand(string wimSourcePath, string wimTargetPath)
        {
            WimSourcePath = wimSourcePath;
            WimTargetPath = wimTargetPath;
        }
    }
    #endregion

    #region struct UpdateCommand32
    /// <summary>
    /// 32bit version of real UpdateCommand struct which is passed to the native wimlib.
    /// </summary>
    /// <remarks>
    /// Original C struct (wimlib_update_command) contains a union with three structs: Add, Delete and Rename.
    /// LayoutKind.Explicit is required to represent them in the .Net world.
    /// C# struct was used instead of class because .Net have to pass an array of value type to C code.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    internal struct UpdateCommand32
    {
        [FieldOffset(0)]
        public UpdateOp Op;

        #region AddCommand
        /// <summary>
        /// Filesystem path to the file or directory tree to add.
        /// </summary>
        [FieldOffset(4)]
        private IntPtr _addFsSourcePathPtr;
        public string AddFsSourcePath
        {
            get => Wim.Lib.PtrToStringAuto(_addFsSourcePathPtr);
            set => UpdatePtr(ref _addFsSourcePathPtr, value);
        }
        /// <summary>
        /// Destination path in the image.  To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>. 
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _addWimTargetPathPtr;
        public string AddWimTargetPath
        {
            get => Wim.Lib.PtrToStringAuto(_addWimTargetPathPtr);
            set => UpdatePtr(ref _addWimTargetPathPtr, value);
        }
        /// <summary>
        /// Path to capture configuration file to use, or null if not specified.
        /// </summary>
        [FieldOffset(12)]
        private IntPtr _addConfigFilePtr;
        public string AddConfigFile
        {
            get => Wim.Lib.PtrToStringAuto(_addConfigFilePtr);
            set => UpdatePtr(ref _addConfigFilePtr, value);
        }
        /// <summary>
        /// Bitwise OR of AddFlags.
        /// </summary>
        [FieldOffset(16)]
        public AddFlags AddFlags;
        #endregion

        #region DeleteCommand
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        [FieldOffset(4)]
        private IntPtr _delWimPathPtr;
        public string DelWimPath
        {
            get => Wim.Lib.PtrToStringAuto(_delWimPathPtr);
            set => UpdatePtr(ref _delWimPathPtr, value);
        }
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        [FieldOffset(8)]
        public DeleteFlags DeleteFlags;
        #endregion

        #region RenameCommand
        /// <summary>
        /// The path to the source file or directory within the image.
        /// </summary>
        [FieldOffset(4)]
        private IntPtr _renWimSourcePathPtr;
        public string RenWimSourcePath
        {
            get => Wim.Lib.PtrToStringAuto(_renWimSourcePathPtr);
            set => UpdatePtr(ref _renWimSourcePathPtr, value);
        }
        /// <summary>
        /// The path to the destination file or directory within the image.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _renWimTargetPathPtr;
        public string RenWimTargetPath
        {
            get => Wim.Lib.PtrToStringAuto(_renWimTargetPathPtr);
            set => UpdatePtr(ref _renWimTargetPathPtr, value);
        }
        /// <summary>
        /// Reserved; set to 0. 
        /// </summary>
        [FieldOffset(12)]
        private int _renameFlags;
        #endregion

        #region Free
        public void Free()
        {
            switch (Op)
            {
                case UpdateOp.Add:
                    FreePtr(ref _addFsSourcePathPtr);
                    FreePtr(ref _addWimTargetPathPtr);
                    FreePtr(ref _addConfigFilePtr);
                    break;
                case UpdateOp.Delete:
                    FreePtr(ref _delWimPathPtr);
                    break;
                case UpdateOp.Rename:
                    FreePtr(ref _renWimSourcePathPtr);
                    FreePtr(ref _renWimTargetPathPtr);
                    break;
            }
        }

        internal static void FreePtr(ref IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
        }

        internal static void UpdatePtr(ref IntPtr ptr, string str)
        {
            FreePtr(ref ptr);
            ptr = Wim.Lib.StringToHGlobalAuto(str);
        }
        #endregion

        #region ToManagedClass
        public UpdateCommand ToManagedClass()
        {
            return Op switch
            {
                UpdateOp.Add => UpdateCommand.SetAdd(AddFsSourcePath, AddWimTargetPath, AddConfigFile, AddFlags),
                UpdateOp.Delete => UpdateCommand.SetDelete(DelWimPath, DeleteFlags),
                UpdateOp.Rename => UpdateCommand.SetRename(RenWimSourcePath, RenWimTargetPath),
                _ => throw new InvalidOperationException("Internal Logic Error at UpdateCommand32.Convert()"),
            };
        }
        #endregion
    }
    #endregion

    #region struct UpdateCommand64
    /// <summary>
    /// 64bit version of real UpdateCommand struct which is passed to the native wimlib.
    /// </summary>
    /// <remarks>
    /// Original C struct (wimlib_update_command) contains a union with three structs: Add, Delete and Rename.
    /// LayoutKind.Explicit is required to represent them in the .Net world.
    /// C# struct was used instead of class because .Net have to pass an array of value type to C code.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    internal struct UpdateCommand64
    {
        [FieldOffset(0)]
        public UpdateOp Op;

        #region AddCommand
        /// <summary>
        /// Filesystem path to the file or directory tree to add.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _addFsSourcePathPtr;
        public string AddFsSourcePath
        {
            get => Wim.Lib.PtrToStringAuto(_addFsSourcePathPtr);
            set => UpdatePtr(ref _addFsSourcePathPtr, value);
        }
        /// <summary>
        /// Destination path in the image.  To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        [FieldOffset(16)]
        private IntPtr _addWimTargetPathPtr;
        public string AddWimTargetPath
        {
            get => Wim.Lib.PtrToStringAuto(_addWimTargetPathPtr);
            set => UpdatePtr(ref _addWimTargetPathPtr, value);
        }
        /// <summary>
        /// Path to capture configuration file to use, or null if not specified.
        /// </summary>
        [FieldOffset(24)]
        private IntPtr _addConfigFilePtr;
        public string AddConfigFile
        {
            get => Wim.Lib.PtrToStringAuto(_addConfigFilePtr);
            set => UpdatePtr(ref _addConfigFilePtr, value);
        }
        /// <summary>
        /// Bitwise OR of AddFlags.
        /// </summary>
        [FieldOffset(32)]
        public AddFlags AddFlags;
        #endregion

        #region DeleteCommand
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _delWimPathPtr;
        public string DelWimPath
        {
            get => Wim.Lib.PtrToStringAuto(_delWimPathPtr);
            set => UpdatePtr(ref _delWimPathPtr, value);
        }
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        [FieldOffset(16)]
        public DeleteFlags DeleteFlags;
        #endregion

        #region RenameCommand
        /// <summary>
        /// The path to the source file or directory within the image.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _renWimSourcePathPtr;
        public string RenWimSourcePath
        {
            get => Wim.Lib.PtrToStringAuto(_renWimSourcePathPtr);
            set => UpdatePtr(ref _renWimSourcePathPtr, value);
        }
        /// <summary>
        /// The path to the destination file or directory within the image.
        /// </summary>
        [FieldOffset(16)]
        private IntPtr _renWimTargetPathPtr;
        public string RenWimTargetPath
        {
            get => Wim.Lib.PtrToStringAuto(_renWimTargetPathPtr);
            set => UpdatePtr(ref _renWimTargetPathPtr, value);
        }
        /// <summary>
        /// Reserved; set to 0. 
        /// </summary>
        [FieldOffset(24)]
        private int _renameFlags;
        #endregion

        #region Free
        public void Free()
        {
            switch (Op)
            {
                case UpdateOp.Add:
                    FreePtr(ref _addFsSourcePathPtr);
                    FreePtr(ref _addWimTargetPathPtr);
                    FreePtr(ref _addConfigFilePtr);
                    break;
                case UpdateOp.Delete:
                    FreePtr(ref _delWimPathPtr);
                    break;
                case UpdateOp.Rename:
                    FreePtr(ref _renWimSourcePathPtr);
                    FreePtr(ref _renWimTargetPathPtr);
                    break;
            }
        }

        internal static void FreePtr(ref IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
        }

        internal static void UpdatePtr(ref IntPtr ptr, string str)
        {
            FreePtr(ref ptr);
            ptr = Wim.Lib.StringToHGlobalAuto(str);
        }
        #endregion

        #region ToManagedClass
        public UpdateCommand ToManagedClass()
        {
            return Op switch
            {
                UpdateOp.Add => UpdateCommand.SetAdd(AddFsSourcePath, AddWimTargetPath, AddConfigFile, AddFlags),
                UpdateOp.Delete => UpdateCommand.SetDelete(DelWimPath, DeleteFlags),
                UpdateOp.Rename => UpdateCommand.SetRename(RenWimSourcePath, RenWimTargetPath),
                _ => throw new InvalidOperationException("Internal Logic Error at UpdateCommand64.Convert()"),
            };
        }
        #endregion
    }
    #endregion
    #endregion

    #region DirEntry
    /// <summary>
    /// Structure passed to the <see cref="Wim.IterateDirTree()"/> callback function.
    /// Roughly, the information about a "file" in the WIM image --- but really a directory entry ("dentry") because hard links are allowed.
    /// The <see cref="HardLinkGroupId"/> field can be used to distinguish actual file inodes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class DirEntryBase
    {
        /// <summary>
        /// Name of the file, or null if this file is unnamed. Only the root directory of an image will be unnamed.
        /// </summary>
        public string FileName => Wim.Lib.PtrToStringAuto(_fileNamePtr);
        private IntPtr _fileNamePtr;
        /// <summary>
        /// 8.3 name (or "DOS name", or "short name") of this file; or null if this file has no such name.
        /// </summary>
        public string DosName => Wim.Lib.PtrToStringAuto(_dosNamePtr);
        private IntPtr _dosNamePtr;
        /// <summary>
        /// Full path to this file within the image.
        /// Path separators will be <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        public string FullPath => Wim.Lib.PtrToStringAuto(_fullPathPtr);
        private IntPtr _fullPathPtr;
        /// <summary>
        /// Depth of this directory entry, where 0 is the root, 1 is the root's children, ..., etc.
        /// </summary>
        public ulong Depth => DepthVal.ToUInt64();
        private UIntPtr DepthVal; // size_t
        /// <summary>
        /// Pointer to the security descriptor for this file, in Windows SECURITY_DESCRIPTOR_RELATIVE format,
        /// or null if this file has no security descriptor.
        /// </summary>
        public byte[] SecurityDescriptor
        {
            get
            {
                if (SecurityDescriptorPtr == IntPtr.Zero)
                    return null;

                byte[] buf = new byte[SecurityDescriptorSize];
                Marshal.Copy(SecurityDescriptorPtr, buf, 0, (int)_securityDescriptorSizeVal.ToUInt32());
                return buf;
            }
        }
        public IntPtr SecurityDescriptorPtr;
        /// <summary>
        /// Size of the above security descriptor, in bytes. 
        /// </summary>
        public ulong SecurityDescriptorSize => _securityDescriptorSizeVal.ToUInt64();
        private UIntPtr _securityDescriptorSizeVal; // size_t
        /// <summary>
        /// File attributes, such as whether the file is a directory or not.
        /// These are the "standard" Windows FILE_ATTRIBUTE_* values, thus <see cref="System.IO.FileAttributes"/> typed.
        /// </summary>
        public FileAttributes Attributes;
        /// <summary>
        /// If the file is a reparse point (FileAttribute.REPARSE_POINT set in the attributes), this will give the reparse tag.
        /// This tells you whether the reparse point is a symbolic link, junction point, or some other, more unusual kind of reparse point.
        /// </summary>
        public ReparseTag ReparseTag;
        /// <summary>
        /// Number of links to this file's inode (hard links).
        ///
        /// Currently, this will always be 1 for directories.
        /// However, it can be greater than 1 for nondirectory files.
        /// </summary>
        public uint NumLinks;
        /// <summary>
        /// Number of named data streams this file has.
        /// Normally 0.
        /// </summary>
        public uint NumNamedStreams;
        /// <summary>
        /// A unique identifier for this file's inode.
        /// However, as a special case, if the inode only has a single link (NumLinks == 1), this value may be 0.
        ///
        /// Note: if a WIM image is captured from a filesystem, this value is not
        /// guaranteed to be the same as the original number of the inode on the filesystem.
        /// </summary>
        public ulong HardLinkGroupId;
        /// <summary>
        /// Time this file was created.
        /// </summary>
        public DateTime CreationTime => _creationTimeVal.ToDateTime(_creationTimeHigh);
        private WimTimeSpec _creationTimeVal;
        /// <summary>
        /// Time this file was last written to.
        /// </summary>
        public DateTime LastWriteTime => _lastWriteTimeVal.ToDateTime(_lastWriteTimeHigh);
        private WimTimeSpec _lastWriteTimeVal;
        /// <summary>
        /// Time this file was last accessed.
        /// </summary>
        public DateTime LastAccessTime => _lastAccessTimeVal.ToDateTime(_lastAccessTimeHigh);
        private WimTimeSpec _lastAccessTimeVal;
        /// <summary>
        /// The UNIX user ID of this file.
        /// This is a wimlib extension.
        ///
        /// This field is only valid if UnixMode != 0.
        /// </summary>
        public uint UnixUserId;
        /// <summary>
        /// The UNIX group ID of this file.
        /// This is a wimlib extension.
        ///
        /// This field is only valid if UnixMode != 0.
        /// </summary>
        public uint UnixGroupId;
        /// <summary>
        /// The UNIX mode of this file.
        /// This is a wimlib extension.
        ///
        /// If this field is 0, then UnixUid, UnixGid, UnixMode, and UnixRootDevice are all unknown
        /// (fields are not present in the WIM/ image).
        /// </summary>
        public uint UnixMode;
        /// <summary>
        /// The UNIX device ID (major and minor number) of this file.
        /// This is a wimlib extension.
        ///
        /// This field is only valid if UnixMode != 0.
        /// </summary>
        public uint UnixRootDevice;
        /// <summary>
        /// The object ID of this file, if any.
        /// Only valid if WimObjectId.ObjectId is not all zeroes.
        /// </summary>
        public WimObjectId ObjectId;

        private int _creationTimeHigh;
        private int _lastWriteTimeHigh;
        private int _lastAccessTimeHigh;
        private int _reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private ulong[] _reserved;
    }

    /// <summary>
    /// Structure passed to the <see cref="Wim.IterateDirTree()"/> callback function.
    /// Roughly, the information about a "file" in the WIM image --- but really a directory entry ("dentry") because hard links are allowed.
    /// The <see cref="HardLinkGroupId"/> field can be used to distinguish actual file inodes.
    /// </summary>
    /// <remarks>
    /// Wrapper of <see cref="DirEntryBase"/>, created to handle array of variable-length type <see cref="StreamEntry"/>.
    /// Converted to and from <see cref="DirEntryBase"/> on-the-fly.
    /// </remarks>
    public class DirEntry
    {
        /// <summary>
        /// Name of the file, or null if this file is unnamed. Only the root directory of an image will be unnamed.
        /// </summary>
        public string FileName;
        /// <summary>
        /// 8.3 name (or "DOS name", or "short name") of this file; or null if this file has no such name.
        /// </summary>
        public string DosName;
        /// <summary>
        /// Full path to this file within the image.
        /// Path separators will be <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        public string FullPath;
        /// <summary>
        /// Depth of this directory entry, where 0 is the root, 1 is the root's children, ..., etc.
        /// </summary>
        public ulong Depth;
        /// <summary>
        /// A security descriptor for this file, in Windows SECURITY_DESCRIPTOR_RELATIVE format,
        /// or null if this file has no security descriptor.
        /// </summary>
        public byte[] SecurityDescriptor;
        /// <summary>
        /// File attributes, such as whether the file is a directory or not.
        /// These are the "standard" Windows FILE_ATTRIBUTE_* values, thus <see cref="System.IO.FileAttributes"/> typed.
        /// </summary>
        public FileAttributes Attributes;
        /// <summary>
        /// If the file is a reparse point (FileAttribute.REPARSE_POINT set in the attributes), this will give the reparse tag.
        /// This tells you whether the reparse point is a symbolic link, junction point, or some other, more unusual kind of reparse point.
        /// </summary>
        public ReparseTag ReparseTag;
        /// <summary>
        /// Number of links to this file's inode (hard links).
        ///
        /// Currently, this will always be 1 for directories.
        /// However, it can be greater than 1 for nondirectory files.
        /// </summary>
        public uint NumLinks;
        /// <summary>
        /// Number of named data streams this file has.
        /// Normally 0.
        /// </summary>
        public uint NumNamedStreams;
        /// <summary>
        /// A unique identifier for this file's inode.
        /// However, as a special case, if the inode only has a single link (NumLinks == 1), this value may be 0.
        ///
        /// Note: if a WIM image is captured from a filesystem, this value is not
        /// guaranteed to be the same as the original number of the inode on the filesystem.
        /// </summary>
        public ulong HardLinkGroupId;
        /// <summary>
        /// Time this file was created.
        /// </summary>
        /// <summary>
        /// Time this file was created.
        /// </summary>
        public DateTime CreationTime;
        /// <summary>
        /// Time this file was last written to.
        /// </summary>
        public DateTime LastWriteTime;
        /// <summary>
        /// Time this file was last accessed.
        /// </summary>
        public DateTime LastAccessTime;
        /// <summary>
        /// The UNIX user ID of this file.
        /// This is a wimlib extension.
        ///
        /// This field is only valid if UnixMode != 0.
        /// </summary>
        public uint UnixUserId;
        /// <summary>
        /// The UNIX group ID of this file.
        /// This is a wimlib extension.
        ///
        /// This field is only valid if UnixMode != 0.
        /// </summary>
        public uint UnixGroupId;
        /// <summary>
        /// The UNIX mode of this file.
        /// This is a wimlib extension.
        ///
        /// If this field is 0, then UnixUid, UnixGid, UnixMode, and UnixRootDevice are all unknown
        /// (fields are not present in the WIM/ image).
        /// </summary>
        public uint UnixMode;
        /// <summary>
        /// The UNIX device ID (major and minor number) of this file.
        /// This is a wimlib extension.
        ///
        /// This field is only valid if UnixMode != 0.
        /// </summary>
        public uint UnixRootDevice;
        /// <summary>
        /// The object ID of this file, if any.
        /// Only valid if WimObjectId.ObjectId is not all zeroes.
        /// </summary>
        public WimObjectId ObjectId;
        /// <summary>
        /// Variable-length array of streams that make up this file.
        ///
        /// The first entry will always exist and will correspond to the unnamed data stream (default file contents), 
        /// so it will have (stream_name == null).
        /// Alternatively, for reparse point files, the first entry will correspond to the reparse data stream.
        /// Alternatively, for encrypted files, the first entry will correspond to the encrypted data.
        ///
        /// Then, following the first entry, there be NumNamedStreams additional entries that specify the named data streams,
        /// if any, each of which will have (stream_name != null).
        /// </summary>
        public StreamEntry[] Streams;
    }

    /// <summary>
    /// Refer to <see href="https://docs.microsoft.com/en-us/windows/win32/fileio/reparse-point-tags"/>.
    /// </summary>
    public enum ReparseTag : uint
    {
        ReservedZero = 0x00000000,
        ReservedOne = 0x00000001,
        MountPoint = 0xA0000003,
        HSM = 0xC0000004,
        HSM2 = 0x80000006,
        DriverExtender = 0x80000005,
        SIS = 0x80000007,
        DFS = 0x8000000A,
        DFSR = 0x80000012,
        FilterManager = 0x8000000B,
        WOF = 0x80000017,
        SymLink = 0xA000000C,
    }
    #endregion

    #region struct WimTimeSpec
    [StructLayout(LayoutKind.Sequential)]
    internal struct WimTimeSpec
    {
        /// <summary>
        /// Represents a data, not an address.
        /// Represented type is int64_t in 64bit, int32_t in 32bit.
        /// </summary>
        private IntPtr _unixEpochVal;
        /// <summary>
        /// Seconds since start of UNIX epoch (January 1, 1970)
        /// </summary>
        public long UnixEpoch => _unixEpochVal.ToInt64();
        /// <summary>
        /// Nanoseconds (0-999999999)
        /// </summary>
        public int NanoSeconds;

        internal DateTime ToDateTime(int high)
        {
            // C# DateTime has a resolution of 100ns
            DateTime genesis = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            genesis = genesis.AddSeconds(UnixEpoch);
            genesis = genesis.AddTicks(NanoSeconds / 100);

            // wimlib provide high 32bit separately if timespec.tv_sec is only 32bit
            if (Wim.Lib.PlatformBitness == PlatformBitness.Bit32)
            {
                long high64 = (long)high << 32;
                genesis = genesis.AddSeconds(high64);
            }

            return genesis;
        }
    }
    #endregion

    #region WimObjectId
    /// <summary>
    /// Since wimlib v1.9.1: an object ID, which is an extra piece of metadata that may be associated with a file on NTFS filesystems. 
    /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa363997(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class WimObjectId
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] ObjectId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] BirthVolumeId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] BirthObjectId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] DomainId;
    }
    #endregion

    #region ResourceEntry
    /// <summary>
    /// Information about a "blob", which is a fixed length sequence of binary data.
    /// Each nonempty stream of each file in a WIM image is associated with a blob.
    /// Blobs are deduplicated within a WIM file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class ResourceEntry
    {
        /// <summary>
        /// If this blob is not missing, then this is the uncompressed size of this blob in bytes.
        /// </summary>
        public ulong UncompressedSize;
        /// <summary>
        /// If this blob is located in a non-solid WIM resource, then this is the compressed size of that resource. 
        /// </summary>
        public ulong CompressedSize;
        /// <summary>
        /// If this blob is located in a non-solid WIM resource, then this is the offset of that resource within the WIM file containing it.
        /// If this blob is located in a solid WIM resource, then this is the offset of this blob within that solid resource when uncompressed.
        /// </summary>
        public ulong Offset;
        /// <summary>
        /// The SHA-1 message digest of the blob's uncompressed contents.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] SHA1;
        /// <summary>
        /// If this blob is located in a WIM resource, then this is the part number of the WIM file containing it.
        /// </summary>
        public uint PartNumber;
        /// <summary>
        /// If this blob is not missing, then this is the number of times this blob is referenced over all images in the WIM.
        /// This number is not guaranteed to be correct.
        /// </summary>
        public uint ReferenceCount;

        /// <summary>
        /// Bit 0 - 6 : Bool Flags
        /// Bit 7 - 31 : Reserved
        /// </summary>
        private uint _bitFlag;
        /// <summary>
        /// 1 iff this blob is located in a non-solid compressed WIM resource.
        /// </summary>
        public bool IsCompressed => WimLibLoader.GetBitField(_bitFlag, 0);
        /// <summary>
        /// 1 iff this blob contains the metadata for an image. 
        /// </summary>
        public bool IsMetadata => WimLibLoader.GetBitField(_bitFlag, 1);
        public bool IsFree => WimLibLoader.GetBitField(_bitFlag, 2);
        public bool IsSpanned => WimLibLoader.GetBitField(_bitFlag, 3);
        /// <summary>
        /// 1 iff a blob with this hash was not found in the blob lookup table of the <see cref="Wim">.
        /// This normally implies a missing call to <see cref="Wim.ReferenceResourceFiles(IEnumerable{string}, RefFlags, OpenFlags)"/> 
        /// or <see cref="Wim.ReferenceResources(IEnumerable{Wim})"/>.
        /// </summary>
        public bool IsMissing => WimLibLoader.GetBitField(_bitFlag, 4);
        /// <summary>
        /// 1 iff this blob is located in a solid resource.
        /// </summary>
        public bool Packed => WimLibLoader.GetBitField(_bitFlag, 5);
        /// <summary>
        /// If this blob is located in a solid WIM resource, then this is the offset of that solid resource within the WIM file containing it.
        /// </summary>
        public ulong RawResourceOffsetInWim;
        /// <summary>
        /// If this blob is located in a solid WIM resource, then this is the compressed size of that solid resource.
        /// </summary>
        public ulong RawResourceCompressedSize;
        /// <summary>
        /// If this blob is located in a solid WIM resource, then this is the uncompressed size of that solid resource.
        /// </summary>
        public ulong RawResourceUncompressedSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        private ulong[] _reserved;
    }
    #endregion

    #region StreamEntry
    /// <summary>
    /// Information about a stream of a particular file in the WIM.
    ///
    /// Normally, only WIM images captured from NTFS filesystems will have multiple streams per file.
    /// In practice, this is a rarely used feature of the filesystem.
    ///
    /// TODO: the library now explicitly tracks stream types,which allows it to have multiple unnamed streams
    /// (e.g. both a reparse point stream and unnamed data stream).
    /// However, this isn't yet exposed by <see cref="Wim.IterateDirTree()"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class StreamEntry
    {
        /// <summary>
        /// Name of the stream, or null if the stream is unnamed.
        /// </summary>
        public string StreamName;
        /// <summary>
        /// Info about this stream's data, such as its hash and size if known.
        /// </summary>
        public ResourceEntry Resource;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]

        private ulong[] _reserved;
    }
    #endregion
    #endregion
}