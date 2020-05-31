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

using Joveler.DynLoader;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable EnumUnderlyingTypeIsInt
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
#pragma warning disable 169
#pragma warning disable 414
#pragma warning disable 649
#pragma warning disable IDE0044
#pragma warning disable IDE0063 // 간단한 'using' 문 사용

namespace ManagedWimLib
{
    internal class WimLibLoader : DynLoaderBase
    {
        #region Const
        internal const string MsgErrorFileNotSet = "ErrorFile is not set unable to read last error.";
        internal const string MsgPrintErrorsDisabled = "Error is not being logged, unable to read last error.";
        #endregion

        #region Constructor
        public WimLibLoader() : base() { }
        public WimLibLoader(string libPath) : base(libPath) { }
        #endregion

        #region UTF-8 and UTF-16
        internal Utf8d Utf8 = new Utf8d();
        internal Utf16d Utf16 = new Utf16d();
        #endregion

        #region Error Settings
        private string _errorFile = null;
        private ErrorPrintState _errorPrintState = ErrorPrintState.PrintOff;

        internal static readonly object _errorFileLock = new object();
        internal string GetErrorFilePath()
        {
            lock (_errorFileLock)
            {
                return _errorFile;
            }
        }
        internal ErrorPrintState GetErrorPrintState()
        {
            lock (_errorFileLock)
            {
                return _errorPrintState;
            }
        }
        #endregion

        #region (override) DefaultLibFileName
        protected override string DefaultLibFileName
        {
            get
            {
#if !NET451
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "libwim.so.15";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "libwim.15.dylib";
#endif
                throw new PlatformNotSupportedException();
            }
        }
        #endregion

