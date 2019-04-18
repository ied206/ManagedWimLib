// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable EnumUnderlyingTypeIsInt
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable 169
#pragma warning disable 414
#pragma warning disable 649
#pragma warning disable IDE0044

namespace ManagedWimLib
{
    #region Native WimLib Enums
    #region Enum CompressionType
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
    public enum CompressionType
    {
        /// <summary>
        /// No compression.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// The XPRESS compression format.
        /// This format combines Lempel-Ziv factorization with Huffman encoding.
        /// Compression and decompression are both fast. 
        /// 
        /// This format supports chunk sizes that are powers of 2 between 2^12 and 2^16, inclusively.
        /// </summary>
        XPRESS = 1,
        /// <summary>
        /// The LZX compression format.
        /// This format combines Lempel-Ziv factorization with Huffman encoding, but with more features and complexity than XPRESS.
        /// Compression is slow to somewhat fast, depending on the settings.
        /// Decompression is fast but slower than XPRESS.
        /// 
        /// This format supports chunk sizes that are powers of 2 between 2^15 and 2^21, inclusively.
        /// Note: chunk sizes other than 2^15 are not compatible with the Microsoft implementation.
        /// </summary>
        LZX = 2,
        /// <summary>
        /// The LZMS compression format.
        /// This format combines Lempel-Ziv factorization with adaptive Huffman encoding and range coding.
        /// Compression and decompression are both fairly slow.
        /// 
        /// This format supports chunk sizes that are powers of 2 between 2^15 and 2^30, inclusively.
        /// This format is best used for large chunk sizes.
        /// </summary>
        LZMS = 3,
    }
    #endregion

    #region Enum Progress
    /// <summary>
    /// Possible values of the first parameter to the user-supplied ProgressFunc callback
    /// </summary>
    public enum ProgressMsg
    {
        /// <summary>
        /// A WIM image is about to be extracted. 
        /// info will point to ProgressInfo_Extract. 
        /// This message is received once per image for calls to Wim.ExtractImage() and Wim.ExtractImageFromPipe().
        /// </summary>
        EXTRACT_IMAGE_BEGIN = 0,
        /// <summary>
        /// One or more file or directory trees within a WIM image is about to be extracted.
        /// info will point to ProgressInfo_Extract.
        /// This message is received only once per Wim.ExtractPaths() and Wim.ExtractPathList(), 
        /// since wimlib combines all paths into a single extraction operation for optimization purposes.
        /// </summary>
        EXTRACT_TREE_BEGIN = 1,
        /// <summary>
        /// This message may be sent periodically (not for every file) while files and directories are being created, 
        /// prior to file data extraction.
        /// info will point to ProgressInfo_Extract.
        /// In particular, the CurrentFileCount and EndFileCount members may be used to track the progress of this phase of extraction.
        /// </summary>
        EXTRACT_FILE_STRUCTURE = 3,
        /// <summary>
        /// File data is currently being extracted.
        /// info will point to ProgressInfo_Extract. 
        /// This is the main message to track the progress of an extraction operation.
        /// </summary>
        EXTRACT_STREAMS = 4,
        /// <summary>
        /// Starting to read a new part of a split pipable WIM over the pipe.
        /// info will point to ProgressInfo_Extract.
        /// </summary>
        EXTRACT_SPWM_PART_BEGIN = 5,
        /// <summary>
        /// This message may be sent periodically (not necessarily for every file) while file and directory metadata is being extracted,
        /// following file data extraction.
        /// info will point to ProgressInfo_Extract.
        /// The CurrentFileCount and EndFileCount members may be used to track the progress of this phase of extraction.
        /// </summary>
        EXTRACT_METADATA = 6,
        /// <summary>
        /// The image has been successfully extracted.
        /// info will point to ProgressInfo_Extract.
        /// This is paired with ProgressMsg.EXTRACT_IMAGE_BEGIN.
        /// </summary>
        EXTRACT_IMAGE_END = 7,
        /// <summary>
        /// The files or directory trees have been successfully extracted.
        /// info will point to ProgressInfo_Extract.
        /// This is paired with ProgressMsg.EXTRACT_TREE_BEGIN.
        /// </summary>
        EXTRACT_TREE_END = 8,
        /// <summary>
        /// The directory or NTFS volume is about to be scanned for metadata.
        /// info will point to ProgressInfo_Scan.
        /// This message is received once per call to Wim.AddImage(), or once per capture source passed to Wim.AddImageMultisource(), 
        /// or once per add command passed to Wim.UpdateImage().
        /// </summary>
        SCAN_BEGIN = 9,
        /// <summary>
        /// A directory or file has been scanned. 
        /// info will point to ProgressInfo_Scan, and its CurPath member will be valid.
        /// This message is only sent if AddFlags.VERBOSE has been specified.
        /// </summary>
        SCAN_DENTRY = 10,
        /// <summary>
        /// The directory or NTFS volume has been successfully scanned.
        /// info will point to ProgressInfo_Scan.
        /// This is paired with a previous ProgressMsg.SCAN_BEGIN message, 
        /// possibly with many intervening ProgressMsg.SCAN_DENTRY messages.
        /// </summary>
        SCAN_END = 11,
        /// <summary>
        /// File data is currently being written to the WIM.
        /// info will point to ProgressInfo_WriteStreams.
        /// This message may be received many times while the WIM file is being written or appended
        /// to with Wim.Write(), Wim.Overwrite(), or Wim.WriteToFd().
        /// </summary>
        WRITE_STREAMS = 12,
        /// <summary>
        /// Per-image metadata is about to be written to the WIM file.
        /// info will not be valid.
        /// </summary>
        WRITE_METADATA_BEGIN = 13,
        /// <summary>
        /// The per-image metadata has been written to the WIM file. 
        /// info will not be valid. 
        /// This message is paired with a preceding ProgressMsg.WRITE_METADATA_BEGIN message.
        /// </summary>
        WRITE_METADATA_END = 14,
        /// <summary>
        /// Wim.Overwrite() has successfully renamed the temporary file to the original WIM file, 
        /// thereby committing the changes to the WIM file. info will point to ProgressInfo_Rename. 
        /// Note: this message is not received if Wim.Overwrite() chose to append to the WIM file in-place.
        /// </summary>
        RENAME = 15,
        /// <summary>
        /// The contents of the WIM file are being checked against the integrity table.
        /// info will point to ProgressInfo_Integrity.
        /// This message is only received (and may be received many times) when Wim.OpenWim() with ProgressCallback
        /// is called with the OpenFlags.CHECK_INTEGRITY flag.
        /// </summary>
        VERIFY_INTEGRITY = 16,
        /// <summary>
        /// An integrity table is being calculated for the WIM being written.
        /// info will point to ProgressInfo_Integrity.
        /// This message is only received (and may be received many times)
        /// when a WIM file is being written with the flag WriteFlags.CHECK_INTEGRITY.
        /// </summary>
        CALC_INTEGRITY = 17,
        /// <summary>
        /// A Wim.Split() operation is in progress, and a new split part is about to be started.
        /// info will point to ProgressInfo_Split.
        /// </summary>
        SPLIT_BEGIN_PART = 19,
        /// <summary>
        /// A WIm.Split() operation is in progress, and a split part has been finished.
        /// info will point to ProgressInfo_Split.
        /// </summary>
        SPLIT_END_PART = 20,
        /// <summary>
        /// A WIM update command is about to be executed. 
        /// info will point to ProgressInfo_Update. 
        /// This message is received once per update command when Wim.UpdateImage() is called with the flag UpdateFlags.SEND_PROGRESS.
        /// </summary>
        UPDATE_BEGIN_COMMAND = 21,
        /// <summary>
        /// A WIM update command has been executed. 
        /// info will point to ProgressInfo_Update.
        /// This message is received once per update command when Wim.UpdateImage() is called with the flag UpdateFlags.SEND_PROGRESS.
        /// </summary>
        UPDATE_END_COMMAND = 22,
        /// <summary>
        /// A file in the image is being replaced as a result of a UpdateCommand.Add without AddFlags.NO_REPLACE specified.
        /// info will point to ProgressInfo_Replace.
        /// This is only received when AddFlags.VERBOSE is also specified in the add command.
        /// </summary>
        REPLACE_FILE_IN_WIM = 23,
        /// <summary>
        /// An image is being extracted with ExtractFlags.WIMBOOT, and a file is being extracted normally (not as a "WIMBoot pointer file")
        /// due to it matching a pattern in the PrepopulateList section of the configuration file
        /// /Windows/System32/WimBootCompress.ini in the WIM image.
        /// info will point to ProgressInfo_WimBootExclude.
        /// </summary>
        WIMBOOT_EXCLUDE = 24,
        /// <summary>
        /// Starting to unmount an image. 
        /// info will point to ProgressInfo_Unmount.
        /// </summary>
        UNMOUNT_BEGIN = 25,
        /// <summary>
        /// wimlib has used a file's data for the last time (including all data streams, if it has multiple).
        /// info will point to ProgressInfo_DoneWithFile.
        /// This message is only received if WriteFlags.SEND_DONE_WITH_FILE_MESSAGES was provided.
        /// </summary>
        DONE_WITH_FILE = 26,
        /// <summary>
        /// Wim.VerifyWim() is starting to verify the metadata for an image.
        /// info will point to ProgressInfo_VerifyImage.
        /// </summary>
        BEGIN_VERIFY_IMAGE = 27,
        /// <summary>
        /// Wim.VerifyWim() has finished verifying the metadata for an image. 
        /// info will point to ProgressInfo_VerifyImage.
        /// </summary>
        END_VERIFY_IMAGE = 28,
        /// <summary>
        /// Wim.VerifyWim() is verifying file data integrity.
        /// info will point to ProgressInfo_VerifyStreams.
        /// </summary>
        VERIFY_STREAMS = 29,
        /// <summary>
        /// The progress function is being asked whether a file should be excluded from capture or not. 
        /// info will point to ProgressInfo_TestFileExclusion.
        /// This is a bidirectional message that allows the progress function to set a flag if the file should be excluded.
        ///
        /// This message is only received if the flag AddFlags.TEST_FILE_EXCLUSION is used.
        /// This method for file exclusions is independent of the "capture configuration file" mechanism.
        /// </summary>
        TEST_FILE_EXCLUSION = 30,
        /// <summary>
        /// An error has occurred and the progress function is being asked whether to ignore the error or not.
        /// info will point to ProgressInfo_HandleError.
        /// This is a bidirectional message.
        /// 
        /// This message provides a limited capability for applications to recover from "unexpected" errors
        /// (i.e. those with no in-library handling policy) arising from the underlying operating system.
        /// Normally, any such error will cause the library to abort the current operation. 
        /// By implementing a handler for this message, the application can instead choose to ignore a given error.
        /// 
        /// Currently, only the following types of errors will result in this progress message being sent:
        /// 
        /// 	- Directory tree scan errors, e.g. from Wim.AddImage()
        /// 	- Most extraction errors; currently restricted to the Windows build of the library only.
        /// </summary>
        HANDLE_ERROR = 31,
    }