        #region LoadFunctions, ResetFunctions
        protected override void LoadFunctions()
        {
            switch (UnicodeConvention)
            {
                case UnicodeConvention.Utf16:
                    #region Error - SetErrorFile
                    Utf16.SetErrorFile = GetFuncPtr<Utf16d.wimlib_set_error_file_by_name>(nameof(Utf16d.wimlib_set_error_file_by_name));
                    #endregion

                    #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
                    Utf16.AddEmptyImage = GetFuncPtr<Utf16d.wimlib_add_empty_image>(nameof(Utf16d.wimlib_add_empty_image));
                    Utf16.AddImage = GetFuncPtr<Utf16d.wimlib_add_image>(nameof(Utf16d.wimlib_add_image));
                    Utf16.AddImageMultiSourceL32 = GetFuncPtr<Utf16d.wimlib_add_image_multisource_l32>("wimlib_add_image_multisource");
                    Utf16.AddImageMultiSourceL64 = GetFuncPtr<Utf16d.wimlib_add_image_multisource_l64>("wimlib_add_image_multisource");
                    Utf16.AddTree = GetFuncPtr<Utf16d.wimlib_add_tree>(nameof(Utf16d.wimlib_add_tree));
                    #endregion

                    #region Delete - DeletePath
                    Utf16.DeletePath = GetFuncPtr<Utf16d.wimlib_delete_path>(nameof(Utf16d.wimlib_delete_path));
                    #endregion

                    #region Export - ExportImage
                    Utf16.ExportImage = GetFuncPtr<Utf16d.wimlib_export_image>(nameof(Utf16d.wimlib_export_image));
                    #endregion

                    #region Extract - ExtractImage, ExtractPathList, ExtractPaths
                    Utf16.ExtractImage = GetFuncPtr<Utf16d.wimlib_extract_image>(nameof(Utf16d.wimlib_extract_image));
                    Utf16.ExtractPathList = GetFuncPtr<Utf16d.wimlib_extract_pathlist>(nameof(Utf16d.wimlib_extract_pathlist));
                    Utf16.ExtractPaths = GetFuncPtr<Utf16d.wimlib_extract_paths>(nameof(Utf16d.wimlib_extract_paths));
                    #endregion

                    #region GetImageInfo - GetImageProperty
                    Utf16.GetImageProperty = GetFuncPtr<Utf16d.wimlib_get_image_property>(nameof(Utf16d.wimlib_get_image_property));
                    #endregion

                    #region GetWimInfo - IsImageNameInUse, ResolveImage
                    Utf16.IsImageNameInUse = GetFuncPtr<Utf16d.wimlib_image_name_in_use>(nameof(Utf16d.wimlib_image_name_in_use));
                    Utf16.ResolveImage = GetFuncPtr<Utf16d.wimlib_resolve_image>(nameof(Utf16d.wimlib_resolve_image));
                    #endregion

                    #region Iterate - IterateDirTree
                    Utf16.IterateDirTree = GetFuncPtr<Utf16d.wimlib_iterate_dir_tree>(nameof(Utf16d.wimlib_iterate_dir_tree));
                    #endregion

                    #region Join - Join, JoinWithProgress
                    Utf16.Join = GetFuncPtr<Utf16d.wimlib_join>(nameof(Utf16d.wimlib_join));
                    Utf16.JoinWithProgress = GetFuncPtr<Utf16d.wimlib_join_with_progress>(nameof(Utf16d.wimlib_join_with_progress));
                    #endregion

                    #region Open - Open, OpenWithProgress
                    Utf16.OpenWim = GetFuncPtr<Utf16d.wimlib_open_wim>(nameof(Utf16d.wimlib_open_wim));
                    Utf16.OpenWimWithProgress = GetFuncPtr<Utf16d.wimlib_open_wim_with_progress>(nameof(Utf16d.wimlib_open_wim_with_progress));
                    #endregion

                    #region Mount - MountImage (Linux Only)
                    Utf16.MountImage = GetFuncPtr<Utf16d.wimlib_mount_image>(nameof(Utf16d.wimlib_mount_image));
                    #endregion

                    #region Reference - ReferenceTemplateImage
                    Utf16.ReferenceResourceFiles = GetFuncPtr<Utf16d.wimlib_reference_resource_files>(nameof(Utf16d.wimlib_reference_resource_files));
                    #endregion

                    #region Rename - RenamePath
                    Utf16.RenamePath = GetFuncPtr<Utf16d.wimlib_rename_path>(nameof(Utf16d.wimlib_rename_path));
                    #endregion

                    #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
                    // ReSharper disable once CommentTypo
                    // wimlib_set_image_descripton is misspelled from wimlib itself.
                    Utf16.SetImageDescription = GetFuncPtr<Utf16d.wimlib_set_image_description>("wimlib_set_image_descripton");
                    Utf16.SetImageFlags = GetFuncPtr<Utf16d.wimlib_set_image_flags>(nameof(Utf16d.wimlib_set_image_flags));
                    Utf16.SetImageName = GetFuncPtr<Utf16d.wimlib_set_image_name>(nameof(Utf16d.wimlib_set_image_name));
                    Utf16.SetImageProperty = GetFuncPtr<Utf16d.wimlib_set_image_property>(nameof(Utf16d.wimlib_set_image_property));
                    #endregion

                    #region Split - Split
                    Utf16.Split = GetFuncPtr<Utf16d.wimlib_split>(nameof(Utf16d.wimlib_split));
                    #endregion

                    #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
                    Utf16.UnmountImage = GetFuncPtr<Utf16d.wimlib_unmount_image>(nameof(Utf16d.wimlib_unmount_image));
                    Utf16.UnmountImageWithProgress = GetFuncPtr<Utf16d.wimlib_unmount_image_with_progress>(nameof(Utf16d.wimlib_unmount_image_with_progress));
                    #endregion

                    #region Write - Write
                    Utf16.Write = GetFuncPtr<Utf16d.wimlib_write>(nameof(Utf16d.wimlib_write));
                    #endregion
                    break;
                case UnicodeConvention.Utf8:
                    #region Error - SetErrorFile
                    Utf8.SetErrorFile = GetFuncPtr<Utf8d.wimlib_set_error_file_by_name>(nameof(Utf8d.wimlib_set_error_file_by_name));
                    #endregion

                    #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
                    Utf8.AddEmptyImage = GetFuncPtr<Utf8d.wimlib_add_empty_image>(nameof(Utf8d.wimlib_add_empty_image));
                    Utf8.AddImage = GetFuncPtr<Utf8d.wimlib_add_image>(nameof(Utf8d.wimlib_add_image));
                    Utf8.AddImageMultiSourceL32 = GetFuncPtr<Utf8d.wimlib_add_image_multisource_l32>("wimlib_add_image_multisource");
                    Utf8.AddImageMultiSourceL64 = GetFuncPtr<Utf8d.wimlib_add_image_multisource_l64>("wimlib_add_image_multisource");
                    Utf8.AddTree = GetFuncPtr<Utf8d.wimlib_add_tree>(nameof(Utf8d.wimlib_add_tree));
                    #endregion

                    #region Delete - DeletePath
                    Utf8.DeletePath = GetFuncPtr<Utf8d.wimlib_delete_path>(nameof(Utf8d.wimlib_delete_path));
                    #endregion

                    #region Export - ExportImage
                    Utf8.ExportImage = GetFuncPtr<Utf8d.wimlib_export_image>(nameof(Utf8d.wimlib_export_image));
                    #endregion

                    #region Extract - ExtractImage, ExtractPathList, ExtractPaths
                    Utf8.ExtractImage = GetFuncPtr<Utf8d.wimlib_extract_image>(nameof(Utf8d.wimlib_extract_image));
                    Utf8.ExtractPathList = GetFuncPtr<Utf8d.wimlib_extract_pathlist>(nameof(Utf8d.wimlib_extract_pathlist));
                    Utf8.ExtractPaths = GetFuncPtr<Utf8d.wimlib_extract_paths>(nameof(Utf8d.wimlib_extract_paths));
                    #endregion

                    #region GetImageInfo - GetImageProperty
                    Utf8.GetImageProperty = GetFuncPtr<Utf8d.wimlib_get_image_property>(nameof(Utf8d.wimlib_get_image_property));
                    #endregion

                    #region GetWimInfo - IsImageNameInUse, ResolveImage
                    Utf8.IsImageNameInUse = GetFuncPtr<Utf8d.wimlib_image_name_in_use>(nameof(Utf8d.wimlib_image_name_in_use));
                    Utf8.ResolveImage = GetFuncPtr<Utf8d.wimlib_resolve_image>(nameof(Utf8d.wimlib_resolve_image));
                    #endregion

                    #region Iterate - IterateDirTree
                    Utf8.IterateDirTree = GetFuncPtr<Utf8d.wimlib_iterate_dir_tree>(nameof(Utf8d.wimlib_iterate_dir_tree));
                    #endregion

                    #region Join - Join, JoinWithProgress
                    Utf8.Join = GetFuncPtr<Utf8d.wimlib_join>(nameof(Utf8d.wimlib_join));
                    Utf8.JoinWithProgress = GetFuncPtr<Utf8d.wimlib_join_with_progress>(nameof(Utf8d.wimlib_join_with_progress));
                    #endregion

                    #region Open - Open, OpenWithProgress
                    Utf8.OpenWim = GetFuncPtr<Utf8d.wimlib_open_wim>(nameof(Utf8d.wimlib_open_wim));
                    Utf8.OpenWimWithProgress = GetFuncPtr<Utf8d.wimlib_open_wim_with_progress>(nameof(Utf8d.wimlib_open_wim_with_progress));
                    #endregion

                    #region Mount - MountImage (Linux Only)
                    Utf8.MountImage = GetFuncPtr<Utf8d.wimlib_mount_image>(nameof(Utf8d.wimlib_mount_image));
                    #endregion

                    #region Reference - ReferenceTemplateImage
                    Utf8.ReferenceResourceFiles = GetFuncPtr<Utf8d.wimlib_reference_resource_files>(nameof(Utf8d.wimlib_reference_resource_files));
                    #endregion

                    #region Rename - RenamePath
                    Utf8.RenamePath = GetFuncPtr<Utf8d.wimlib_rename_path>(nameof(Utf8d.wimlib_rename_path));
                    #endregion

                    #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
                    // ReSharper disable once CommentTypo
                    // wimlib_set_image_descripton is misspelled from wimlib itself.
                    Utf8.SetImageDescription = GetFuncPtr<Utf8d.wimlib_set_image_description>("wimlib_set_image_descripton");
                    Utf8.SetImageFlags = GetFuncPtr<Utf8d.wimlib_set_image_flags>(nameof(Utf8d.wimlib_set_image_flags));
                    Utf8.SetImageName = GetFuncPtr<Utf8d.wimlib_set_image_name>(nameof(Utf8d.wimlib_set_image_name));
                    Utf8.SetImageProperty = GetFuncPtr<Utf8d.wimlib_set_image_property>(nameof(Utf8d.wimlib_set_image_property));
                    #endregion

                    #region Split - Split
                    Utf8.Split = GetFuncPtr<Utf8d.wimlib_split>(nameof(Utf8d.wimlib_split));
                    #endregion

                    #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
                    Utf8.UnmountImage = GetFuncPtr<Utf8d.wimlib_unmount_image>(nameof(Utf8d.wimlib_unmount_image));
                    Utf8.UnmountImageWithProgress = GetFuncPtr<Utf8d.wimlib_unmount_image_with_progress>(nameof(Utf8d.wimlib_unmount_image_with_progress));
                    #endregion

                    #region Write - Write
                    Utf8.Write = GetFuncPtr<Utf8d.wimlib_write>(nameof(Utf8d.wimlib_write));
                    #endregion
                    break;
            }

            #region Global - GlobalInit, GlobalCleanup
            GlobalInit = GetFuncPtr<wimlib_global_init>(nameof(wimlib_global_init));
            GlobalCleanup = GetFuncPtr<wimlib_global_cleanup>(nameof(wimlib_global_cleanup));
            #endregion

            #region Error - GetErrorString, SetPrintErrors
            GetErrorString = GetFuncPtr<wimlib_get_error_string>(nameof(wimlib_get_error_string));
            SetPrintErrorsPtr = GetFuncPtr<wimlib_set_print_errors>(nameof(wimlib_set_print_errors));
            #endregion

            #region Create - CreateWim
            CreateNewWim = GetFuncPtr<wimlib_create_new_wim>(nameof(wimlib_create_new_wim));
            #endregion

            #region Delete - DeleteImage
            DeleteImage = GetFuncPtr<wimlib_delete_image>(nameof(wimlib_delete_image));
            #endregion

            #region Free - Free
            Free = GetFuncPtr<wimlib_free>(nameof(wimlib_free));
            #endregion

            #region GetImageInfo - GetImageDescription, GetImageName
            GetImageDescription = GetFuncPtr<wimlib_get_image_description>(nameof(wimlib_get_image_description));
            GetImageName = GetFuncPtr<wimlib_get_image_name>(nameof(wimlib_get_image_name));
            #endregion

            #region GetWimInfo - GetWimInfo, GetXmlData
            GetWimInfo = GetFuncPtr<wimlib_get_wim_info>(nameof(wimlib_get_wim_info));
            GetXmlData = GetFuncPtr<wimlib_get_xml_data>(nameof(wimlib_get_xml_data));
            #endregion

            #region GetVersion - GetVersion, GetVersionString
            GetVersion = GetFuncPtr<wimlib_get_version>(nameof(wimlib_get_version));
            GetVersionString = GetFuncPtr<wimlib_get_version_string>(nameof(wimlib_get_version_string));
            #endregion

            #region Iterate - IterateLookupTable
            IterateLookupTable = GetFuncPtr<wimlib_iterate_lookup_table>(nameof(wimlib_iterate_lookup_table));
            #endregion

            #region Reference - ReferenceTemplateImage, ReferenceResourceFiles, ReferenceResources
            ReferenceResources = GetFuncPtr<wimlib_reference_resources>(nameof(wimlib_reference_resources));
            ReferenceTemplateImage = GetFuncPtr<wimlib_reference_template_image>(nameof(wimlib_reference_template_image));
            #endregion

            #region Callback - RegsiterProgressFunction
            RegisterProgressFunction = GetFuncPtr<wimlib_register_progress_function>(nameof(wimlib_register_progress_function));
            #endregion

            #region SetImageInfo - SetWimInfo
            SetWimInfo = GetFuncPtr<wimlib_set_wim_info>(nameof(wimlib_set_wim_info));
            #endregion

            #region SetOutput - SetOutputChunkSize, SetOutputPackChunkSize, SetOutputCompressionType, SetOutputPackCompressionType
            SetOutputChunkSize = GetFuncPtr<wimlib_set_output_chunk_size>(nameof(wimlib_set_output_chunk_size));
            SetOutputPackChunkSize = GetFuncPtr<wimlib_set_output_pack_chunk_size>(nameof(wimlib_set_output_pack_chunk_size));
            SetOutputCompressionType = GetFuncPtr<wimlib_set_output_compression_type>(nameof(wimlib_set_output_compression_type));
            SetOutputPackCompressionType = GetFuncPtr<wimlib_set_output_pack_compression_type>(nameof(wimlib_set_output_pack_compression_type));
            #endregion

            #region Verify - VerifyWim
            VerifyWim = GetFuncPtr<wimlib_verify_wim>(nameof(wimlib_verify_wim));
            #endregion

            #region Update - UpdateImage
            UpdateImage32 = GetFuncPtr<wimlib_update_image_32>("wimlib_update_image");
            UpdateImage64 = GetFuncPtr<wimlib_update_image_64>("wimlib_update_image");
            #endregion

            #region Write - Overwrite
            Overwrite = GetFuncPtr<wimlib_overwrite>(nameof(wimlib_overwrite));
            #endregion

            #region CompressInfo - SetDefaultCompressionLevel, GetCompressorNeededMemory
            SetDefaultCompressionLevel = GetFuncPtr<wimlib_set_default_compression_level>(nameof(wimlib_set_default_compression_level));
            GetCompressorNeededMemory = GetFuncPtr<wimlib_get_compressor_needed_memory>(nameof(wimlib_get_compressor_needed_memory));
            #endregion

            #region Compressor - CreateCompressor, FreeCompressor, Compress
            CreateCompressor = GetFuncPtr<wimlib_create_compressor>(nameof(wimlib_create_compressor));
            FreeCompressor = GetFuncPtr<wimlib_free_compressor>(nameof(wimlib_free_compressor));
            Compress = GetFuncPtr<wimlib_compress>(nameof(wimlib_compress));
            #endregion

            #region Decompressor - CreateDecompressor, FreeDecompressor, Decompress
            CreateDecompressor = GetFuncPtr<wimlib_create_decompressor>(nameof(wimlib_create_decompressor));
            FreeDecompressor = GetFuncPtr<wimlib_free_decompressor>(nameof(wimlib_free_decompressor));
            Decompress = GetFuncPtr<wimlib_decompress>(nameof(wimlib_decompress));
            #endregion

            #region (Code) Set ErrorFile and PrintError
            SetErrorFile();
            #endregion
        }

        protected override void ResetFunctions()
        {
            #region (Code) Cleanup ErrorFile
            SetPrintErrors(false);
            #endregion

            #region Global - GlobalInit, GlobalCleanup
            GlobalInit = null;
            GlobalCleanup = null;
            #endregion

            #region Error - GetErrorString, SetErrorFile, SetPrintErrors
            GetErrorString = null;
            Utf16.SetErrorFile = null;
            Utf8.SetErrorFile = null;
            SetPrintErrorsPtr = null;
            #endregion

            #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
            Utf16.AddEmptyImage = null;
            Utf16.AddImage = null;
            Utf16.AddImageMultiSourceL32 = null;
            Utf16.AddImageMultiSourceL64 = null;
            Utf16.AddTree = null;
            Utf8.AddEmptyImage = null;
            Utf8.AddImage = null;
            Utf8.AddImageMultiSourceL32 = null;
            Utf8.AddImageMultiSourceL64 = null;
            Utf8.AddTree = null;
            #endregion

            #region Create - CreateWim
            CreateNewWim = null;
            #endregion

            #region Delete - DeleteImage, DeletePath
            DeleteImage = null;
            Utf16.DeletePath = null;
            Utf8.DeletePath = null;
            #endregion

            #region Export - ExportImage
            Utf16.ExportImage = null;
            Utf8.ExportImage = null;
            #endregion

            #region Extract - ExtractImage, ExtractPathList, ExtractPaths
            Utf16.ExtractImage = null;
            Utf16.ExtractPathList = null;
            Utf16.ExtractPaths = null;
            Utf8.ExtractImage = null;
            Utf8.ExtractPathList = null;
            Utf8.ExtractPaths = null;
            #endregion

            #region Free - Free
            Free = null;
            #endregion

            #region GetImageInfo - GetImageDescription, GetImageName, GetImageProperty
            GetImageDescription = null;
            GetImageName = null;
            Utf16.GetImageProperty = null;
            Utf8.GetImageProperty = null;
            #endregion

            #region GetWimInfo - GetWimInfo, GetXmlData, IsImageNameInUse, ResolveImage
            GetWimInfo = null;
            GetXmlData = null;
            Utf16.IsImageNameInUse = null;
            Utf16.ResolveImage = null;
            Utf8.IsImageNameInUse = null;
            Utf8.ResolveImage = null;
            #endregion

            #region GetVersion - GetVersion, GetVersionString
            GetVersion = null;
            GetVersionString = null;
            #endregion

            #region Iterate - IterateDirTree, IterateLookupTable
            Utf16.IterateDirTree = null;
            Utf8.IterateDirTree = null;
            IterateLookupTable = null;
            #endregion

            #region Join - Join, JoinWithProgress
            Utf16.Join = null;
            Utf16.JoinWithProgress = null;
            Utf8.Join = null;
            Utf8.JoinWithProgress = null;
            #endregion

            #region Open - Open, OpenWithProgress
            Utf16.OpenWim = null;
            Utf16.OpenWimWithProgress = null;
            Utf8.OpenWim = null;
            Utf8.OpenWimWithProgress = null;
            #endregion

            #region Mount - MountImage (Linux Only)
            Utf16.MountImage = null;
            Utf8.MountImage = null;
            #endregion

            #region Reference - ReferenceTemplateImage, ReferenceResourceFiles, ReferenceResources
            Utf16.ReferenceResourceFiles = null;
            Utf8.ReferenceResourceFiles = null;
            ReferenceResources = null;
            ReferenceTemplateImage = null;
            #endregion

            #region Callback - RegsiterProgressFunction
            RegisterProgressFunction = null;
            #endregion

            #region Rename - RenamePath
            Utf16.RenamePath = null;
            Utf8.RenamePath = null;
            #endregion

            #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty, SetWimInfo
            Utf16.SetImageDescription = null;
            Utf16.SetImageFlags = null;
            Utf16.SetImageName = null;
            Utf16.SetImageProperty = null;
            Utf8.SetImageDescription = null;
            Utf8.SetImageFlags = null;
            Utf8.SetImageName = null;
            Utf8.SetImageProperty = null;
            SetWimInfo = null;
            #endregion

            #region SetOutput - SetOutputChunkSize, SetOutputPackChunkSize, SetOutputCompressionType, SetOutputPackCompressionType
            SetOutputChunkSize = null;
            SetOutputPackChunkSize = null;
            SetOutputCompressionType = null;
            SetOutputPackCompressionType = null;
            #endregion

            #region Split - Split
            Utf16.Split = null;
            Utf8.Split = null;
            #endregion

            #region Verify - VerifyWim
            VerifyWim = null;
            #endregion

            #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
            Utf16.UnmountImage = null;
            Utf16.UnmountImageWithProgress = null;
            Utf8.UnmountImage = null;
            Utf8.UnmountImageWithProgress = null;
            #endregion

            #region Update - UpdateImage
            UpdateImage32 = null;
            UpdateImage64 = null;
            #endregion

            #region Write - Write, Overwrite
            Utf16.Write = null;
            Utf8.Write = null;
            Overwrite = null;
            #endregion

            #region CompressInfo - SetDefaultCompressionLevel, GetCompressorNeededMemory
            SetDefaultCompressionLevel = null;
            GetCompressorNeededMemory = null;
            #endregion

            #region Compressor - CreateCompressor, FreeCompressor, Compress
            CreateCompressor = null;
            FreeCompressor = null;
            Compress = null;
            #endregion

            #region Decompressor - CreateDecompressor, FreeDecompressor, Decompress
            CreateDecompressor = null;
            FreeDecompressor = null;
            Decompress = null;
            #endregion
        }
        #endregion