    /// <summary>
    /// A pointer to this union is passed to the user-supplied ProgressFunc callback.
    /// One (or none) of the structures contained in this union will be applicable for the operation (ProgressMsg) 
    /// indicated in the first argument to the callback.
    /// </summary>
    public enum CallbackStatus : int
    {
        /// <summary>
        /// The operation should be continued.  This is the normal return value.
        /// </summary>
        CONTINUE = 0,
        /// <summary>
        /// he operation should be aborted.  This will cause the current
        /// operation to fail with ErrorCode.ABORTED_BY_PROGRESS.
        /// </summary>
        ABORT = 1,
    }
    #endregion

    #region Enum ErrorCode 
    public enum ErrorCode : int
    {
        SUCCESS = 0,
        ALREADY_LOCKED = 1,
        DECOMPRESSION = 2,
        FUSE = 6,
        GLOB_HAD_NO_MATCHES = 8,
        /// <summary>
        /// The number of metadata resources found in the WIM did not match the image count specified in the WIM header, 
        /// or the number of &lt;IMAGE&gt; elements in the XML data of the WIM did not match the image count specified in the WIM header.
        /// </summary>
        IMAGE_COUNT = 10,
        IMAGE_NAME_COLLISION = 11,
        INSUFFICIENT_PRIVILEGES = 12,
        /// <summary>
        /// OpenFlags.CHECK_INTEGRITY was specified in openFlags, and the WIM file failed the integrity check.
        /// </summary>
        INTEGRITY = 13,
        INVALID_CAPTURE_CONFIG = 14,
        /// <summary>
        /// The library did not recognize the compression chunk size of the WIM as valid for its compression type.
        /// </summary>
        INVALID_CHUNK_SIZE = 15,
        /// <summary>
        /// The library did not recognize the compression type of the WIM.
        /// </summary>
        INVALID_COMPRESSION_TYPE = 16,
        /// <summary>
        /// The header of the WIM was otherwise invalid.
        /// </summary>
        INVALID_HEADER = 17,
        INVALID_IMAGE = 18,
        /// <summary>
        /// OpenFlags.CHECK_INTEGRITY was specified in openFlags and the WIM contained an integrity table,
        /// but the integrity table was invalid.
        /// </summary>
        INVALID_INTEGRITY_TABLE = 19,
        /// <summary>
        /// The lookup table of the WIM was invalid.
        /// </summary>
        INVALID_LOOKUP_TABLE_ENTRY = 20,
        INVALID_METADATA_RESOURCE = 21,
        INVALID_OVERLAY = 23,
        /// <summary>
        /// WimStruct was null; or, wimFile was not a nonempty string.
        /// </summary>
        INVALID_PARAM = 24,
        INVALID_PART_NUMBER = 25,
        INVALID_PIPABLE_WIM = 26,
        INVALID_REPARSE_DATA = 27,
        INVALID_RESOURCE_HASH = 28,
        INVALID_UTF16_STRING = 30,
        INVALID_UTF8_STRING = 31,
        IS_DIRECTORY = 32,
        /// <summary>
        /// The WIM was a split WIM and OpenFlags.ERROR_IF_SPLIT was specified in openFlags.
        /// </summary>
        IS_SPLIT_WIM = 33,
        LINK = 35,
        METADATA_NOT_FOUND = 36,
        MKDIR = 37,
        MQUEUE = 38,
        NOMEM = 39,
        NOTDIR = 40,
        NOTEMPTY = 41,
        NOT_A_REGULAR_FILE = 42,
        /// <summary>
        /// The file did not begin with the magic characters that identify a WIM file.
        /// </summary>
        NOT_A_WIM_FILE = 43,
        NOT_PIPABLE = 44,
        NO_FILENAME = 45,
        NTFS_3G = 46,
        /// <summary>
        /// Failed to open the WIM file for reading. 
        /// Some possible reasons: the WIM file does not exist, or the calling process does not have permission to open it.
        /// </summary>
        OPEN = 47,
        /// <summary>
        /// Failed to read data from the WIM file.
        /// </summary>
        OPENDIR = 48,
        PATH_DOES_NOT_EXIST = 49,
        READ = 50,
        READLINK = 51,
        RENAME = 52,
        REPARSE_POINT_FIXUP_FAILED = 54,
        RESOURCE_NOT_FOUND = 55,
        RESOURCE_ORDER = 56,
        SET_ATTRIBUTES = 57,
        SET_REPARSE_DATA = 58,
        SET_SECURITY = 59,
        SET_SHORT_NAME = 60,
        SET_TIMESTAMPS = 61,
        SPLIT_INVALID = 62,
        STAT = 63,
        /// <summary>
        /// Unexpected end-of-file while reading data from the WIM file.
        /// </summary>
        UNEXPECTED_END_OF_FILE = 65,
        UNICODE_STRING_NOT_REPRESENTABLE = 66,
        /// <summary>
        /// The WIM version number was not recognized. (May be a pre-Vista WIM.)
        /// </summary>
        UNKNOWN_VERSION = 67,
        UNSUPPORTED = 68,
        UNSUPPORTED_FILE = 69,
        /// <summary>
        /// OpenFlags.WRITE_ACCESS was specified but the WIM file was considered read-only because of
        /// any of the reasons mentioned in the documentation for the OpenFlags.WRITE_ACCESS flag.
        /// </summary>
        WIM_IS_READONLY = 71,
        WRITE = 72,
        /// <summary>
        /// The XML data of the WIM was invalid.
        /// </summary>
        XML = 73,
        /// <summary>
        /// The WIM cannot be opened because it contains encrypted segments. (It may be a Windows 8+ "ESD" file.)
        /// </summary>
        WIM_IS_ENCRYPTED = 74,
        WIMBOOT = 75,
        ABORTED_BY_PROGRESS = 76,
        UNKNOWN_PROGRESS_STATUS = 77,
        MKNOD = 78,
        MOUNTED_IMAGE_IS_BUSY = 79,
        NOT_A_MOUNTPOINT = 80,
        NOT_PERMITTED_TO_UNMOUNT = 81,
        FVE_LOCKED_VOLUME = 82,
        UNABLE_TO_READ_CAPTURE_CONFIG = 83,
        /// <summary>
        /// The WIM file is not complete (e.g. the program which wrote it was terminated before it finished)
        /// </summary>
        WIM_IS_INCOMPLETE = 84,
        COMPACTION_NOT_POSSIBLE = 85,
        IMAGE_HAS_MULTIPLE_REFERENCES = 86,
        DUPLICATE_EXPORTED_IMAGE = 87,
        CONCURRENT_MODIFICATION_DETECTED = 88,
        SNAPSHOT_FAILURE = 89,
        INVALID_XATTR = 90,
        SET_XATTR = 91,
    }
    #endregion