        #region WimLib Function Pointers
        #region UTF-16 Instances
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        internal class Utf16d
        {
            internal const UnmanagedType StrType = UnmanagedType.LPWStr;
            internal const CharSet StructCharSet = CharSet.Unicode;

            #region Error - SetErrorFile
            /// <summary>
            /// Set the path to the file to which the library will print error and warning messages.
            /// The library will open this file for appending.
            /// 
            /// This also enables error messages, as if by a call to wimlib_set_print_errors(true).
            /// </summary>
            /// <remarks>
            /// WIMLIB_ERR_OPEN: The file named by @p path could not be opened for appending.
            /// WIMLIB_ERR_UNSUPPORTED: wimlib was compiled using the <c>--without-error-messages</c> option.
            /// </remarks>
            /// <returns>0 on success; a ::wimlib_error_code value on failure.</returns>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_error_file_by_name(
                [MarshalAs(StrType)] string path);
            internal wimlib_set_error_file_by_name SetErrorFile;
            #endregion

            #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_empty_image(
                IntPtr wim,
                [MarshalAs(StrType)] string name,
                out int new_idx_ret);
            internal wimlib_add_empty_image AddEmptyImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image(
                IntPtr wim,
                [MarshalAs(StrType)] string source,
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal wimlib_add_image AddImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l32(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL32[] sources,
                UIntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal wimlib_add_image_multisource_l32 AddImageMultiSourceL32;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l64(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL64[] sources,
                UIntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal wimlib_add_image_multisource_l64 AddImageMultiSourceL64;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string fs_source_path,
                [MarshalAs(StrType)] string wim_target_path,
                AddFlags add_flags);
            internal wimlib_add_tree AddTree;
            #endregion

            #region Delete - DeletePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_delete_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                DeleteFlags delete_flags);
            internal wimlib_delete_path DeletePath;
            #endregion

            #region Export - ExportImage
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_export_image(
                IntPtr src_wim,
                int src_image,
                IntPtr dest_wim,
                [MarshalAs(StrType)] string dest_name,
                [MarshalAs(StrType)] string dest_description,
                ExportFlags export_flags);
            internal wimlib_export_image ExportImage;
            #endregion

            #region Extract - ExtractImage, ExtractPaths, ExtractPathList
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                ExtractFlags extract_flags);
            internal wimlib_extract_image ExtractImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_pathlist(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(StrType)] string path_list_file,
                ExtractFlags extract_flags);
            internal wimlib_extract_pathlist ExtractPathList;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_paths(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] paths,
                UIntPtr num_paths, // size_t
                ExtractFlags extract_flags);
            internal wimlib_extract_paths ExtractPaths;
            #endregion

            #region GetImageInfo - GetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr wimlib_get_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name);
            internal wimlib_get_image_property GetImageProperty;
            #endregion

            #region GetWimInfo - IsImageNameInUse, ResolveImage
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal delegate bool wimlib_image_name_in_use(
                IntPtr wim,
                [MarshalAs(StrType)] string name);
            internal wimlib_image_name_in_use IsImageNameInUse;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int wimlib_resolve_image(
                IntPtr wim,
                [MarshalAs(StrType)] string image_name_or_num);
            internal wimlib_resolve_image ResolveImage;
            #endregion

            #region Iterate - IterateDirTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            internal delegate int wimlib_iterate_dir_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                IterateDirTreeFlags flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeIterateDirTreeCallback cb,
                IntPtr user_ctx);
            internal wimlib_iterate_dir_tree IterateDirTree;
            #endregion

            #region Join - Join, JoinWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags);
            internal wimlib_join Join;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join_with_progress(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal wimlib_join_with_progress JoinWithProgress;
            #endregion

            #region Open - OpenWim, OpenWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret);
            internal wimlib_open_wim OpenWim;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim_with_progress(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal wimlib_open_wim_with_progress OpenWimWithProgress;
            #endregion

            #region Mount - MountImage (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_mount_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string dir,
                MountFlags mount_flags,
                [MarshalAs(StrType)] string staging_dir);
            internal wimlib_mount_image MountImage;
            #endregion

            #region Reference - ReferenceResourceFiles
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_reference_resource_files(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] resource_wimfiles_or_globs,
                uint count,
                RefFlags ref_flags,
                OpenFlags open_flags);
            internal wimlib_reference_resource_files ReferenceResourceFiles;
            #endregion

            #region Rename - RenamePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_rename_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string source_path,
                [MarshalAs(StrType)] string dest_path);
            internal wimlib_rename_path RenamePath;
            #endregion

            #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_description(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string description);
            internal wimlib_set_image_description SetImageDescription;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_flags(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string flags);
            internal wimlib_set_image_flags SetImageFlags;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_name(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string name);
            internal wimlib_set_image_name SetImageName;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name,
                [MarshalAs(StrType)] string property_value);
            internal wimlib_set_image_property SetImageProperty;
            #endregion

            #region Split - Split
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_split(
                IntPtr wim,
                [MarshalAs(StrType)] string swm_name,
                ulong part_size,
                WriteFlags write_flags);
            internal wimlib_split Split;
            #endregion

            #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags);
            internal wimlib_unmount_image UnmountImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image_with_progress(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal wimlib_unmount_image_with_progress UnmountImageWithProgress;
            #endregion

            #region Write - Write
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_write(
                IntPtr wim,
                [MarshalAs(StrType)] string path,
                int image,
                WriteFlags write_flags,
                uint num_threads);
            internal wimlib_write Write;
            #endregion

            #region struct CaptureSourceBase
            /// <summary>
            /// An array of these structures is passed to <see cref="Wim.AddImage(string, string, string, AddFlags)"/>
            /// to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LLP64 platforms (Windows)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            internal struct CaptureSourceBaseL32
            {
                /// <summary>
                /// Absolute or relative path to a file or directory on the external filesystem to be included in the image.
                /// </summary>
                public string FsSourcePath;
                /// <summary>
                /// Destination path in the image.
                /// To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>. 
                /// </summary>
                public string WimTargetPath;
                /// <summary>
                /// Reserved; set to 0.
                /// </summary>
                private int _reserved;

                public CaptureSourceBaseL32(string fsSourcePath, string wimTargetPath)
                {
                    FsSourcePath = fsSourcePath;
                    WimTargetPath = wimTargetPath;
                    _reserved = 0;
                }
            };

            /// <summary>
            /// An array of these structures is passed to <see cref="Wim.AddImage(string, string, string, AddFlags)"/>
            /// to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LP64 platforms (64bit POSIX)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            internal struct CaptureSourceBaseL64
            {
                /// <summary>
                /// Absolute or relative path to a file or directory on the external filesystem to be included in the image.
                /// </summary>
                public string FsSourcePath;
                /// <summary>
                /// Destination path in the image.
                /// To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>. 
                /// </summary>
                public string WimTargetPath;
                /// <summary>
                /// Reserved; set to 0.
                /// </summary>
                private long _reserved;

                public CaptureSourceBaseL64(string fsSourcePath, string wimTargetPath)
                {
                    FsSourcePath = fsSourcePath;
                    WimTargetPath = wimTargetPath;
                    _reserved = 0;
                }
            };
            #endregion
        }
        #endregion

        #region UTF-8 Instances
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        internal class Utf8d
        {
            internal const UnmanagedType StrType = UnmanagedType.LPStr;
            internal const CharSet StructCharSet = CharSet.Ansi;

            #region Error - SetErrorFile
            /// <summary>
            /// Set the path to the file to which the library will print error and warning messages.
            /// The library will open this file for appending.
            /// 
            /// This also enables error messages, as if by a call to wimlib_set_print_errors(true).
            /// </summary>
            /// <remarks>
            /// WIMLIB_ERR_OPEN: The file named by @p path could not be opened for appending.
            /// WIMLIB_ERR_UNSUPPORTED: wimlib was compiled using the <c>--without-error-messages</c> option.
            /// </remarks>
            /// <returns>0 on success; a ::wimlib_error_code value on failure.</returns>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_error_file_by_name(
                [MarshalAs(StrType)] string path);
            internal wimlib_set_error_file_by_name SetErrorFile;
            #endregion

            #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_empty_image(
                IntPtr wim,
                [MarshalAs(StrType)] string name,
                out int new_idx_ret);
            internal wimlib_add_empty_image AddEmptyImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image(
                IntPtr wim,
                [MarshalAs(StrType)] string source,
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal wimlib_add_image AddImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l32(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL32[] sources,
                UIntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal wimlib_add_image_multisource_l32 AddImageMultiSourceL32;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l64(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL64[] sources,
                UIntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal wimlib_add_image_multisource_l64 AddImageMultiSourceL64;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string fs_source_path,
                [MarshalAs(StrType)] string wim_target_path,
                AddFlags add_flags);
            internal wimlib_add_tree AddTree;
            #endregion

            #region Delete - DeletePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_delete_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                DeleteFlags delete_flags);
            internal wimlib_delete_path DeletePath;
            #endregion

            #region Export - ExportImage
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_export_image(
                IntPtr src_wim,
                int src_image,
                IntPtr dest_wim,
                [MarshalAs(StrType)] string dest_name,
                [MarshalAs(StrType)] string dest_description,
                ExportFlags export_flags);
            internal wimlib_export_image ExportImage;
            #endregion

            #region Extract - ExtractImage, ExtractPaths, ExtractPathList
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                ExtractFlags extract_flags);
            internal wimlib_extract_image ExtractImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_pathlist(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(StrType)] string path_list_file,
                ExtractFlags extract_flags);
            internal wimlib_extract_pathlist ExtractPathList;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_paths(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] paths,
                UIntPtr num_paths, // size_t
                ExtractFlags extract_flags);
            internal wimlib_extract_paths ExtractPaths;
            #endregion

            #region GetImageInfo - GetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr wimlib_get_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name);
            internal wimlib_get_image_property GetImageProperty;
            #endregion

            #region GetWimInfo - IsImageNameInUse, ResolveImage
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal delegate bool wimlib_image_name_in_use(
                IntPtr wim,
                [MarshalAs(StrType)] string name);
            internal wimlib_image_name_in_use IsImageNameInUse;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int wimlib_resolve_image(
                IntPtr wim,
                [MarshalAs(StrType)] string image_name_or_num);
            internal wimlib_resolve_image ResolveImage;
            #endregion

            #region Iterate - IterateDirTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            internal delegate int wimlib_iterate_dir_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                IterateDirTreeFlags flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeIterateDirTreeCallback cb,
                IntPtr user_ctx);
            internal wimlib_iterate_dir_tree IterateDirTree;
            #endregion

            #region Join - Join, JoinWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags);
            internal wimlib_join Join;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join_with_progress(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal wimlib_join_with_progress JoinWithProgress;
            #endregion

            #region Open - OpenWim, OpenWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret);
            internal wimlib_open_wim OpenWim;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim_with_progress(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal wimlib_open_wim_with_progress OpenWimWithProgress;
            #endregion

            #region Mount - MountImage (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_mount_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string dir,
                MountFlags mount_flags,
                [MarshalAs(StrType)] string staging_dir);
            internal wimlib_mount_image MountImage;
            #endregion

            #region Reference - ReferenceResourceFiles
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_reference_resource_files(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] resource_wimfiles_or_globs,
                uint count,
                RefFlags ref_flags,
                OpenFlags open_flags);
            internal wimlib_reference_resource_files ReferenceResourceFiles;
            #endregion

            #region Rename - RenamePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_rename_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string source_path,
                [MarshalAs(StrType)] string dest_path);
            internal wimlib_rename_path RenamePath;
            #endregion

            #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_description(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string description);
            internal wimlib_set_image_description SetImageDescription;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_flags(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string flags);
            internal wimlib_set_image_flags SetImageFlags;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_name(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string name);
            internal wimlib_set_image_name SetImageName;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name,
                [MarshalAs(StrType)] string property_value);
            internal wimlib_set_image_property SetImageProperty;
            #endregion

            #region Split - Split
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_split(
                IntPtr wim,
                [MarshalAs(StrType)] string swm_name,
                ulong part_size,
                WriteFlags write_flags);
            internal wimlib_split Split;
            #endregion

            #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags);
            internal wimlib_unmount_image UnmountImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image_with_progress(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal wimlib_unmount_image_with_progress UnmountImageWithProgress;
            #endregion

            #region Write - Write
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_write(
                IntPtr wim,
                [MarshalAs(StrType)] string path,
                int image,
                WriteFlags write_flags,
                uint num_threads);
            internal wimlib_write Write;
            #endregion

            #region struct CaptureSourceBase
            /// <summary>
            /// An array of these structures is passed to <see cref="Wim.AddImage(string, string, string, AddFlags)"/>
            /// to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LLP64 platforms (Windows)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            internal struct CaptureSourceBaseL32
            {
                /// <summary>
                /// Absolute or relative path to a file or directory on the external filesystem to be included in the image.
                /// </summary>
                public string FsSourcePath;
                /// <summary>
                /// Destination path in the image.
                /// To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>. 
                /// </summary>
                public string WimTargetPath;
                /// <summary>
                /// Reserved; set to 0.
                /// </summary>
                private int _reserved;

                public CaptureSourceBaseL32(string fsSourcePath, string wimTargetPath)
                {
                    FsSourcePath = fsSourcePath;
                    WimTargetPath = wimTargetPath;
                    _reserved = 0;
                }
            };

            /// <summary>
            /// An array of these structures is passed to <see cref="Wim.AddImage(string, string, string, AddFlags)"/>
            /// to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LP64 platforms (64bit POSIX)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            internal struct CaptureSourceBaseL64
            {
                /// <summary>
                /// Absolute or relative path to a file or directory on the external filesystem to be included in the image.
                /// </summary>
                public string FsSourcePath;
                /// <summary>
                /// Destination path in the image.
                /// To specify the root directory of the image, use <see cref="Path.DirectorySeparatorChar"/>. 
                /// </summary>
                public string WimTargetPath;
                /// <summary>
                /// Reserved; set to 0.
                /// </summary>
                private long _reserved;

                public CaptureSourceBaseL64(string fsSourcePath, string wimTargetPath)
                {
                    FsSourcePath = fsSourcePath;
                    WimTargetPath = wimTargetPath;
                    _reserved = 0;
                }
            };
            #endregion
        }
        #endregion

        #region GlobalInit, GlobalCleanup
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_global_init(InitFlags initFlags);
        /// <summary>
        /// Initialization function for wimlib.
        /// Call before using any other wimlib function (except possibly Wim.SetPrintErrors()).
        /// If not done manually, this function will be called automatically with a flags argument of 0.
        /// This function does nothing if called again after it has already successfully run.
        /// </summary>
        internal wimlib_global_init GlobalInit;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_global_cleanup();
        /// <summary>
        /// Cleanup function for wimlib.
        /// You are not required to call this function, but it will release any global resources allocated by the library.
        /// </summary>
        internal wimlib_global_cleanup GlobalCleanup;
        #endregion

        #region Create - Callback
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate CallbackStatus NativeProgressFunc(
            ProgressMsg msgType,
            IntPtr info,
            IntPtr progctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_register_progress_function(
            IntPtr wim,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
            IntPtr progctx);
        internal wimlib_register_progress_function RegisterProgressFunction;
        #endregion

        #region Error - GetErrorString, SetErrorFile, SetPrintErrors and Helpers
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_error_string(ErrorCode code);
        internal wimlib_get_error_string GetErrorString;

        internal void SetErrorFile()
        {
            string errorFile = Path.GetTempFileName();
            SetErrorFile(errorFile);
        }

        internal void SetErrorFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var ret = UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.SetErrorFile(path),
                _ => Utf8.SetErrorFile(path),
            };

            // When ret is ErrorCode.NotSupported, wimlib was compiled using the --without-error-messages option.
            // In that case, ManagedWimLib must not throw WimException.
            if (ret == ErrorCode.Unsupported)
            {
                _errorPrintState = ErrorPrintState.NotSupported;

                // ErrorFile is no longer used, delete it
                if (_errorFile != null)
                {
                    if (File.Exists(_errorFile))
                        File.Delete(_errorFile);
                    _errorFile = null;
                }
            }
            else
            {
                WimException.CheckErrorCode(ret);

                // Set new ErrorFile and report state as ErrorPrintState.PrintOn.
                _errorPrintState = ErrorPrintState.PrintOn;
                _errorFile = path;
            }
        }

        /// <summary>
        /// Set whether wimlib can print error and warning messages to the error file, which defaults to standard error.
        /// Error and warning messages may provide information that cannot be determined only from returned error codes.
        /// 
        /// By default, error messages are not printed.
        /// This setting applies globally (it is not per-WIM).
        /// This can be called before wimlib_global_init().
        /// </summary>
        /// <remarks>
        /// WIMLIB_ERR_UNSUPPORTED: wimlib was compiled using the --without-error-messages option.
        /// </remarks>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_print_errors([MarshalAs(UnmanagedType.I1)] bool showMessages);
        private wimlib_set_print_errors SetPrintErrorsPtr;

        internal void SetPrintErrors(bool showMessages)
        {
            lock (_errorFileLock)
            {
                ErrorCode ret = SetPrintErrorsPtr(showMessages);

                // When ret is ErrorCode.Unsupported, wimlib was compiled using the --without-error-messages option.
                // In that case, ManagedWimLib must not throw WimException.
                if (ret == ErrorCode.Unsupported)
                {
                    _errorPrintState = ErrorPrintState.NotSupported;
                }
                else
                {
                    WimException.CheckErrorCode(ret);
                    _errorPrintState = showMessages ? ErrorPrintState.PrintOn : ErrorPrintState.PrintOff;
                }
            }
        }

        internal string[] GetErrors()
        {
            lock (_errorFileLock)
            {
                if (_errorFile == null)
                    return null;
                if (_errorPrintState != ErrorPrintState.PrintOn)
                    return null;

                using (FileStream fs = new FileStream(_errorFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader r = new StreamReader(fs, UnicodeEncoding, false))
                {
                    return r.ReadToEnd().Split('\n').Select(x => x.Trim()).Where(x => 0 < x.Length).ToArray();
                }
            }
        }

        internal string GetLastError()
        {
            lock (_errorFileLock)
            {
                if (_errorFile == null)
                    return null;
                if (_errorPrintState != ErrorPrintState.PrintOn)
                    return null;

                using (FileStream fs = new FileStream(_errorFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader r = new StreamReader(fs, UnicodeEncoding, false))
                {
                    var lines = r.ReadToEnd().Split('\n').Select(x => x.Trim()).Where(x => 0 < x.Length);
                    return lines.LastOrDefault();
                }
            }
        }

        internal void ResetErrorFile()
        {
            lock (_errorFileLock)
            {
                if (_errorFile == null)
                    return;
                if (_errorPrintState != ErrorPrintState.PrintOn)
                    return;

                // Overwrite to Empty File
                using (FileStream fs = new FileStream(_errorFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter w = new StreamWriter(fs, UnicodeEncoding))
                {
                    w.WriteLine();
                }
            }
        }
        #endregion

        #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
        internal ErrorCode AddEmptyImage(IntPtr wim, string name, out int newIdxRet)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.AddEmptyImage(wim, name, out newIdxRet),
                _ => Utf8.AddEmptyImage(wim, name, out newIdxRet),
            };
        }

        internal ErrorCode AddImage(IntPtr wim, string source, string name, string configFile, AddFlags addFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.AddImage(wim, source, name, configFile, addFlags),
                _ => Utf8.AddImage(wim, source, name, configFile, addFlags),
            };
        }

        internal ErrorCode AddImageMultiSource(IntPtr wim, CaptureSource[] sources, UIntPtr numSources, string name, string configFile, AddFlags addFlags)
        {
            switch (UnicodeConvention)
            {
                case UnicodeConvention.Utf16:
                    switch (PlatformLongSize)
                    {
                        case PlatformLongSize.Long32:
                            Utf16d.CaptureSourceBaseL32[] capSrcsL32 = new Utf16d.CaptureSourceBaseL32[sources.Length];
                            for (int i = 0; i < sources.Length; i++)
                            {
                                CaptureSource src = sources[i];
                                capSrcsL32[i] = new Utf16d.CaptureSourceBaseL32
                                {
                                    FsSourcePath = src.FsSourcePath,
                                    WimTargetPath = src.WimTargetPath,
                                };
                            }
                            return Utf16.AddImageMultiSourceL32(wim, capSrcsL32, numSources, name, configFile, addFlags);
                        case PlatformLongSize.Long64:
                            Utf16d.CaptureSourceBaseL64[] capSrcsL64 = new Utf16d.CaptureSourceBaseL64[sources.Length];
                            for (int i = 0; i < sources.Length; i++)
                            {
                                CaptureSource src = sources[i];
                                capSrcsL64[i] = new Utf16d.CaptureSourceBaseL64
                                {
                                    FsSourcePath = src.FsSourcePath,
                                    WimTargetPath = src.WimTargetPath,
                                };
                            }
                            return Utf16.AddImageMultiSourceL64(wim, capSrcsL64, numSources, name, configFile, addFlags);
                    }
                    throw new PlatformNotSupportedException();
                case UnicodeConvention.Utf8:
                default:
                    switch (PlatformLongSize)
                    {
                        case PlatformLongSize.Long32:
                            Utf8d.CaptureSourceBaseL32[] capSrcsL32 = new Utf8d.CaptureSourceBaseL32[sources.Length];
                            for (int i = 0; i < sources.Length; i++)
                            {
                                CaptureSource src = sources[i];
                                capSrcsL32[i] = new Utf8d.CaptureSourceBaseL32
                                {
                                    FsSourcePath = src.FsSourcePath,
                                    WimTargetPath = src.WimTargetPath,
                                };
                            }
                            return Utf8.AddImageMultiSourceL32(wim, capSrcsL32, numSources, name, configFile, addFlags);
                        case PlatformLongSize.Long64:
                            Utf8d.CaptureSourceBaseL64[] capSrcsL64 = new Utf8d.CaptureSourceBaseL64[sources.Length];
                            for (int i = 0; i < sources.Length; i++)
                            {
                                CaptureSource src = sources[i];
                                capSrcsL64[i] = new Utf8d.CaptureSourceBaseL64
                                {
                                    FsSourcePath = src.FsSourcePath,
                                    WimTargetPath = src.WimTargetPath,
                                };
                            }
                            return Utf8.AddImageMultiSourceL64(wim, capSrcsL64, numSources, name, configFile, addFlags);
                    }
                    throw new PlatformNotSupportedException();
            }
        }

        internal ErrorCode AddTree(IntPtr wim, int image, string fsSourcePath, string wimTargetPath, AddFlags addFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.AddTree(wim, image, fsSourcePath, wimTargetPath, addFlags),
                _ => Utf8.AddTree(wim, image, fsSourcePath, wimTargetPath, addFlags),
            };
        }
        #endregion

        #region Create - CreateWim
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_create_new_wim(
            CompressionType ctype,
            out IntPtr wim_ret);
        internal wimlib_create_new_wim CreateNewWim;
        #endregion

        #region Delete - DeleteImage, DeletePath
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_delete_image(
            IntPtr wim,
            int image);
        internal wimlib_delete_image DeleteImage;

        internal ErrorCode DeletePath(IntPtr wim, int image, string path, DeleteFlags deleteFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.DeletePath(wim, image, path, deleteFlags),
                _ => Utf8.DeletePath(wim, image, path, deleteFlags),
            };
        }
        #endregion

        #region Export - ExportImage
        internal ErrorCode ExportImage(IntPtr srcWim, int srcImage, IntPtr destWim, string destName, string destDesc, ExportFlags exportFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.ExportImage(srcWim, srcImage, destWim, destName, destDesc, exportFlags),
                _ => Utf8.ExportImage(srcWim, srcImage, destWim, destName, destDesc, exportFlags),
            };
        }
        #endregion

        #region Extract - ExtractImage, ExtractPaths, ExtractPathList
        internal ErrorCode ExtractImage(IntPtr wim, int image, string target, ExtractFlags extractFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.ExtractImage(wim, image, target, extractFlags),
                _ => Utf8.ExtractImage(wim, image, target, extractFlags),
            };
        }

        internal ErrorCode ExtractPathList(IntPtr wim, int image, string target, string pathListFile, ExtractFlags extractFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.ExtractPathList(wim, image, target, pathListFile, extractFlags),
                _ => Utf8.ExtractPathList(wim, image, target, pathListFile, extractFlags),
            };
        }

        internal ErrorCode ExtractPaths(IntPtr wim, int image, string target, string[] paths, UIntPtr numPaths, ExtractFlags extract_flags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.ExtractPaths(wim, image, target, paths, numPaths, extract_flags),
                _ => Utf8.ExtractPaths(wim, image, target, paths, numPaths, extract_flags),
            };
        }
        #endregion

        #region Free - Free
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_free(IntPtr wim);
        internal wimlib_free Free;
        #endregion

        #region GetImageInfo - GetImageDescription, GetImageName, GetImageProperty
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_image_description(
            IntPtr wim,
            int image);
        internal wimlib_get_image_description GetImageDescription;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_image_name(
            IntPtr wim,
            int image);
        internal wimlib_get_image_name GetImageName;

        internal IntPtr GetImageProperty(IntPtr wim, int image, string property_name)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.GetImageProperty(wim, image, property_name),
                _ => Utf8.GetImageProperty(wim, image, property_name),
            };
        }
        #endregion

        #region GetWimInfo - GetWimInfo, GetXmlData, IsImageNameInUse, ResolveImage
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_get_wim_info(
            IntPtr wim,
            IntPtr info);
        internal wimlib_get_wim_info GetWimInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_get_xml_data(
            IntPtr wim,
            ref IntPtr buf_ret,
            ref UIntPtr bufsize_ret); // size_t
        internal wimlib_get_xml_data GetXmlData;

        internal bool IsImageNameInUse(IntPtr wim, string name)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.IsImageNameInUse(wim, name),
                _ => Utf8.IsImageNameInUse(wim, name),
            };
        }

        internal int ResolveImage(IntPtr wim, string imageNameOrNum)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.ResolveImage(wim, imageNameOrNum),
                _ => Utf8.ResolveImage(wim, imageNameOrNum),
            };
        }
        #endregion

        #region GetVersion - GetVersion, GetVersionTuple
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate uint wimlib_get_version();
        /// <summary>
        /// Return the version of wimlib as a 32-bit number whose top 12 bits contain the
        /// major version, the next 10 bits contain the minor version, and the low 10
        /// bits contain the patch version.
        /// </summary>
        /// <remarks>
        /// In other words, the returned value is equal to ((WIMLIB_MAJOR_VERSION &lt;&lt;
        /// 20) | (WIMLIB_MINOR_VERSION &lt;&lt; 10) | WIMLIB_PATCH_VERSION) for the
        /// corresponding header file.
        /// </remarks>
        internal wimlib_get_version GetVersion;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_version_string();
        internal wimlib_get_version_string GetVersionString;
        #endregion

        #region Iterate - IterateDirTree, IterateLookupTable
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate int NativeIterateDirTreeCallback(
            IntPtr dentry,
            IntPtr progctx);

        internal int IterateDirTree(IntPtr wim, int image, string path, IterateDirTreeFlags flags, NativeIterateDirTreeCallback cb, IntPtr userCtx)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.IterateDirTree(wim, image, path, flags, cb, userCtx),
                _ => Utf8.IterateDirTree(wim, image, path, flags, cb, userCtx),
            };
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate int NativeIterateLookupTableCallback(
            ResourceEntry resource,
            IntPtr progctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate int wimlib_iterate_lookup_table(
            IntPtr wim,
            IterateLookupTableFlags flags,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeIterateLookupTableCallback cb,
            IntPtr user_ctx);
        internal wimlib_iterate_lookup_table IterateLookupTable;
        #endregion

        #region Join - Join, JoinWithProgress
        internal ErrorCode Join(string[] swms, uint numSwms, string outputPath, OpenFlags swmsOpenFlags, WriteFlags writeFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.Join(swms, numSwms, outputPath, swmsOpenFlags, writeFlags),
                _ => Utf8.Join(swms, numSwms, outputPath, swmsOpenFlags, writeFlags),
            };
        }

        internal ErrorCode JoinWithProgress(string[] swms, uint numSwms, string outputPath, OpenFlags swmsOpenFlags, WriteFlags writeFlags, NativeProgressFunc progfunc, IntPtr progctx)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.JoinWithProgress(swms, numSwms, outputPath, swmsOpenFlags, writeFlags, progfunc, progctx),
                _ => Utf8.JoinWithProgress(swms, numSwms, outputPath, swmsOpenFlags, writeFlags, progfunc, progctx),
            };
        }
        #endregion

        #region Open - OpenWim, OpenWithProgress
        internal ErrorCode OpenWim(string wimFile, OpenFlags openFlags, out IntPtr wimRet)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.OpenWim(wimFile, openFlags, out wimRet),
                _ => Utf8.OpenWim(wimFile, openFlags, out wimRet),
            };
        }

        internal ErrorCode OpenWimWithProgress(string wimFile, OpenFlags openFlags, out IntPtr wimRet, NativeProgressFunc progfunc, IntPtr progctx)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.OpenWimWithProgress(wimFile, openFlags, out wimRet, progfunc, progctx),
                _ => Utf8.OpenWimWithProgress(wimFile, openFlags, out wimRet, progfunc, progctx),
            };
        }
        #endregion

        #region Mount - MountImage (Linux Only)
        internal ErrorCode MountImage(IntPtr wim, int image, string dir, MountFlags mountFlags, string stagingDir)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.MountImage(wim, image, dir, mountFlags, stagingDir),
                _ => Utf8.MountImage(wim, image, dir, mountFlags, stagingDir),
            };
        }
        #endregion

        #region Reference - ReferenceResourceFiles, ReferenceResources, ReferenceTemplateImage
        internal ErrorCode ReferenceResourceFiles(IntPtr wim, string[] resourceWimfilesOrGlobs, uint count, RefFlags refFlags, OpenFlags openFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.ReferenceResourceFiles(wim, resourceWimfilesOrGlobs, count, refFlags, openFlags),
                _ => Utf8.ReferenceResourceFiles(wim, resourceWimfilesOrGlobs, count, refFlags, openFlags),
            };
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_reference_resources(
            IntPtr wim,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] resource_wims,
            uint num_resource_wims,
            RefFlags ref_flags);
        internal wimlib_reference_resources ReferenceResources;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_reference_template_image(
            IntPtr wim,
            int new_image,
            IntPtr template_wim,
            int template_image,
            int flags);
        internal wimlib_reference_template_image ReferenceTemplateImage;
        #endregion

        #region Rename - RenamePath
        internal ErrorCode RenamePath(IntPtr wim, int image, string sourcePath, string destPath)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.RenamePath(wim, image, sourcePath, destPath),
                _ => Utf8.RenamePath(wim, image, sourcePath, destPath),
            };
        }
        #endregion

        #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty, SetWimInfo
        internal ErrorCode SetImageDescription(IntPtr wim, int image, string description)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.SetImageDescription(wim, image, description),
                _ => Utf8.SetImageDescription(wim, image, description),
            };
        }

        internal ErrorCode SetImageFlags(IntPtr wim, int image, string flags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.SetImageFlags(wim, image, flags),
                _ => Utf8.SetImageFlags(wim, image, flags),
            };
        }

        internal ErrorCode SetImageName(IntPtr wim, int image, string name)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.SetImageName(wim, image, name),
                _ => Utf8.SetImageName(wim, image, name),
            };
        }

        internal ErrorCode SetImageProperty(IntPtr wim, int image, string propertyName, string propertyValue)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.SetImageProperty(wim, image, propertyName, propertyValue),
                _ => Utf8.SetImageProperty(wim, image, propertyName, propertyValue),
            };
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_wim_info(
            IntPtr wim,
            WimInfo info,
            ChangeFlags which);
        internal wimlib_set_wim_info SetWimInfo;
        #endregion

        #region SetOutput - SetOutputChunkSize, SetOutputPackChunkSize, SetOutputCompressionType, SetOutputPackCompressionType
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_chunk_size(
            IntPtr wim,
            uint chunk_size);
        internal wimlib_set_output_chunk_size SetOutputChunkSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_pack_chunk_size(
            IntPtr wim,
            uint chunk_size);
        internal wimlib_set_output_pack_chunk_size SetOutputPackChunkSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_compression_type(
            IntPtr wim,
            CompressionType ctype);
        internal wimlib_set_output_compression_type SetOutputCompressionType;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_pack_compression_type(
            IntPtr wim,
            CompressionType ctype);
        internal wimlib_set_output_pack_compression_type SetOutputPackCompressionType;
        #endregion

        #region Split - Split
        internal ErrorCode Split(IntPtr wim, string swmName, ulong partSize, WriteFlags writeFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.Split(wim, swmName, partSize, writeFlags),
                _ => Utf8.Split(wim, swmName, partSize, writeFlags),
            };
        }
        #endregion

        #region Verify - VerifyWim
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_verify_wim(
            IntPtr wim,
            int verify_flags);
        internal wimlib_verify_wim VerifyWim;
        #endregion

        #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
        internal ErrorCode UnmountImage(string dir, UnmountFlags unmountFlags)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.UnmountImage(dir, unmountFlags),
                _ => Utf8.UnmountImage(dir, unmountFlags),
            };
        }

        internal ErrorCode UnmountImageWithProgress(string dir, UnmountFlags unmountFlags, NativeProgressFunc progfunc, IntPtr progctx)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.UnmountImageWithProgress(dir, unmountFlags, progfunc, progctx),
                _ => Utf8.UnmountImageWithProgress(dir, unmountFlags, progfunc, progctx),
            };
        }
        #endregion

        #region Update - UpdateImage
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_update_image_32(
            IntPtr wim,
            int image,
            [MarshalAs(UnmanagedType.LPArray)] UpdateCommand32[] cmds,
            uint num_cmds,
            UpdateFlags update_flags);
        internal wimlib_update_image_32 UpdateImage32;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_update_image_64(
            IntPtr wim,
            int image,
            [MarshalAs(UnmanagedType.LPArray)] UpdateCommand64[] cmds,
            ulong num_cmds,
            UpdateFlags update_flags);
        internal wimlib_update_image_64 UpdateImage64;
        #endregion

        #region Write - Write, Overwrite
        internal ErrorCode Write(IntPtr wim, string path, int image, WriteFlags writeFlags, uint numThreads)
        {
            return UnicodeConvention switch
            {
                UnicodeConvention.Utf16 => Utf16.Write(wim, path, image, writeFlags, numThreads),
                _ => Utf8.Write(wim, path, image, writeFlags, numThreads),
            };
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_overwrite(
            IntPtr wim,
            WriteFlags write_flags,
            uint numThreads);
        internal wimlib_overwrite Overwrite;
        #endregion

        #region CompressInfo - SetDefaultCompressionLevel, GetCompressorNeededMemory
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_default_compression_level(
            int ctype,
            uint compression_level);
        internal wimlib_set_default_compression_level SetDefaultCompressionLevel;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ulong wimlib_get_compressor_needed_memory(
            CompressionType ctype,
            UIntPtr max_block_size, // size_t
            uint compression_level);
        internal wimlib_get_compressor_needed_memory GetCompressorNeededMemory;
        #endregion

        #region Compressor - CreateCompressor, FreeCompressor, Compress
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_create_compressor(
            CompressionType ctype,
            UIntPtr max_block_size, // size_t
            uint compression_level,
            out IntPtr compressor_ret);
        internal wimlib_create_compressor CreateCompressor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_free_compressor(IntPtr compressor);
        internal wimlib_free_compressor FreeCompressor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate UIntPtr wimlib_compress( // size_t
            byte* uncompressed_data,
            UIntPtr uncompressed_size, // size_t
            byte* compressed_data,
            UIntPtr compressed_size_avail, // size_t
            IntPtr compressor);
        internal wimlib_compress Compress;
        #endregion

        #region Decompressor - CreateDecompressor, FreeDecompressor, Decompress
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_create_decompressor(
            CompressionType ctype,
            UIntPtr max_block_size, // size_t
            out IntPtr decompressor_ret);
        internal wimlib_create_decompressor CreateDecompressor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_free_decompressor(IntPtr decompressor);
        internal wimlib_free_decompressor FreeDecompressor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate int wimlib_decompress( // size_t
            byte* compressed_data,
            UIntPtr compressed_size, // size_t
            byte* uncompressed_data,
            UIntPtr uncompressed_size_avail, // size_t
            IntPtr decompressor);
        internal wimlib_decompress Decompress;
        #endregion
        #endregion

        #region Utility
        internal static void SetBitField(ref uint bitField, int bitShift, bool value)
        {
            if (value)
                bitField |= (uint)(1 << bitShift);
            else
                bitField &= ~(uint)(1 << bitShift);
        }

        internal static bool GetBitField(uint bitField, int bitShift)
        {
            return (bitField & (1 << bitShift)) != 0;
        }
        #endregion
    }

    #region enum ErrorPrintState
    /// <summary>
    /// Represents whether wimlib is printing error messages or not.
    /// </summary>
    public enum ErrorPrintState
    {
        /// <summary>
        /// Error messages are not being printed to ErrorFile.
        /// </summary>
        PrintOff = 0,
        /// <summary>
        /// Error messages are being printed to ErrorFile.
        /// </summary>
        PrintOn = 1,
        /// <summary>
        /// wimlib was not built with --without-error-messages option.
        /// </summary>
        NotSupported = 2,
    }
    #endregion
}