    #region Enum IterateFlags
    [Flags]
    public enum IterateFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// For Wim.IterateDirTree():
        /// Iterate recursively on children rather than just on the specified path.
        /// </summary>
        RECURSIVE = 0x00000001,
        /// <summary>
        /// For Wim.IterateDirTree():
        /// Don't iterate on the file or directory itself; only its children (in the case of a non-empty directory)
        /// </summary>
        CHILDREN = 0x00000002,
        /// <summary>
        /// Return ErrorCode.RESOURCE_NOT_FOUND if any file data blobs needed to fill in the ResourceEntry's for the iteration
        /// cannot be found in the blob lookup table of the WimStruct.
        /// The default behavior without this flag is to fill in the ResourceEntry.SHA1 and set ResourceEntry.IsMissing" flag.
        /// </summary>
        RESOURCES_NEEDED = 0x00000004,
    }
    #endregion

    #region Enum AddFlags
    [Flags]
    public enum AddFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// UNIX-like systems only:
        /// Directly capture an NTFS volume rather than a generic directory.
        /// This requires that wimlib was compiled with support for libntfs-3g.
        ///
        /// This flag cannot be combined with AddFlags.DEREFERENCE or AddFlags.UNIX_DATA.
        ///
        /// Do not use this flag on Windows,
        /// where wimlib already supports all Windows-native filesystems, including NTFS, through the Windows APIs.
        /// </summary>
        NTFS = 0x00000001,
        /// <summary>
        /// Follow symbolic links when scanning the directory tree.
        /// Currently only supported on UNIX-like systems.
        /// </summary>
        DEREFERENCE = 0x00000002,
        /// <summary>
        /// Call the progress function with the message ProgressMsg.SCAN_DENTRY when each directory or file has been scanned.
        /// </summary>
        VERBOSE = 0x00000004,
        /// <summary>
        /// Mark the image being added as the bootable image of the WIM.
        /// This flag is valid only for Wim.AddImage() and Wim.AddImageMultisource().
        ///
        /// Note that you can also change the bootable image of a WIM using Wim.SetWimInfo().
        ///
        /// Note: AddFlags.BOOT does something different from, and independent from, AddFlags.WIMBOOT.
        /// </summary>
        BOOT = 0x00000008,
        /// <summary>
        /// UNIX-like systems only:
        /// Store the UNIX owner, group, mode, and device ID (major and minor number) of each file.
        /// In addition, capture special files such as device nodes and FIFOs. 
        /// Since wimlib v1.11.0, on Linux also capture extended attributes.
        /// See the documentation for the "--unix-data" option to wimcapture for more information.
        /// </summary>
        UNIX_DATA = 0x00000010,
        /// <summary>
        /// Do not capture security descriptors.
        /// Only has an effect in NTFS-3G capture mode, or in Windows native builds.
        /// </summary>
        NO_ACLS = 0x00000020,
        /// <summary>
        /// Fail immediately if the full security descriptor of any file or directory cannot be accessed.  
        /// Only has an effect in Windows native builds.
        /// The default behavior without this flag is to first try omitting the SACL from the security descriptor,
        /// then to try omitting the security descriptor entirely.
        /// </summary>
        STRICT_ACLS = 0x00000040,
        /// <summary>
        /// Call the progress function with the message ProgressMsg.SCAN_DENTRY when a directory or file is excluded from capture.
        /// This is a subset of the messages provided by AddFlags.VERBOSE.
        /// </summary>
        EXCLUDE_VERBOSE = 0x00000080,
        /// <summary>
        /// Reparse-point fixups:
        /// Modify absolute symbolic links (and junctions, in the case of Windows) that point inside the directory
        /// being captured to instead be absolute relative to the directory being captured.
        ///
        /// Without this flag, the default is to do reparse-point fixups if WIM_HDR_FLAG_RP_FIX is set in the WIM header
        /// or if this is the first image being added.
        /// </summary>
        RPFIX = 0x00000100,
        /// <summary>
        /// Don't do reparse point fixups. See AddFlags.RPFIX.
        /// </summary>
        NORPFIX = 0x00000200,
        /// <summary>
        /// Do not automatically exclude unsupported files or directories from capture,
        /// such as encrypted files in NTFS-3G capture mode, or device files and FIFOs on
        /// UNIX-like systems when not also using AddFlags.UNIX_DATA.  
        /// Instead, fail with ErrorCode.UNSUPPORTED_FILE when such a file is encountered.
        /// </summary>
        NO_UNSUPPORTED_EXCLUDE = 0x00000400,
        /// <summary>
        /// Automatically select a capture configuration appropriate for capturing filesystems containing Windows operating systems.
        /// For example, "/pagefile.sys" and "/System Volume Information" will be excluded.
        ///
        /// When this flag is specified, the corresponding config parameter (for Wim.AddImage()) or member (for Wim.UpdateImage()) must be null.
        /// Otherwise, ErrorCode.INVALID_PARAM will be returned.
        ///
        /// Note that the default behavior ---that is, when neither AddFlags.WINCONFIG nor AddFlags.WIMBOOT is specified and config is null---
        /// is to use no capture configuration, meaning that no files are excluded from capture.
        /// </summary>
        WINCONFIG = 0x00000800,
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
        /// wimlib_set_output_compression_type(wim, CompressType.XPRESS);
        /// wimlib_set_output_chunk_size(wim, 4096);
        ///
        /// However, "WIMBoot" also works with other XPRESS chunk sizes as well as LZX with 32768 byte chunks.
        ///
        /// Note: AddFlags.WIMBOOT does something different from, and independent from, AddFlags.BOOT.
        ///
        /// Since wimlib v1.8.3, AddFlags.WIMBOOT also causes offline WIM-backed files to be added as the "real" files
        /// rather than as their reparse points, provided that their data is already present in the WIM. 
        /// This feature can be useful when updating a backing WIM file in an "offline" state.
        /// </summary>
        WIMBOOT = 0x00001000,
        /// <summary>
        /// If the add command involves adding a non-directory file to a location at which there already exists
        /// a nondirectory file in the image, issue ErrorCode.INVALID_OVERLAY instead of replacing the file.
        /// This was the default behavior before wimlib v1.7.0.
        /// </summary>
        NO_REPLACE = 0x00002000,
        /// <summary>
        /// Send ProgressMsg.TEST_FILE_EXCLUSION messages to the progress function.
        ///
        /// Note: This method for file exclusions is independent from the capture configuration file mechanism.
        /// </summary>
        TEST_FILE_EXCLUSION = 0x00004000,
        /// <summary>
        /// Since wimlib v1.9.0: create a temporary filesystem snapshot of the source directory and add the files from it.
        /// Currently, this option is only supported on Windows, where it uses the Volume Shadow Copy Service (VSS).
        /// Using this option, you can create a consistent backup of the system volume of
        /// a running Windows system without running into problems with locked files.
        /// For the VSS snapshot to be successfully created, your application must be run as an Administrator, 
        /// and it cannot be run in WoW64 mode (i.e. if Windows is 64-bit, then your application must be 64-bit as well).
        /// </summary>
        SNAPSHOT = 0x00008000,
        /// <summary>
        /// Since wimlib v1.9.0: permit the library to discard file paths after the initial scan. 
        /// If the application won't use WriteFlags.SEND_DONE_WITH_FILE_MESSAGES while writing the WIM archive, 
        /// this flag can be used to allow the library to enable optimizations such as opening files by inode number rather than by path.
        /// Currently this only makes a difference on Windows.
        /// </summary>
        FILE_PATHS_UNNEEDED = 0x00010000,
    }
    #endregion

    #region Enum ChangeFlags
    [Flags]
    public enum ChangeFlags : int
    {
        /// <summary>
        /// Set or unset the "readonly" WIM header flag (WIM_HDR_FLAG_READONLY in Microsoft's documentation),
        /// based on the WimInfo.IsMarkedReadonly member of the info parameter.
        /// This is distinct from basic file permissions; this flag can be set on a WIM file that is physically writable.
        ///
        /// wimlib disallows modifying on-disk WIM files with the readonly flag set.
        /// However, Wim.Overwrite() with WriteFlags.IGNORE_READONLY_FLAG will override this ---
        /// and in fact, this is necessary to set the readonly flag persistently on an existing WIM file.
        /// </summary>
        READONLY_FLAG = 0x00000001,
        /// <summary>
        /// Set the GUID (globally unique identifier) of the WIM file to the value specified in WimInfo.Guid of the info parameter.
        /// </summary>
        GUID = 0x00000002,
        /// <summary>
        /// Change the bootable image of the WIM to the value specified in WimInfo.BootIndex of the info parameter.
        /// </summary>
        BOOT_INDEX = 0x00000004,
        /// <summary>
        /// Change the WIM_HDR_FLAG_RP_FIX flag of the WIM file to the value specified in WimInfo.HasRpfix of the info parameter.
        /// This flag generally indicates whether an image in the WIM has been captured with reparse-point fixups enabled.
        /// wimlib also treats this flag as specifying whether to do reparse-point fixups by default
        /// when capturing or applying WIM images.
        /// </summary>
        RPFIX_FLAG = 0x00000008
    }
    #endregion

    #region Enum DeleteFlags
    [Flags]
    public enum DeleteFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Do not issue an error if the path to delete does not exist.
        /// </summary>
        FORCE = 0x00000001,
        /// <summary>
        /// Delete the file or directory tree recursively; if not specified, an error is issued if the path to delete is a directory.
        /// </summary>
        RECURSIVE = 0x00000002,
    }
    #endregion

    #region Enum ExportFlags
    [Flags]
    public enum ExportFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// If a single image is being exported, mark it bootable in the destination WIM.
        /// Alternatively, if Wim.AllImages is specified as the image to export,
        /// the image in the source WIM (if any) that is marked as bootable is also
        /// marked as bootable in the destination WIM.
        /// </summary>
        BOOT = 0x00000001,
        /// <summary>
        /// Give the exported image(s) no names. 
        /// Avoids problems with image name collisions.
        /// </summary>
        NO_NAMES = 0x00000002,
        /// <summary>
        /// Give the exported image(s) no descriptions.
        /// </summary>
        NO_DESCRIPTIONS = 0x00000004,
        /// <summary>
        /// This advises the library that the program is finished with the source
        /// WIMStruct and will not attempt to access it after the call to
        /// Wim.ExportImage(), with the exception of the call to Wim.Free().
        /// </summary>
        GIFT = 0x00000008,
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
        WIMBOOT = 0x00000010,
    }
    #endregion

    #region Enum ExtractFlags
    [Flags]
    public enum ExtractFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Extract the image directly to an NTFS volume rather than a generic directory.
        /// This mode is only available if wimlib was compiled with libntfs-3g support;
        /// if not, ErrorCode.UNSUPPORTED will be returned.
        /// In this mode, the extraction target will be interpreted as the path to an NTFS volume image
        /// (as a regular file or block device) rather than a directory.
        /// It will be opened using libntfs-3g, and the image will be extracted to the NTFS filesystem's root directory.
        /// Note: this flag cannot be used when Wim.ExtractImage() is called with Wim.AllImages as the image,
        /// nor can it be used with Wim.ExtractPaths() when passed multiple paths.
        /// </summary>
        NTFS = 0x00000001,
        /// <summary>
        /// UNIX-like systems only:
        /// Extract UNIX-specific metadata captured with AddFlags.UNIX_DATA.
        /// </summary>
        UNIX_DATA = 0x00000020,
        /// <summary>
        /// Do not extract security descriptors.
        /// This flag cannot be combined with ExtractFlags.STRICT_ACLS.
        /// </summary>
        NO_ACLS = 0x00000040,
        /// <summary>
        /// Fail immediately if the full security descriptor of any file or directory
        /// cannot be set exactly as specified in the WIM image.
        /// On Windows, the default behavior without this flag when wimlib does not have permission to set the
        /// correct security descriptor is to fall back to setting the security descriptor with the SACL omitted,
        /// then with the DACL omitted, then with the owner omitted, then not at all.
        /// This flag cannot be combined with ExtractFlags.NO_ACLS.
        /// </summary>
        STRICT_ACLS = 0x00000080,
        /// <summary>
        /// This is the extraction equivalent to AddFlags.RPFIX.
        /// This forces reparse-point fixups on, so absolute symbolic links or junction points will
        /// be fixed to be absolute relative to the actual extraction root.
        /// Reparse-point fixups are done by default for Wim.ExtractImage() and Wim.ExtractImageFromPipe()
        /// if WIM_HDR_FLAG_RP_FIX is set in the WIM header.
        /// This flag cannot be combined with ExtractFlags.NORPFIX.
        /// </summary>
        RPFIX = 0x00000100,
        /// <summary>
        /// Force reparse-point fixups on extraction off, regardless of the state of the WIM_HDR_FLAG_RP_FIX flag in the WIM header.
        /// This flag cannot be combined with ExtractFlags.RPFIX.
        /// </summary>
        NORPFIX = 0x00000200,
        /// <summary>
        /// For Wim.ExtractPaths() and Wim.ExtractPathList() only:
        /// Extract the paths, each of which must name a regular file, to standard output.
        /// </summary>
        TO_STDOUT = 0x00000400,
        /// <summary>
        /// Instead of ignoring files and directories with names that cannot be represented on the current platform
        /// (note: Windows has more restrictions on filenames than POSIX-compliant systems),
        /// try to replace characters or append junk to the names so that they can be extracted in some form.
        ///
        /// Note: this flag is unlikely to have any effect when extracting a WIM image that was captured on Windows.
        /// </summary>
        REPLACE_INVALID_FILENAMES = 0x00000800,
        /// <summary>
        /// On Windows, when there exist two or more files with the same case insensitive name but different case sensitive names,
        /// try to extract them all by appending junk to the end of them, rather than arbitrarily extracting only one.
        ///
        /// Note: this flag is unlikely to have any effect when extracting a WIM image that was captured on Windows.
        /// </summary>
        ALL_CASE_CONFLICTS = 0x00001000,
        /// <summary>
        /// Do not ignore failure to set timestamps on extracted files.
        /// This flag currently only has an effect when extracting to a directory on UNIX-like systems.
        /// </summary>
        STRICT_TIMESTAMPS = 0x00002000,
        /// <summary>
        /// Do not ignore failure to set short names on extracted files.
        /// This flag currently only has an effect on Windows.
        /// </summary>
        STRICT_SHORT_NAMES = 0x00004000,
        /// <summary>
        /// Do not ignore failure to extract symbolic links and junctions due to permissions problems.
        /// This flag currently only has an effect on Windows. 
        /// By default, such failures are ignored since the default configuration of Windows 
        /// only allows the Administrator to create symbolic links.
        /// </summary>
        STRICT_SYMLINKS = 0x00008000,
        /// <summary>
        /// For Wim.ExtractPaths() and Wim.ExtractPathList() only:
        /// Treat the paths to extract as wildcard patterns ("globs") which may contain the wildcard characters '?' and '*'.
        /// The '?' character matches any non-path-separator character, whereas the '*' character matches zero or more
        /// non-path-separator characters.
        /// Consequently, each glob may match zero or more actual paths in the WIM image.
        ///
        /// By default, if a glob does not match any files, a warning but not an error will be issued.
        /// This is the case even if the glob did not actually contain wildcard characters. 
        /// Use ExtractFlags.STRICT_GLOB to get an error instead.
        /// </summary>
        GLOB_PATHS = 0x00040000,
        /// <summary>
        /// In combination with ExtractFlags.GLOB_PATHS, causes an error (ErrorCode.PATH_DOES_NOT_EXIST)
        /// rather than a warning to be issued when one of the provided globs did not match a file.
        /// </summary>
        STRICT_GLOB = 0x00080000,
        /// <summary>
        /// Do not extract Windows file attributes such as readonly, hidden, etc.
        ///
        /// This flag has an effect on Windows as well as in the NTFS-3G extraction mode.
        /// </summary>
        NO_ATTRIBUTES = 0x00100000,
        /// <summary>
        /// For Wim.ExtractPaths() and Wim.ExtractPathList() only: 
        /// Do not preserve the directory structure of the archive when extracting --- that is,
        /// place each extracted file or directory tree directly in the target directory.
        /// The target directory will still be created if it does not already exist.
        /// </summary>
        NO_PRESERVE_DIR_STRUCTURE = 0x00200000,
        /// <summary>
        /// Windows only: Extract files as "pointers" back to the WIM archive.
        ///
        /// The effects of this option are fairly complex.
        /// See the documentation for the "--wimboot" option of "wimapply" for more information.
        /// </summary>
        WIMBOOT = 0x00400000,
        /// <summary>
        /// Since wimlib v1.8.2 and Windows-only:
        /// compress the extracted files using System Compression, when possible. 
        /// This only works on either Windows 10 or later, or on an older Windows to which Microsoft's wofadk.sys driver has been added.
        /// Several different compression formats may be used with System Compression;
        /// this particular flag selects the XPRESS compression format with 4096 byte chunks.
        /// </summary>
        COMPACT_XPRESS4K = 0x01000000,
        /// <summary>
        /// Like ExtractFlags.COMPACT_XPRESS4K, but use XPRESS compression with 8192 byte chunks.
        /// </summary>
        COMPACT_XPRESS8K = 0x02000000,
        /// <summary>
        /// Like ExtractFlags.COMPACT_XPRESS4K, but use XPRESS compression with 16384 byte chunks.
        /// </summary>
        COMPACT_XPRESS16K = 0x04000000,
        /// <summary>
        /// Like ExtractFlags.COMPACT_XPRESS4K, but use LZX compression with 32768 byte chunks.
        /// </summary>
        COMPACT_LZX = 0x08000000,
    }
    #endregion

    #region Enum MountFlags (Linux Only)
    [Flags]
    public enum MountFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Mount the WIM image read-write rather than the default of read-only.
        /// </summary>
        READWRITE = 0x00000001,
        /// <summary>
        /// Enable FUSE debugging by passing the -d option to fuse_main().
        /// </summary>
        DEBUG = 0x00000002,
        /// <summary>
        /// Do not allow accessing named data streams in the mounted WIM image.
        /// </summary>
        STREAM_INTERFACE_NONE = 0x00000004,
        /// <summary>
        /// Access named data streams in the mounted WIM image through extended file
        /// attributes named "user.X", where X is the name of a data stream. 
        /// This is the default mode.
        /// </summary>
        STREAM_INTERFACE_XATTR = 0x00000008,
        /// <summary>
        /// Access named data streams in the mounted WIM image by specifying the file
        /// name, a colon, then the name of the data stream.
        /// </summary>
        STREAM_INTERFACE_WINDOWS = 0x00000010,
        /// <summary>
        /// Support UNIX owners, groups, modes, and special files.
        /// </summary>
        UNIX_DATA = 0x00000020,
        /// <summary>
        /// Allow other users to see the mounted filesystem.
        /// This passes the allow_other option to fuse_main().
        /// </summary>
        ALLOW_OTHER = 0x00000040,
    }
    #endregion

    #region Enum OpenFlags
    [Flags]
    public enum OpenFlags : int
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Verify the WIM contents against the WIM's integrity table, if present.
        /// The integrity table stores checksums for the raw data of the WIM file, divided into fixed size chunks.
        /// Verification will compute checksums and compare them with the stored values.
        /// If there are any mismatches, then ErrorCode.INTEGRITY will be issued. 
        /// If the WIM file does not contain an integrity table, then this flag has no effect.
        /// </summary>
        CHECK_INTEGRITY = 0x00000001,
        /// <summary>
        /// Issue an error (ErrorCode.IS_SPLIT_WIM) if the WIM is part of a split WIM. 
        /// Software can provide this flag for convenience if it explicitly does not want to support split WIMs.
        /// </summary>
        ERROR_IF_SPLIT = 0x00000002,
        /// <summary>
        /// Check if the WIM is writable and issue an error (ErrorCode.WIM_IS_READONLY) if it is not.
        /// A WIM is considered writable only if it is writable at the filesystem level, does not have the
        /// "WIM_HDR_FLAG_READONLY" flag set in its header, and is not part of a spanned set. 
        /// It is not required to provide this flag before attempting to make changes to the WIM,
        /// but with this flag you get an error immediately rather than potentially much later,
        /// when Wim.Overwrite() is finally called.
        /// </summary>
        WRITE_ACCESS = 0x00000004,
    }
    #endregion

    #region Enum UnmountFlags (Linux Only)
    [Flags]
    public enum UnmountFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Provide WriteFlags.CHECK_INTEGRITY when committing the WIM image.
        /// Ignored if UnmountFlags.COMMIT not also specified.
        /// </summary>
        CHECK_INTEGRITY = 0x00000001,
        /// <summary>
        /// Commit changes to the read-write mounted WIM image.
        /// If this flag is not specified, changes will be discarded.
        /// </summary>
        COMMIT = 0x00000002,
        /// <summary>
        /// Provide WriteFlags.REBUILD when committing the WIM image.
        /// Ignored if UnmountFlags.COMMIT not also specified.
        /// </summary>
        REBUILD = 0x00000004,
        /// <summary>
        /// Provide WriteFlags.RECOMPRESS when committing the WIM image.
        /// Ignored if UnmountFlags.COMMIT not also specified.
        /// </summary>
        RECOMPRESS = 0x00000008,
        /// <summary>
        /// In combination with :UnmountFlags.COMMIT for a read-write mounted WIM image,
        /// forces all file descriptors to the open WIM image to be closed before committing it.
        /// </summary>
        /// <remarks>
        /// Without UnmountFlags.COMMIT or with a read-only mounted WIM image, this flag has no effect.
        /// </remarks>
        FORCE = 0x00000010,
        /// <summary>
        /// In combination with UnmountFlags.COMMIT for a read-write mounted WIM image,
        /// causes the modified image to be committed to the WIM file as a new, unnamed image appended to the archive.
        /// The original image in the WIM file will be unmodified.
        /// </summary>
        NEW_IMAGE = 0x00000020,
    }
    #endregion

    #region Enum UpdateFlags
    [Flags]
    public enum UpdateFlags : uint
    {
        /// <summary>
        /// Send ProgressMsg.UPDATE_BEGIN_COMMAND and ProgressMsg.UPDATE_END_COMMAND messages.
        /// </summary>
        SEND_PROGRESS = 0x00000001,
    }
    #endregion

    #region Enum WriteFlags
    [Flags]
    public enum WriteFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Include an integrity table in the resulting WIM file.
        ///
        /// For WimStruct's created with Wim.OpenWim(), the default behavior is to
        /// include an integrity table if and only if one was present before. 
        /// For WimStruct's created with Wim.CreateNewWim(), the default behavior is to not include an integrity table.
        /// </summary>
        CHECK_INTEGRITY = 0x00000001,
        /// <summary>
        /// Do not include an integrity table in the resulting WIM file.
        /// This is the default behavior, unless the WimStruct was created by opening a WIM with an integrity table.
        /// </summary>
        NO_CHECK_INTEGRITY = 0x00000002,
        /// <summary>
        /// Write the WIM as "pipable".
        /// After writing a WIM with this flag specified, images from it can be applied directly from a pipe using Wim.ExtractImageFromPipe().
        /// See the documentation for the "--pipable" option of "wimcapture" for more information.
        /// Beware: WIMs written with this flag will not be compatible with Microsoft's software.
        ///
        /// For WimStruct's created with Wim.OpenWim(), the default behavior is to write the WIM as pipable if and only if it was pipable before.
        /// For WimStruct's created with Wim.CreateNewWim(), the default behavior is to write the WIM as non-pipable.
        /// </summary>
        PIPABLE = 0x00000004,
        /// <summary>
        /// Do not write the WIM as "pipable".
        /// This is the default behavior, unless the WimStruct was created by opening a pipable WIM.
        /// </summary>
        NOT_PIPABLE = 0x00000008,
        /// <summary>
        /// When writing data to the WIM file, recompress it, even if the data is already available in the desired compressed form
        /// (for example, in a WIM file from which an image has been exported using Wim.ExportImage()).
        ///
        /// WriteFlags.RECOMPRESS can be used to recompress with a higher compression ratio for the same compression type and chunk size.
        /// Simply using/ the default compression settings may suffice for this, especially if the WIM
        /// file was created using another program/library that may not use as sophisticated compression algorithms.
        /// Or, Wim.SetDefaultCompressionLevel() can be called beforehand to set an even higher compression level than the default.
        ///
        /// If the WIM contains solid resources, then WriteFlags.RECOMPRESS can be used in
        /// combination with WriteFlags.SOLID to prevent any solid resources from being re-used.
        /// Otherwise, solid resources are re-used somewhat more liberally than normal compressed resources.
        ///
        /// WriteFlags.RECOMPRESS does not cause recompression of data that would not otherwise be written.
        /// For example, a call to Wim.Overwrite() with WriteFlags.RECOMPRESS will not, by itself,
        /// cause already-existing data in the WIM file to be recompressed.
        /// To force the WIM file to be fully rebuilt and recompressed, combine WriteFlags.RECOMPRESS with WriteFlags.REBUILD.
        /// </summary>
        RECOMPRESS = 0x00000010,
        /// <summary>
        /// Immediately before closing the WIM file, sync its data to disk.
        ///
        /// This flag forces the function to wait until the data is safely on disk before returning success.
        /// Otherwise, modern operating systems tend to cache data for some time (in some cases, 30+ seconds)
        /// before actually writing it to disk, even after reporting to the application that the writes have succeeded.
        ///
        /// Wim.Overwrite() will set this flag automatically if it decides to overwrite the WIM file via a temporary file instead of in-place.
        /// This is necessary on POSIX systems; it will, for example, avoid problems with delayed allocation on ext4.
        /// </summary>
        FSYNC = 0x00000020,
        /// <summary>
        /// For Wim.Overwrite():
        /// rebuild the entire WIM file, even if it otherwise could be updated in-place by appending to it.
        /// Any data that existed in the original WIM file but is not actually needed by any of the remaining images will not be included.
        /// This can free up space left over after previous in-place modifications to the WIM file.
        ///
        /// This flag can be combined with WriteFlags.RECOMPRESS to force all data to be recompressed. 
        /// Otherwise, compressed data is re-used if possible.
        ///
        /// Wim.Write() ignores this flag.
        /// </summary>
        REBUILD = 0x00000040,
        /// <summary>
        /// For Wim.Overwrite():
        /// override the default behavior after one or more calls to Wim.DeleteImage(), which is to rebuild the entire WIM file.
        /// With this flag, only minimal changes to correctly remove the image from the WIM file will be taken. 
        /// This can be much faster, but it will result in the WIM file getting larger rather than smaller.
        ///
        /// Wim.Write() ignores this flag.
        /// </summary>
        SOFT_DELETE = 0x00000080,
        /// <summary>
        /// For Wim.Overwrite(), allow overwriting the WIM file even if the readonly flag (WIM_HDR_FLAG_READONLY) is set in the WIM header.
        /// This can be used following a call to Wim.SetWimInfo() with the WIMLIB_CHANGE_READONLY_FLAG flag to 
        /// actually set the readonly flag on the on-disk WIM file.
        ///
        /// Wim.Write() ignores this flag.
        /// </summary>
        IGNORE_READONLY_FLAG = 0x00000100,
        /// <summary>
        /// Do not include file data already present in other WIMs.
        /// This flag can be used to write a "delta" WIM after the WIM files on which the delta is to be
        /// based were referenced with Wim.ReferenceResourceFiles() or Wim.ReferenceResources().
        /// </summary>
        SKIP_EXTERNAL_WIMS = 0x00000200,
        /// <summary>
        /// For Wim.Write(), retain the WIM's GUID instead of generating a new one.
        ///
        /// Wim.Overwrite() sets this by default, since the WIM remains, logically, the same file.
        /// </summary>
        RETAIN_GUID = 0x00000800,
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
        /// Note that providing this flag does not affect the "append by default" behavior of Wim.Overwrite(). 
        /// In other words, Wim.Overwrite() with just WriteFlags.SOLID can be used to append solid-compressed data to a
        /// WIM file that originally did not contain any solid-compressed data. 
        /// But if you instead want to rebuild and recompress an entire WIM file in solid mode,
        /// then also provide WriteFlags.REBUILD and WriteFlags.RECOMPRESS.
        ///
        /// Currently, new solid resources will, by default, be written using LZMS compression with 64 MiB (67108864 byte) chunks. 
        /// Use Wim.SetOutputPackCompressionType() and/or Wim.SetOutputPackChunkSize() to change this.
        /// This is independent of the WIM's main compression type and chunk size;
        /// you can have a WIM that nominally uses LZX compression and 32768 byte chunks but actually contains
        /// LZMS-compressed solid resources, for example.
        /// However, if including solid resources, I suggest that you set the WIM's main compression type to LZMS as well,
        /// either by creating the WIM with Wim.CreateNewWim(CompressType.LZMS, ...)
        /// or by calling Wim.SetOutputCompressionType(..., CompressType.LZMS).
        ///
        /// This flag will be set by default when writing or overwriting a WIM file that
        /// either already contains solid resources, or has had solid resources exported
        /// into it and the WIM's main compression type is LZMS.
        /// </summary>
        SOLID = 0x00001000,
        /// <summary>
        /// Send ProgressMsg.DONE_WITH_FILE messages while writing the WIM file.
        /// This is only needed in the unusual case that the library user needs to
        /// know exactly when wimlib has read each file for the last time.
        /// </summary>
        SEND_DONE_WITH_FILE_MESSAGES = 0x00002000,
        /// <summary>
        /// Do not consider content similarity when arranging file data for solid compression. 
        /// Providing this flag will typically worsen the compression ratio,
        /// so only provide this flag if you know what you are doing.
        /// </summary>
        NO_SOLID_SORT = 0x00004000,
        /// <summary>
        /// Since wimlib v1.8.3 and for Wim.Overwrite() only: "unsafely" compact the WIM file in-place, without appending.
        /// Existing resources are shifted down to fill holes and new resources are appended as needed.
        /// The WIM file is truncated to its final size, which may shrink the on-disk file.
        /// 
        /// This operation cannot be safely interrupted.
        /// If the operation is interrupted, then the WIM file will be corrupted,
        /// and it may be impossible (or at least very difficult) to recover any data from it.
        /// Users of this flag are expected to know what they are doing and assume responsibility for any data corruption that may result.
        ///
        /// If the WIM file cannot be compacted in-place because of its structure, its layout, or other requested write parameters,
        /// then Wim.Overwrite() fails with ErrorCode.COMPACTION_NOT_POSSIBLE, and the caller may wish to retry the operation without this flag.
        /// </summary>
        UNSAFE_COMPACT = 0x00008000,
    }
    #endregion

    #region Enum InitFlags
    [Flags]
    public enum InitFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Windows-only:
        /// Do not attempt to acquire additional privileges (currently SeBackupPrivilege, SeRestorePrivilege, 
        /// SeSecurityPrivilege, SeTakeOwnershipPrivilege, and SeManageVolumePrivilege) when initializing the library.
        /// 
        /// This flag is intended for the case where the calling program manages these privileges itself. 
        /// Note: by default, no error is issued if privileges cannot be acquired, although related errors may be reported later,
        /// depending on if the operations performed actually require additional privileges or not.
        /// </summary>
        DONT_ACQUIRE_PRIVILEGES = 0x00000002,
        /// <summary>
        /// Windows only:
        /// If InitFlags.DONT_ACQUIRE_PRIVILEGES not specified, return ErrorCode.INSUFFICIENT_PRIVILEGES if privileges 
        /// that may be needed to read all possible data and metadata for a capture operation could not be acquired. 
        /// Can be combined with InitFlags.STRICT_APPLY_PRIVILEGES.
        /// </summary>
        STRICT_CAPTURE_PRIVILEGES = 0x00000004,
        /// <summary>
        /// Windows only:
        /// If InitFlags.DONT_ACQUIRE_PRIVILEGES not specified, return ErrorCode.INSUFFICIENT_PRIVILEGES if privileges
        /// that may be needed to restore all possible data and metadata for an apply operation could not be acquired. 
        /// Can be combined with InitFlags.STRICT_CAPTURE_PRIVILEGES.
        /// </summary>
        STRICT_APPLY_PRIVILEGES = 0x00000008,
        /// <summary>
        /// Default to interpreting WIM paths case sensitively (default on UNIX-like systems).
        /// </summary>
        DEFAULT_CASE_SENSITIVE = 0x00000010,
        /// <summary>
        /// Default to interpreting WIM paths case insensitively (default on Windows).
        /// This does not apply to mounted images.
        /// </summary>
        DEFAULT_CASE_INSENSITIVE = 0x00000020,
    }
    #endregion

    #region Enum RefFlags
    [Flags]
    public enum RefFlags : int
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// For Wim.ReferenceResourceFiles(), enable shell-style filename globbing.
        /// Ignored by Wim.ReferenceResources().
        /// </summary>
        GLOB_ENABLE = 0x00000001,
        /// <summary>
        /// For Wim.ReferenceResourceFiles(), issue an error (ErrorCode.GLOB_HAD_NO_MATCHES) if a glob did not match any files. 
        /// The default behavior without this flag is to issue no error at that point, but then attempt to open
        /// the glob as a literal path, which of course will fail anyway if no file exists at that path. 
        /// No effect if RefFlags.GLOB_ENABLE is not also specified.
        /// Ignored by Wim.ReferenceResources().
        /// </summary>
        GLOB_ERR_ON_NOMATCH = 0x00000002,
    }
    #endregion
    #endregion

    #region Native WimLib Structs
    #region Struct CaptureSource
    /// <summary>
    /// An array of these structures is passed to Wim.AddImageMultiSource() to specify the sources from which to create a WIM image. 
    /// </summary>
    /// <remarks>
    /// Wrapper struct of Utf{16,8}.CaptureSourceBase.
    /// </remarks>
    public struct CaptureSource
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

    #region Struct WimInfo
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public struct WimInfo
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
        /// The default compression type of resources in this WIM file, as WimLibCompressionType enum.
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
            get => NativeMethods.GetBitField(_bitFlag, 0);
            set => NativeMethods.SetBitField(ref _bitFlag, 0, value);
        }
        /// <summary>
        /// 1 iff this info struct is for a WimStruct that has a backing file.
        /// </summary>
        public bool OpenedFromFile
        {
            get => NativeMethods.GetBitField(_bitFlag, 1);
            set => NativeMethods.SetBitField(ref _bitFlag, 1, value);
        }
        /// <summary>
        /// 1 iff this WIM file is considered readonly for any reason
        /// (e.g. the "readonly" header flag is set, or this is part of a split WIM, or filesystem permissions deny writing)
        /// </summary>
        public bool IsReadonly
        {
            get => NativeMethods.GetBitField(_bitFlag, 2);
            set => NativeMethods.SetBitField(ref _bitFlag, 2, value);
        }
        /// <summary>
        /// 1 iff the "reparse point fix" flag is set in this WIM's header
        /// </summary>
        public bool HasRpfix
        {
            get => NativeMethods.GetBitField(_bitFlag, 3);
            set => NativeMethods.SetBitField(ref _bitFlag, 3, value);
        }
        /// <summary>
        /// 1 iff the "readonly" flag is set in this WIM's header
        /// </summary>
        public bool IsMarkedReadonly
        {
            get => NativeMethods.GetBitField(_bitFlag, 4);
            set => NativeMethods.SetBitField(ref _bitFlag, 4, value);
        }
        /// <summary>
        /// 1 iff the "spanned" flag is set in this WIM's header
        /// </summary>
        public bool Spanned
        {
            get => NativeMethods.GetBitField(_bitFlag, 5);
            set => NativeMethods.SetBitField(ref _bitFlag, 5, value);
        }
        /// <summary>
        /// 1 iff the "write in progress" flag is set in this WIM's header
        /// </summary>
        public bool WriteInProgress
        {
            get => NativeMethods.GetBitField(_bitFlag, 6);
            set => NativeMethods.SetBitField(ref _bitFlag, 6, value);
        }
        /// <summary>
        /// 1 iff the "metadata only" flag is set in this WIM's header
        /// </summary>
        public bool MetadataOnly
        {
            get => NativeMethods.GetBitField(_bitFlag, 7);
            set => NativeMethods.SetBitField(ref _bitFlag, 7, value);
        }
        /// <summary>
        /// 1 iff the "resource only" flag is set in this WIM's header
        /// </summary>
        public bool ResourceOnly
        {
            get => NativeMethods.GetBitField(_bitFlag, 8);
            set => NativeMethods.SetBitField(ref _bitFlag, 8, value);
        }
        /// <summary>
        /// 1 iff this WIM file is pipable (see WriteFlags.PIPABLE).
        /// </summary>
        public bool Pipable
        {
            get => NativeMethods.GetBitField(_bitFlag, 9);
            set => NativeMethods.SetBitField(ref _bitFlag, 9, value);
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        private uint[] _reserved;
    }
    #endregion

    #region Struct UpdateCommand 
    [Flags]
    public enum UpdateOp : uint
    {
        /// <summary>
        /// Add a new file or directory tree to the image.
        /// </summary>
        ADD = 0,
        /// <summary>
        /// Delete a file or directory tree from the image.
        /// </summary>
        DELETE = 1,
        /// <summary>
        /// Rename a file or directory tree in the image.
        /// </summary>
        RENAME = 2,
    }
    #endregion

    #region UpdateCommand
    public class UpdateCommand
    {
        #region Field - UpdateOp
        public UpdateOp Op;
        #endregion

        #region Field - UpdateAdd
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

        #region Field - UpdateDelete
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        public string DelWimPath;
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        public DeleteFlags DeleteFlags;
        #endregion

        #region Field - UpdateRename
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
        public UpdateAdd Add
        {
            get
            {
                if (Op != UpdateOp.ADD)
                    throw new InvalidOperationException("Field [Op] should be [UpdateOp.ADD]");
                return new UpdateAdd(AddFsSourcePath, AddWimTargetPath, AddConfigFile, AddFlags);
            }
            set
            {
                Op = UpdateOp.ADD;
                AddFsSourcePath = value.FsSourcePath;
                AddWimTargetPath = value.WimTargetPath;
                AddConfigFile = value.ConfigFile;
                AddFlags = value.AddFlags;
            }
        }

        public UpdateDelete Delete
        {
            get
            {
                if (Op != UpdateOp.DELETE)
                    throw new InvalidOperationException("Field [Op] should be [UpdateOp.DELETE]");
                return new UpdateDelete(DelWimPath, DeleteFlags);
            }
            set
            {
                Op = UpdateOp.DELETE;
                DelWimPath = value.WimPath;
                DeleteFlags = value.DeleteFlags;
            }
        }

        public UpdateRename Rename
        {
            get
            {
                if (Op != UpdateOp.RENAME)
                    throw new InvalidOperationException("Field [Op] should be [UpdateOp.DELETE]");
                return new UpdateRename(RenWimSourcePath, RenWimTargetPath);
            }
            set
            {
                Op = UpdateOp.RENAME;
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
                Op = UpdateOp.ADD,
                AddFsSourcePath = fsSourcePath,
                AddWimTargetPath = wimTargetPath,
                AddConfigFile = configFile,
                AddFlags = addFlags,
            };
        }

        public static UpdateCommand SetAdd(UpdateAdd add)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.ADD,
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
                Op = UpdateOp.DELETE,
                DelWimPath = wimPath,
                DeleteFlags = deleteFlags,
            };
        }

        public static UpdateCommand SetDelete(UpdateDelete del)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.DELETE,
                DelWimPath = del.WimPath,
                DeleteFlags = del.DeleteFlags,
            };
        }

        public static UpdateCommand SetRename(string wimSourcePath, string wimTargetPath)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.RENAME,
                RenWimSourcePath = wimSourcePath,
                RenWimTargetPath = wimTargetPath,
                _renameFlags = 0,
            };
        }

        public static UpdateCommand SetRename(UpdateRename ren)
        {
            return new UpdateCommand
            {
                Op = UpdateOp.RENAME,
                RenWimSourcePath = ren.WimSourcePath,
                RenWimTargetPath = ren.WimTargetPath,
                _renameFlags = 0,
            };
        }
        #endregion

        #region SubClass - UpdateAdd, UpdateDelete, UpdateRename
        public class UpdateAdd
        {
            /// <summary>
            /// Filesystem path to the file or directory tree to add.
            /// </summary>
            public string FsSourcePath;
            /// <summary>
            /// Destination path in the image.
            /// To specify the root directory of the image, use Wim.RootPath.
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

            public UpdateAdd(string fsSourcePath, string wimTargetPath, string configFile, AddFlags addFlags)
            {
                FsSourcePath = fsSourcePath;
                WimTargetPath = wimTargetPath;
                ConfigFile = configFile;
                AddFlags = addFlags;
            }
        }

        public class UpdateDelete
        {
            /// <summary>
            /// The path to the file or directory within the image to delete.
            /// </summary>
            public string WimPath;
            /// <summary>
            /// Bitwise OR of DeleteFlags.
            /// </summary>
            public DeleteFlags DeleteFlags;

            public UpdateDelete(string wimPath, DeleteFlags deleteFlags)
            {
                WimPath = wimPath;
                DeleteFlags = deleteFlags;
            }
        }

        public class UpdateRename
        {
            /// <summary>
            /// The path to the source file or directory within the image.
            /// </summary>
            public string WimSourcePath;
            /// <summary>
            /// The path to the destination file or directory within the image.
            /// </summary>
            public string WimTargetPath;

            public UpdateRename(string wimSourcePath, string wimTargetPath)
            {
                WimSourcePath = wimSourcePath;
                WimTargetPath = wimTargetPath;
            }
        }
        #endregion

        #region ToNativeStruct
        public UpdateCommand32 ToNativeStruct32()
        {
            switch (Op)
            {
                case UpdateOp.ADD:
                    return new UpdateCommand32
                    {
                        Op = UpdateOp.ADD,
                        AddFsSourcePath = AddFsSourcePath,
                        AddWimTargetPath = AddWimTargetPath,
                        AddConfigFile = AddConfigFile,
                        AddFlags = AddFlags,
                    };
                case UpdateOp.DELETE:
                    return new UpdateCommand32
                    {
                        Op = UpdateOp.DELETE,
                        DelWimPath = DelWimPath,
                        DeleteFlags = DeleteFlags,
                    };
                case UpdateOp.RENAME:
                    return new UpdateCommand32
                    {
                        Op = UpdateOp.RENAME,
                        RenWimSourcePath = RenWimSourcePath,
                        RenWimTargetPath = RenWimTargetPath,
                    };
                default:
                    throw new InvalidOperationException("Internal Logic Error at UpdateCommand.ToNativeStruct32()");
            }
        }

        public UpdateCommand64 ToNativeStruct64()
        {
            switch (Op)
            {
                case UpdateOp.ADD:
                    return new UpdateCommand64
                    {
                        Op = UpdateOp.ADD,
                        AddFsSourcePath = AddFsSourcePath,
                        AddWimTargetPath = AddWimTargetPath,
                        AddConfigFile = AddConfigFile,
                        AddFlags = AddFlags,
                    };
                case UpdateOp.DELETE:
                    return new UpdateCommand64
                    {
                        Op = UpdateOp.DELETE,
                        DelWimPath = DelWimPath,
                        DeleteFlags = DeleteFlags,
                    };
                case UpdateOp.RENAME:
                    return new UpdateCommand64
                    {
                        Op = UpdateOp.RENAME,
                        RenWimSourcePath = RenWimSourcePath,
                        RenWimTargetPath = RenWimTargetPath,
                    };
                default:
                    throw new InvalidOperationException("Internal Logic Error at UpdateCommand.ToNativeStruct64()");
            }
        }
        #endregion
    }
    #endregion

    #region UpdateCommand32
    [StructLayout(LayoutKind.Explicit)]
    public struct UpdateCommand32
    {
        [FieldOffset(0)]
        public UpdateOp Op;

        #region UpdateAddCommand
        /// <summary>
        /// Filesystem path to the file or directory tree to add.
        /// </summary>
        [FieldOffset(4)]
        private IntPtr _addFsSourcePathPtr;
        public string AddFsSourcePath
        {
            get => NativeMethods.MarshalPtrToString(_addFsSourcePathPtr);
            set => UpdatePtr(ref _addFsSourcePathPtr, value);
        }
        /// <summary>
        /// Destination path in the image.  To specify the root directory of the image, use Wim.RootPath.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _addWimTargetPathPtr;
        public string AddWimTargetPath
        {
            get => NativeMethods.MarshalPtrToString(_addWimTargetPathPtr);
            set => UpdatePtr(ref _addWimTargetPathPtr, value);
        }
        /// <summary>
        /// Path to capture configuration file to use, or null if not specified.
        /// </summary>
        [FieldOffset(12)]
        private IntPtr _addConfigFilePtr;
        public string AddConfigFile
        {
            get => NativeMethods.MarshalPtrToString(_addConfigFilePtr);
            set => UpdatePtr(ref _addConfigFilePtr, value);
        }
        /// <summary>
        /// Bitwise OR of AddFlags.
        /// </summary>
        [FieldOffset(16)]
        public AddFlags AddFlags;
        #endregion

        #region UpdateDeleteCommand
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        [FieldOffset(4)]
        private IntPtr _delWimPathPtr;
        public string DelWimPath
        {
            get => NativeMethods.MarshalPtrToString(_delWimPathPtr);
            set => UpdatePtr(ref _delWimPathPtr, value);
        }
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        [FieldOffset(8)]
        public DeleteFlags DeleteFlags;
        #endregion

        #region UpdateRenameCommand
        /// <summary>
        /// The path to the source file or directory within the image.
        /// </summary>
        [FieldOffset(4)]
        private IntPtr _renWimSourcePathPtr;
        public string RenWimSourcePath
        {
            get => NativeMethods.MarshalPtrToString(_renWimSourcePathPtr);
            set => UpdatePtr(ref _renWimSourcePathPtr, value);
        }
        /// <summary>
        /// The path to the destination file or directory within the image.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _renWimTargetPathPtr;
        public string RenWimTargetPath
        {
            get => NativeMethods.MarshalPtrToString(_renWimTargetPathPtr);
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
                case UpdateOp.ADD:
                    FreePtr(ref _addFsSourcePathPtr);
                    FreePtr(ref _addWimTargetPathPtr);
                    FreePtr(ref _addConfigFilePtr);
                    break;
                case UpdateOp.DELETE:
                    FreePtr(ref _delWimPathPtr);
                    break;
                case UpdateOp.RENAME:
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
            ptr = NativeMethods.MarshalStringToPtr(str);
        }
        #endregion

        #region ToManagedClass
        public UpdateCommand ToManagedClass()
        {
            switch (Op)
            {
                case UpdateOp.ADD:
                    return UpdateCommand.SetAdd(AddFsSourcePath, AddWimTargetPath, AddConfigFile, AddFlags);
                case UpdateOp.DELETE:
                    return UpdateCommand.SetDelete(DelWimPath, DeleteFlags);
                case UpdateOp.RENAME:
                    return UpdateCommand.SetRename(RenWimSourcePath, RenWimTargetPath);
                default:
                    throw new InvalidOperationException("Internal Logic Error at UpdateCommand32.Convert()");
            }
        }
        #endregion
    }
    #endregion

    #region UpdateCommand64
    [StructLayout(LayoutKind.Explicit)]
    public struct UpdateCommand64
    {
        [FieldOffset(0)]
        public UpdateOp Op;

        #region UpdateAddCommand
        /// <summary>
        /// Filesystem path to the file or directory tree to add.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _addFsSourcePathPtr;
        public string AddFsSourcePath
        {
            get => NativeMethods.MarshalPtrToString(_addFsSourcePathPtr);
            set => UpdatePtr(ref _addFsSourcePathPtr, value);
        }
        /// <summary>
        /// Destination path in the image.  To specify the root directory of the image, use Wim.RootPath.
        /// </summary>
        [FieldOffset(16)]
        private IntPtr _addWimTargetPathPtr;
        public string AddWimTargetPath
        {
            get => NativeMethods.MarshalPtrToString(_addWimTargetPathPtr);
            set => UpdatePtr(ref _addWimTargetPathPtr, value);
        }
        /// <summary>
        /// Path to capture configuration file to use, or null if not specified.
        /// </summary>
        [FieldOffset(24)]
        private IntPtr _addConfigFilePtr;
        public string AddConfigFile
        {
            get => NativeMethods.MarshalPtrToString(_addConfigFilePtr);
            set => UpdatePtr(ref _addConfigFilePtr, value);
        }
        /// <summary>
        /// Bitwise OR of AddFlags.
        /// </summary>
        [FieldOffset(32)]
        public AddFlags AddFlags;
        #endregion

        #region UpdateDeleteCommand
        /// <summary>
        /// The path to the file or directory within the image to delete.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _delWimPathPtr;
        public string DelWimPath
        {
            get => NativeMethods.MarshalPtrToString(_delWimPathPtr);
            set => UpdatePtr(ref _delWimPathPtr, value);
        }
        /// <summary>
        /// Bitwise OR of DeleteFlags.
        /// </summary>
        [FieldOffset(16)]
        public DeleteFlags DeleteFlags;
        #endregion

        #region UpdateRenameCommand
        /// <summary>
        /// The path to the source file or directory within the image.
        /// </summary>
        [FieldOffset(8)]
        private IntPtr _renWimSourcePathPtr;
        public string RenWimSourcePath
        {
            get => NativeMethods.MarshalPtrToString(_renWimSourcePathPtr);
            set => UpdatePtr(ref _renWimSourcePathPtr, value);
        }
        /// <summary>
        /// The path to the destination file or directory within the image.
        /// </summary>
        [FieldOffset(16)]
        private IntPtr _renWimTargetPathPtr;
        public string RenWimTargetPath
        {
            get => NativeMethods.MarshalPtrToString(_renWimTargetPathPtr);
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
                case UpdateOp.ADD:
                    FreePtr(ref _addFsSourcePathPtr);
                    FreePtr(ref _addWimTargetPathPtr);
                    FreePtr(ref _addConfigFilePtr);
                    break;
                case UpdateOp.DELETE:
                    FreePtr(ref _delWimPathPtr);
                    break;
                case UpdateOp.RENAME:
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
            ptr = NativeMethods.MarshalStringToPtr(str);
        }
        #endregion

        #region ToManagedClass
        public UpdateCommand ToManagedClass()
        {
            switch (Op)
            {
                case UpdateOp.ADD:
                    return UpdateCommand.SetAdd(AddFsSourcePath, AddWimTargetPath, AddConfigFile, AddFlags);
                case UpdateOp.DELETE:
                    return UpdateCommand.SetDelete(DelWimPath, DeleteFlags);
                case UpdateOp.RENAME:
                    return UpdateCommand.SetRename(RenWimSourcePath, RenWimTargetPath);
                default:
                    throw new InvalidOperationException("Internal Logic Error at UpdateCommand64.Convert()");
            }
        }
        #endregion
    }
    #endregion
    #endregion

    #region Struct DirEntry
    /// <summary>
    /// Structure passed to the Wim.IterateDirTree() callback function.
    /// Roughly, the information about a "file" in the WIM image --- but really a directory entry ("dentry") because hard links are allowed.
    /// The HardLinkGroupId field can be used to distinguish actual file inodes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DirEntryBase
    {
        /// <summary>
        /// Name of the file, or null if this file is unnamed. Only the root directory of an image will be unnamed.
        /// </summary>
        public string FileName => NativeMethods.MarshalPtrToString(_fileNamePtr);
        private IntPtr _fileNamePtr;
        /// <summary>
        /// 8.3 name (or "DOS name", or "short name") of this file; or null if this file has no such name.
        /// </summary>
        public string DosName => NativeMethods.MarshalPtrToString(_dosNamePtr);
        private IntPtr _dosNamePtr;
        /// <summary>
        /// Full path to this file within the image.
        /// Path separators will be Wim.PathSeparator.
        /// </summary>
        public string FullPath => NativeMethods.MarshalPtrToString(_fullPathPtr);
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
        /// These are the "standard" Windows FILE_ATTRIBUTE_* values.
        /// </summary>
        public FileAttribute Attributes;
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
    /// Structure passed to the Wim.IterateDirTree() callback function.
    /// Roughly, the information about a "file" in the WIM image --- but really a directory entry ("dentry") because hard links are allowed.
    /// The HardLinkGroupId field can be used to distinguish actual file inodes.
    /// </summary>
    /// <remarks>
    /// Wrapper of DirEntryBase
    /// </remarks>
    public struct DirEntry
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
        /// Path separators will be Wim.PathSeparator.
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
        /// These are the "standard" Windows FILE_ATTRIBUTE_* values.
        /// </summary>
        public FileAttribute Attributes;
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

    [Flags]
    public enum FileAttribute : uint
    {
        READONLY = 0x00000001,
        HIDDEN = 0x00000002,
        SYSTEM = 0x00000004,
        DIRECTORY = 0x00000010,
        ARCHIVE = 0x00000020,
        DEVICE = 0x00000040,
        NORMAL = 0x00000080,
        TEMPORARY = 0x00000100,
        SPARSE_FILE = 0x00000200,
        REPARSE_POINT = 0x00000400,
        COMPRESSED = 0x00000800,
        OFFLINE = 0x00001000,
        NOT_CONTENT_INDEXED = 0x00002000,
        ENCRYPTED = 0x00004000,
        VIRTUAL = 0x00010000,
    }

    public enum ReparseTag : uint
    {
        RESERVED_ZERO = 0x00000000,
        RESERVED_ONE = 0x00000001,
        MOUNT_POINT = 0xA0000003,
        HSM = 0xC0000004,
        HSM2 = 0x80000006,
        DRIVER_EXTENDER = 0x80000005,
        SIS = 0x80000007,
        DFS = 0x8000000A,
        DFSR = 0x80000012,
        FILTER_MANAGER = 0x8000000B,
        WOF = 0x80000017,
        SYMLINK = 0xA000000C,
    }
    #endregion

    #region Struct WimTimeSpec
    [StructLayout(LayoutKind.Sequential)]
    internal struct WimTimeSpec
    {
        /// <summary>
        /// Seconds since start of UNIX epoch (January 1, 1970)
        /// </summary>
        public long UnixEpoch => _unixEpochVal.ToInt64();
        private IntPtr _unixEpochVal; // int64_t in 64bit, int32_t in 32bit
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
            if (IntPtr.Size == 4)
            {
                long high64 = (long)high << 32;
                genesis = genesis.AddSeconds(high64);
            }

            return genesis;
        }
    }
    #endregion

    #region Struct WimObjectId
    /// <summary>
    /// Since wimlib v1.9.1: an object ID, which is an extra piece of metadata that may be associated with a file on NTFS filesystems. 
    /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa363997(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WimObjectId
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

    #region Struct ResourceEntry
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
        public bool IsCompressed => NativeMethods.GetBitField(_bitFlag, 0);
        /// <summary>
        /// 1 iff this blob contains the metadata for an image. 
        /// </summary>
        public bool IsMetadata => NativeMethods.GetBitField(_bitFlag, 1);
        public bool IsFree => NativeMethods.GetBitField(_bitFlag, 2);
        public bool IsSpanned => NativeMethods.GetBitField(_bitFlag, 3);
        /// <summary>
        /// 1 iff a blob with this hash was not found in the blob lookup table of the WimStruct.
        /// This normally implies a missing call to Wim.ReferenceResourceFiles() or Wim.ReferenceResources().
        /// </summary>
        public bool IsMissing => NativeMethods.GetBitField(_bitFlag, 4);
        /// <summary>
        /// 1 iff this blob is located in a solid resource.
        /// </summary>
        public bool Packed => NativeMethods.GetBitField(_bitFlag, 5);
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

    #region Struct StreamEntry
    /// <summary>
    /// Information about a stream of a particular file in the WIM.
    ///
    /// Normally, only WIM images captured from NTFS filesystems will have multiple streams per file.
    /// In practice, this is a rarely used feature of the filesystem.
    ///
    /// TODO: the library now explicitly tracks stream types, which allows it to have multiple unnamed streams
    /// (e.g. both a reparse point stream and unnamed data stream).
    /// However, this isn't yet exposed by Wim.IterateDirTree().
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct StreamEntry
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