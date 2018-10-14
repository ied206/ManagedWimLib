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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
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

namespace ManagedWimLib
{
    #region NativeMethods
    internal static class NativeMethods
    {
        #region Const
        public const string MsgInitFirstError = "Please call Wim.GlobalInit() first!";
        public const string MsgAlreadyInited = "ManagedWimLib is already initialized.";
        public const string MsgErrorFileNotSet = "ErrorFile is not set unable to read last error.";
        public const string MsgPrintErrorsDisabled = "Error is not being logged, unable to read last error.";
        #endregion

        #region Fields and Properties
        internal enum LongBits
        {
            Long64 = 0, // Windows, Linux 32bit
            Long32 = 1, // Linux 64bit
        }

        internal static IntPtr hModule;
        internal static bool Loaded => hModule != IntPtr.Zero;
        
        public static bool UseUtf16;
        public static Encoding Encoding => UseUtf16 ? Encoding.Unicode : new UTF8Encoding(false);
        internal static LongBits LongBitType { get; set; }
        
        public static string ErrorFile = null;
        public static bool PrintErrorsEnabled = false;
        #endregion
        
        #region MarshalString
        public static string MarshalPtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return string.Empty;
            return UseUtf16 ? Marshal.PtrToStringUni(ptr) : Marshal.PtrToStringAnsi(ptr);
        }

        public static IntPtr MarshalStringToPtr(string str)
        {
            return UseUtf16 ? Marshal.StringToHGlobalUni(str) : Marshal.StringToHGlobalAnsi(str);
        }
        #endregion
        
        #region Windows kernel32 API
        internal static class Win32
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

            [DllImport("kernel32.dll")]
            internal static extern int FreeLibrary(IntPtr hModule);
        }
        #endregion

        #region Linux libdl API
        internal static class Linux
        {
#pragma warning disable IDE1006
            internal const int RTLD_NOW = 0x0002;
            internal const int RTLD_GLOBAL = 0x0100;

            [DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int dlclose(IntPtr handle);

            [DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl)]
            internal static extern string dlerror();

            [DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);
#pragma warning restore IDE1006
        }
        #endregion

        #region LoadFunctions, ResetFunctions
        private static T GetFuncPtr<T>(string funcSymbol) where T : Delegate
        {
            IntPtr funcPtr;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                funcPtr = Win32.GetProcAddress(hModule, funcSymbol);
                if (funcPtr == IntPtr.Zero)
                    throw new ArgumentException($"Cannot import [{funcSymbol}]", new Win32Exception());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                funcPtr = Linux.dlsym(hModule, funcSymbol);
                if (funcPtr == IntPtr.Zero)
                    throw new ArgumentException($"Cannot import [{funcSymbol}]", Linux.dlerror());
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
        }

        internal static void LoadFuntions()
        {
            if (UseUtf16)
            {
                #region Error - SetErrorFile
                Utf16.SetErrorFile = GetFuncPtr<Utf16.wimlib_set_error_file_by_name>("wimlib_set_error_file_by_name");
                #endregion
                
                #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
                Utf16.AddEmptyImage = GetFuncPtr<Utf16.wimlib_add_empty_image>("wimlib_add_empty_image");
                Utf16.AddImage = GetFuncPtr<Utf16.wimlib_add_image>("wimlib_add_image");
                Utf16.AddImageMultiSourceL32 = GetFuncPtr<Utf16.wimlib_add_image_multisource_l32>("wimlib_add_image_multisource");
                Utf16.AddImageMultiSourceL64 = GetFuncPtr<Utf16.wimlib_add_image_multisource_l64>("wimlib_add_image_multisource");
                Utf16.AddTree = GetFuncPtr<Utf16.wimlib_add_tree>("wimlib_add_tree");
                #endregion
                
                #region Delete - DeletePath
                Utf16.DeletePath = GetFuncPtr<Utf16.wimlib_delete_path>("wimlib_delete_path");
                #endregion
                
                #region Export - ExportImage
                Utf16.ExportImage = GetFuncPtr<Utf16.wimlib_export_image>("wimlib_export_image");
                #endregion
                
                #region Extract - ExtractImage, ExtractPathList, ExtractPaths
                Utf16.ExtractImage = GetFuncPtr<Utf16.wimlib_extract_image>("wimlib_extract_image");
                Utf16.ExtractPathList = GetFuncPtr<Utf16.wimlib_extract_pathlist>("wimlib_extract_pathlist");
                Utf16.ExtractPaths = GetFuncPtr<Utf16.wimlib_extract_paths>("wimlib_extract_paths");
                #endregion
                
                #region GetImageInfo - GetImageProperty
                Utf16.GetImageProperty = GetFuncPtr<Utf16.wimlib_get_image_property>("wimlib_get_image_property");
                #endregion

                #region GetWimInfo - IsImageNameInUse, ResolveImage
                Utf16.IsImageNameInUse = GetFuncPtr<Utf16.wimlib_image_name_in_use>("wimlib_image_name_in_use");
                Utf16.ResolveImage = GetFuncPtr<Utf16.wimlib_resolve_image>("wimlib_resolve_image");
                #endregion
                
                #region Iterate - IterateDirTree
                Utf16.IterateDirTree = GetFuncPtr<Utf16.wimlib_iterate_dir_tree>("wimlib_iterate_dir_tree");
                #endregion
                
                #region Join - Join, JoinWithProgress
                Utf16.Join = GetFuncPtr<Utf16.wimlib_join>("wimlib_join");
                Utf16.JoinWithProgress = GetFuncPtr<Utf16.wimlib_join_with_progress>("wimlib_join_with_progress");
                #endregion

                #region Open - Open, OpenWithProgress
                Utf16.OpenWim = GetFuncPtr<Utf16.wimlib_open_wim>("wimlib_open_wim");
                Utf16.OpenWimWithProgress = GetFuncPtr<Utf16.wimlib_open_wim_with_progress>("wimlib_open_wim_with_progress");
                #endregion
                
                #region Mount - MountImage (Linux Only)
                Utf16.MountImage = GetFuncPtr<Utf16.wimlib_mount_image>("wimlib_mount_image");
                #endregion
                
                #region Reference - ReferenceTemplateImage
                Utf16.ReferenceResourceFiles = GetFuncPtr<Utf16.wimlib_reference_resource_files>("wimlib_reference_resource_files");
                #endregion
                
                #region Rename - RenamePath
                Utf16.RenamePath = GetFuncPtr<Utf16.wimlib_rename_path>("wimlib_rename_path");
                #endregion
                
                #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
                // wimlib_set_image_descripton is misspelled from wimlib itself.
                Utf16.SetImageDescription = GetFuncPtr<Utf16.wimlib_set_image_description>("wimlib_set_image_descripton");
                Utf16.SetImageFlags = GetFuncPtr<Utf16.wimlib_set_image_flags>("wimlib_set_image_flags");
                Utf16.SetImageName = GetFuncPtr<Utf16.wimlib_set_image_name>("wimlib_set_image_name");
                Utf16.SetImageProperty = GetFuncPtr<Utf16.wimlib_set_image_property>("wimlib_set_image_property");
                #endregion
                
                #region Split - Split
                Utf16.Split = GetFuncPtr<Utf16.wimlib_split>("wimlib_split");
                #endregion
                
                #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
                Utf16.UnmountImage = GetFuncPtr<Utf16.wimlib_unmount_image>("wimlib_unmount_image");
                Utf16.UnmountImageWithProgress = GetFuncPtr<Utf16.wimlib_unmount_image_with_progress>("wimlib_unmount_image_with_progress");
                #endregion
                
                #region Write - Write
                Utf16.Write = GetFuncPtr<Utf16.wimlib_write>("wimlib_write");
                #endregion
            }
            else
            {
                #region Error - SetErrorFile
                Utf8.SetErrorFile = GetFuncPtr<Utf8.wimlib_set_error_file_by_name>("wimlib_set_error_file_by_name");
                #endregion
                
                #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
                Utf8.AddEmptyImage = GetFuncPtr<Utf8.wimlib_add_empty_image>("wimlib_add_empty_image");
                Utf8.AddImage = GetFuncPtr<Utf8.wimlib_add_image>("wimlib_add_image");
                Utf8.AddImageMultiSourceL32 = GetFuncPtr<Utf8.wimlib_add_image_multisource_l32>("wimlib_add_image_multisource");
                Utf8.AddImageMultiSourceL64 = GetFuncPtr<Utf8.wimlib_add_image_multisource_l64>("wimlib_add_image_multisource");
                Utf8.AddTree = GetFuncPtr<Utf8.wimlib_add_tree>("wimlib_add_tree");
                #endregion
                
                #region Delete - DeletePath
                Utf8.DeletePath = GetFuncPtr<Utf8.wimlib_delete_path>("wimlib_delete_path");
                #endregion
                
                #region Export - ExportImage
                Utf8.ExportImage = GetFuncPtr<Utf8.wimlib_export_image>("wimlib_export_image");
                #endregion
                
                #region Extract - ExtractImage, ExtractPathList, ExtractPaths
                Utf8.ExtractImage = GetFuncPtr<Utf8.wimlib_extract_image>("wimlib_extract_image");
                Utf8.ExtractPathList = GetFuncPtr<Utf8.wimlib_extract_pathlist>("wimlib_extract_pathlist");
                Utf8.ExtractPaths = GetFuncPtr<Utf8.wimlib_extract_paths>("wimlib_extract_paths");
                #endregion
                
                #region GetImageInfo - GetImageProperty
                Utf8.GetImageProperty = GetFuncPtr<Utf8.wimlib_get_image_property>("wimlib_get_image_property");
                #endregion

                #region GetWimInfo - IsImageNameInUse, ResolveImage
                Utf8.IsImageNameInUse = GetFuncPtr<Utf8.wimlib_image_name_in_use>("wimlib_image_name_in_use");
                Utf8.ResolveImage = GetFuncPtr<Utf8.wimlib_resolve_image>("wimlib_resolve_image");
                #endregion
                
                #region Iterate - IterateDirTree
                Utf8.IterateDirTree = GetFuncPtr<Utf8.wimlib_iterate_dir_tree>("wimlib_iterate_dir_tree");
                #endregion
                
                #region Join - Join, JoinWithProgress
                Utf8.Join = GetFuncPtr<Utf8.wimlib_join>("wimlib_join");
                Utf8.JoinWithProgress = GetFuncPtr<Utf8.wimlib_join_with_progress>("wimlib_join_with_progress");
                #endregion

                #region Open - Open, OpenWithProgress
                Utf8.OpenWim = GetFuncPtr<Utf8.wimlib_open_wim>("wimlib_open_wim");
                Utf8.OpenWimWithProgress = GetFuncPtr<Utf8.wimlib_open_wim_with_progress>("wimlib_open_wim_with_progress");
                #endregion
                
                #region Mount - MountImage (Linux Only)
                Utf8.MountImage = GetFuncPtr<Utf8.wimlib_mount_image>("wimlib_mount_image");
                #endregion
                
                #region Reference - ReferenceTemplateImage
                Utf8.ReferenceResourceFiles = GetFuncPtr<Utf8.wimlib_reference_resource_files>("wimlib_reference_resource_files");
                #endregion
                
                #region Rename - RenamePath
                Utf8.RenamePath = GetFuncPtr<Utf8.wimlib_rename_path>("wimlib_rename_path");
                #endregion
                
                #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
                // wimlib_set_image_descripton is misspelled from wimlib itself.
                Utf8.SetImageDescription = GetFuncPtr<Utf8.wimlib_set_image_description>("wimlib_set_image_descripton");
                Utf8.SetImageFlags = GetFuncPtr<Utf8.wimlib_set_image_flags>("wimlib_set_image_flags");
                Utf8.SetImageName = GetFuncPtr<Utf8.wimlib_set_image_name>("wimlib_set_image_name");
                Utf8.SetImageProperty = GetFuncPtr<Utf8.wimlib_set_image_property>("wimlib_set_image_property");
                #endregion
                
                #region Split - Split
                Utf8.Split = GetFuncPtr<Utf8.wimlib_split>("wimlib_split");
                #endregion
                
                #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
                Utf8.UnmountImage = GetFuncPtr<Utf8.wimlib_unmount_image>("wimlib_unmount_image");
                Utf8.UnmountImageWithProgress = GetFuncPtr<Utf8.wimlib_unmount_image_with_progress>("wimlib_unmount_image_with_progress");
                #endregion
                
                #region Write - Write
                Utf8.Write = GetFuncPtr<Utf8.wimlib_write>("wimlib_write");
                #endregion
            }
            
            #region Global - GlobalInit, GlobalCleanup
            GlobalInit = GetFuncPtr<wimlib_global_init>("wimlib_global_init");
            GlobalCleanup = GetFuncPtr<wimlib_global_cleanup>("wimlib_global_cleanup");
            #endregion

            #region Error - GetErrorString, SetPrintErrors
            GetErrorString = GetFuncPtr<wimlib_get_error_string>("wimlib_get_error_string");
            SetPrintErrors = GetFuncPtr<wimlib_set_print_errors>("wimlib_set_print_errors");
            #endregion

            #region Create - CreateWim
            CreateNewWim = GetFuncPtr<wimlib_create_new_wim>("wimlib_create_new_wim");
            #endregion

            #region Delete - DeleteImage
            DeleteImage = GetFuncPtr<wimlib_delete_image>("wimlib_delete_image");
            #endregion

            #region Free - Free
            Free = GetFuncPtr<wimlib_free>("wimlib_free");
            #endregion

            #region GetImageInfo - GetImageDescription, GetImageName
            GetImageDescription = GetFuncPtr<wimlib_get_image_description>("wimlib_get_image_description");
            GetImageName = GetFuncPtr<wimlib_get_image_name>("wimlib_get_image_name");
            #endregion

            #region GetWimInfo - GetWimInfo, GetXmlData
            GetWimInfo = GetFuncPtr<wimlib_get_wim_info>("wimlib_get_wim_info");
            GetXmlData = GetFuncPtr<wimlib_get_xml_data>("wimlib_get_xml_data");
            #endregion

            #region GetVersion - GetVersion
            GetVersionPtr = GetFuncPtr<wimlib_get_version>("wimlib_get_version");
            #endregion

            #region Iterate - IterateLookupTable
            IterateLookupTable = GetFuncPtr<wimlib_iterate_lookup_table>("wimlib_iterate_lookup_table");
            #endregion

            #region Reference - ReferenceTemplateImage, ReferenceResourceFiles, ReferenceResources
            ReferenceResources = GetFuncPtr<wimlib_reference_resources>("wimlib_reference_resources");
            ReferenceTemplateImage = GetFuncPtr<wimlib_reference_template_image>("wimlib_reference_template_image");
            #endregion

            #region Callback - RegsiterProgressFunction
            RegisterProgressFunction = GetFuncPtr<wimlib_register_progress_function_delegate>("wimlib_register_progress_function");
            #endregion

            #region SetImageInfo - SetWimInfo
            SetWimInfo = GetFuncPtr<wimlib_set_wim_info>("wimlib_set_wim_info");
            #endregion

            #region SetOutput - SetOutputChunkSize, SetOutputPackChunkSize, SetOutputCompressionType, SetOutputPackCompressionType
            SetOutputChunkSize = GetFuncPtr<wimlib_set_output_chunk_size>("wimlib_set_output_chunk_size");
            SetOutputPackChunkSize = GetFuncPtr<wimlib_set_output_pack_chunk_size>("wimlib_set_output_pack_chunk_size");
            SetOutputCompressionType = GetFuncPtr<wimlib_set_output_compression_type>("wimlib_set_output_compression_type");
            SetOutputPackCompressionType = GetFuncPtr<wimlib_set_output_pack_compression_type>("wimlib_set_output_pack_compression_type");
            #endregion

            #region Verify - VerifyWim
            VerifyWim = GetFuncPtr<wimlib_verify_wim>("wimlib_verify_wim");
            #endregion

            #region Update - UpdateImage
            UpdateImage32 = GetFuncPtr<wimlib_update_image_32>("wimlib_update_image");
            UpdateImage64 = GetFuncPtr<wimlib_update_image_64>("wimlib_update_image");
            #endregion

            #region Write - Overwrite
            Overwrite = GetFuncPtr<wimlib_overwrite>("wimlib_overwrite");
            #endregion
        }

        internal static void ResetFuntions()
        {
            #region Global - GlobalInit, GlobalCleanup
            GlobalInit = null;
            GlobalCleanup = null;
            #endregion

            #region Error - GetErrorString, SetErrorFile, SetPrintErrors
            GetErrorString = null;
            Utf16.SetErrorFile = null;
            Utf8.SetErrorFile = null;
            SetPrintErrors = null;
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

            #region GetVersion - GetVersion
            GetVersionPtr = null;
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
        }
        #endregion

        #region WimLib Function Pointer
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        internal static class Utf16
        {
            internal const UnmanagedType StrType = UnmanagedType.LPWStr;
            internal const CharSet StructCharSet = CharSet.Unicode;
            
            #region Error - SetErrorFile
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_error_file_by_name(
                [MarshalAs(StrType)] string path);
            internal static wimlib_set_error_file_by_name SetErrorFile;
            #endregion
            
            #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_empty_image(
                IntPtr wim,
                [MarshalAs(StrType)] string name,
                out int new_idx_ret);
            internal static wimlib_add_empty_image AddEmptyImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image(
                IntPtr wim,
                [MarshalAs(StrType)] string source,
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal static wimlib_add_image AddImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l32(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL32[] sources,
                IntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal static wimlib_add_image_multisource_l32 AddImageMultiSourceL32;
            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l64(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL64[] sources,
                IntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal static wimlib_add_image_multisource_l64 AddImageMultiSourceL64;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string fs_source_path,
                [MarshalAs(StrType)] string wim_target_path,
                AddFlags add_flags);
            internal static wimlib_add_tree AddTree;
            #endregion
            
            #region Delete - DeletePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_delete_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                DeleteFlags delete_flags);
            internal static wimlib_delete_path DeletePath;
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
            internal static wimlib_export_image ExportImage;
            #endregion
            
            #region Extract - ExtractImage, ExtractPaths, ExtractPathList
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                ExtractFlags extract_flags);
            internal static wimlib_extract_image ExtractImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_pathlist(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(StrType)] string path_list_file,
                ExtractFlags extract_flags);
            internal static wimlib_extract_pathlist ExtractPathList;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_paths(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] paths,
                IntPtr num_paths, // size_t
                ExtractFlags extract_flags);
            internal static wimlib_extract_paths ExtractPaths;
            #endregion
            
            #region GetImageInfo - GetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr wimlib_get_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name);
            internal static wimlib_get_image_property GetImageProperty;
            #endregion
            
            #region GetWimInfo - IsImageNameInUse, ResolveImage
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool wimlib_image_name_in_use(
                IntPtr wim,
                [MarshalAs(StrType)] string name);
            internal static wimlib_image_name_in_use IsImageNameInUse;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int wimlib_resolve_image(
                IntPtr wim,
                [MarshalAs(StrType)] string image_name_or_num);
            internal static wimlib_resolve_image ResolveImage;
            #endregion
            
            #region Iterate - IterateDirTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            internal delegate ErrorCode wimlib_iterate_dir_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                IterateFlags flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeIterateDirTreeCallback cb,
                IntPtr user_ctx);
            internal static wimlib_iterate_dir_tree IterateDirTree;
            #endregion
            
            #region Join - Join, JoinWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags);
            internal static wimlib_join Join;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join_with_progress(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal static wimlib_join_with_progress JoinWithProgress;
            #endregion
            
            #region Open - OpenWim, OpenWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret);
            internal static wimlib_open_wim OpenWim;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim_with_progress(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal static wimlib_open_wim_with_progress OpenWimWithProgress;
            #endregion
            
            #region Mount - MountImage (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_mount_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string dir,
                MountFlags mount_flags,
                [MarshalAs(StrType)] string staging_dir);
            internal static wimlib_mount_image MountImage;
            #endregion            
            
            #region Reference - ReferenceResourceFiles
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_reference_resource_files(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] resource_wimfiles_or_globs,
                uint count,
                RefFlags ref_flags,
                OpenFlags open_flags);
            internal static wimlib_reference_resource_files ReferenceResourceFiles;
            #endregion
            
            #region Rename - RenamePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_rename_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string source_path,
                [MarshalAs(StrType)] string dest_path);
            internal static wimlib_rename_path RenamePath;
            #endregion
            
            #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_description(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string description);
            internal static wimlib_set_image_description SetImageDescription;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_flags(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string flags);
            internal static wimlib_set_image_flags SetImageFlags;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_name(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string name);
            internal static wimlib_set_image_name SetImageName;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name,
                [MarshalAs(StrType)] string property_value);
            internal static wimlib_set_image_property SetImageProperty;
            #endregion
            
            #region Split - Split
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_split(
                IntPtr wim,
                [MarshalAs(StrType)] string swm_name,
                ulong part_size,
                WriteFlags write_flags);
            internal static wimlib_split Split;
            #endregion
            
            #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags);
            internal static wimlib_unmount_image UnmountImage;
            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image_with_progress(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal static wimlib_unmount_image_with_progress UnmountImageWithProgress;
            #endregion
            
            #region Write - Write
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_write(
                IntPtr wim,
                [MarshalAs(StrType)] string path,
                int image,
                WriteFlags write_flags,
                uint num_threads);
            internal static wimlib_write Write;
            #endregion
            
            #region Struct CaptureSourceBase
            /// <summary>
            /// An array of these structures is passed to Wim.AddImageMultiSource() to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LLP64 platforms (Windows)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            public struct CaptureSourceBaseL32
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
            /// An array of these structures is passed to Wim.AddImageMultiSource() to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LP64 platforms (64bit POSIX)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            public struct CaptureSourceBaseL64
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
        
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        internal static class Utf8
        {
            internal const UnmanagedType StrType = UnmanagedType.LPStr;
            internal const CharSet StructCharSet = CharSet.Ansi;
            
            #region Error - SetErrorFile
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_error_file_by_name(
                [MarshalAs(StrType)] string path);
            internal static wimlib_set_error_file_by_name SetErrorFile;
            #endregion
            
            #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_empty_image(
                IntPtr wim,
                [MarshalAs(StrType)] string name,
                out int new_idx_ret);
            internal static wimlib_add_empty_image AddEmptyImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image(
                IntPtr wim,
                [MarshalAs(StrType)] string source,
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal static wimlib_add_image AddImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l32(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL32[] sources,
                IntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal static wimlib_add_image_multisource_l32 AddImageMultiSourceL32;
            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_image_multisource_l64(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray)] CaptureSourceBaseL64[] sources,
                IntPtr num_sources, // size_t
                [MarshalAs(StrType)] string name,
                [MarshalAs(StrType)] string config_file,
                AddFlags add_flags);
            internal static wimlib_add_image_multisource_l64 AddImageMultiSourceL64;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_add_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string fs_source_path,
                [MarshalAs(StrType)] string wim_target_path,
                AddFlags add_flags);
            internal static wimlib_add_tree AddTree;
            #endregion
            
            #region Delete - DeletePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_delete_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                DeleteFlags delete_flags);
            internal static wimlib_delete_path DeletePath;
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
            internal static wimlib_export_image ExportImage;
            #endregion
            
            #region Extract - ExtractImage, ExtractPaths, ExtractPathList
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                ExtractFlags extract_flags);
            internal static wimlib_extract_image ExtractImage;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_pathlist(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(StrType)] string path_list_file,
                ExtractFlags extract_flags);
            internal static wimlib_extract_pathlist ExtractPathList;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_extract_paths(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string target,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] paths,
                IntPtr num_paths, // size_t
                ExtractFlags extract_flags);
            internal static wimlib_extract_paths ExtractPaths;
            #endregion
            
            #region GetImageInfo - GetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr wimlib_get_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name);
            internal static wimlib_get_image_property GetImageProperty;
            #endregion
            
            #region GetWimInfo - IsImageNameInUse, ResolveImage
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool wimlib_image_name_in_use(
                IntPtr wim,
                [MarshalAs(StrType)] string name);
            internal static wimlib_image_name_in_use IsImageNameInUse;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int wimlib_resolve_image(
                IntPtr wim,
                [MarshalAs(StrType)] string image_name_or_num);
            internal static wimlib_resolve_image ResolveImage;
            #endregion
            
            #region Iterate - IterateDirTree
            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            internal delegate ErrorCode wimlib_iterate_dir_tree(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string path,
                IterateFlags flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeIterateDirTreeCallback cb,
                IntPtr user_ctx);
            internal static wimlib_iterate_dir_tree IterateDirTree;
            #endregion
            
            #region Join - Join, JoinWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags);
            internal static wimlib_join Join;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_join_with_progress(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] swms,
                uint num_swms,
                [MarshalAs(StrType)] string output_path,
                OpenFlags swms_open_flags,
                WriteFlags write_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal static wimlib_join_with_progress JoinWithProgress;
            #endregion
            
            #region Open - OpenWim, OpenWithProgress
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret);
            internal static wimlib_open_wim OpenWim;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_open_wim_with_progress(
                [MarshalAs(StrType)] string wim_file,
                OpenFlags open_flags,
                out IntPtr wim_ret,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal static wimlib_open_wim_with_progress OpenWimWithProgress;
            #endregion
            
            #region Mount - MountImage (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_mount_image(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string dir,
                MountFlags mount_flags,
                [MarshalAs(StrType)] string staging_dir);
            internal static wimlib_mount_image MountImage;
            #endregion
            
            #region Reference - ReferenceResourceFiles
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_reference_resource_files(
                IntPtr wim,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = StrType)] string[] resource_wimfiles_or_globs,
                uint count,
                RefFlags ref_flags,
                OpenFlags open_flags);
            internal static wimlib_reference_resource_files ReferenceResourceFiles;
            #endregion
            
            #region Rename - RenamePath
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_rename_path(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string source_path,
                [MarshalAs(StrType)] string dest_path);
            internal static wimlib_rename_path RenamePath;
            #endregion
            
            #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_description(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string description);
            internal static wimlib_set_image_description SetImageDescription;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_flags(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string flags);
            internal static wimlib_set_image_flags SetImageFlags;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_name(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string name);
            internal static wimlib_set_image_name SetImageName;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_set_image_property(
                IntPtr wim,
                int image,
                [MarshalAs(StrType)] string property_name,
                [MarshalAs(StrType)] string property_value);
            internal static wimlib_set_image_property SetImageProperty;
            #endregion
            
            #region Split - Split
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_split(
                IntPtr wim,
                [MarshalAs(StrType)] string swm_name,
                ulong part_size,
                WriteFlags write_flags);
            internal static wimlib_split Split;
            #endregion
            
            #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags);
            internal static wimlib_unmount_image UnmountImage;
            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_unmount_image_with_progress(
                [MarshalAs(StrType)] string dir,
                UnmountFlags unmount_flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progfunc,
                IntPtr progctx);
            internal static wimlib_unmount_image_with_progress UnmountImageWithProgress;
            #endregion
            
            #region Write - Write
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ErrorCode wimlib_write(
                IntPtr wim,
                [MarshalAs(StrType)] string path,
                int image,
                WriteFlags write_flags,
                uint num_threads);
            internal static wimlib_write Write;
            #endregion
            
            #region Struct CaptureSourceBase
            /// <summary>
            /// An array of these structures is passed to Wim.AddImageMultiSource() to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LLP64 platforms (Windows)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            public struct CaptureSourceBaseL32
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
            /// An array of these structures is passed to Wim.AddImageMultiSource() to specify the sources from which to create a WIM image. 
            /// </summary>
            /// <remarks>
            /// For LP64 platforms (64bit POSIX)
            /// </remarks>
            [StructLayout(LayoutKind.Sequential, CharSet = StructCharSet)]
            public struct CaptureSourceBaseL64
            {
                /// <summary>
                /// Absolute or relative path to a file or directory on the external filesystem to be included in the image.
                /// </summary>
                public string FsSourcePath;
                /// <summary>
                /// Destination path in the image.
                /// To specify the root directory of the image, use Wim.RootPath. 
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
        
        #region GlobalInit, GlobalCleanup
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_global_init(InitFlags initFlags);
        /// <summary>
        /// Initialization function for wimlib.
        /// Call before using any other wimlib function (except possibly Wim.SetPrintErrors()).
        /// If not done manually, this function will be called automatically with a flags argument of 0.
        /// This function does nothing if called again after it has already successfully run.
        /// </summary>
        internal static wimlib_global_init GlobalInit;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_global_cleanup();
        /// <summary>
        /// Cleanup function for wimlib.
        /// You are not required to call this function, but it will release any global resources allocated by the library.
        /// </summary>
        internal static wimlib_global_cleanup GlobalCleanup;
        #endregion

        #region Create - Callback
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate CallbackStatus NativeProgressFunc(
            ProgressMsg msgType,
            IntPtr info,
            IntPtr progctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_register_progress_function_delegate(
            IntPtr wim,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeProgressFunc progFunc,
            IntPtr progctx);
        internal static wimlib_register_progress_function_delegate RegisterProgressFunction;
        #endregion

        #region Error - GetErrorString, SetErrorFile, SetPrintErrors
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_error_string(ErrorCode code);
        internal static wimlib_get_error_string GetErrorString;

        internal static ErrorCode SetErrorFile(string path)
        {
            return UseUtf16 ? Utf16.SetErrorFile(path) : Utf8.SetErrorFile(path);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_print_errors(
            bool showMessages);
        internal static wimlib_set_print_errors SetPrintErrors;
        #endregion

        #region Add - AddEmptyImage, AddImage, AddImageMultiSource, AddTree
        internal static ErrorCode AddEmptyImage(
            IntPtr wim, 
            string name, 
            out int new_idx_ret)
        {
            return UseUtf16 
                ? Utf16.AddEmptyImage(wim, name, out new_idx_ret)
                : Utf8.AddEmptyImage(wim, name, out new_idx_ret);
        }
        
        internal static ErrorCode AddImage(
            IntPtr wim, 
            string source, 
            string name, 
            string config_file, 
            AddFlags add_flags)
        {
            return UseUtf16  
                ? Utf16.AddImage(wim, source, name, config_file, add_flags) 
                : Utf8.AddImage(wim, source, name, config_file, add_flags);
        }

        internal static ErrorCode AddImageMultiSource(
            IntPtr wim, 
            CaptureSource[] sources, 
            IntPtr num_sources, // size_t
            string name, 
            string config_file,
            AddFlags add_flags)
        {
            if (UseUtf16)
            {
                switch (LongBitType)
                {
                    case LongBits.Long32:
                        Utf16.CaptureSourceBaseL32[] capSrcsL32 = new Utf16.CaptureSourceBaseL32[sources.Length];
                        for (int i = 0; i < sources.Length; i++)
                        {
                            CaptureSource src = sources[i];
                            capSrcsL32[i] = new Utf16.CaptureSourceBaseL32
                            {
                                FsSourcePath = src.FsSourcePath,
                                WimTargetPath = src.WimTargetPath,
                            };
                        }
                        return Utf16.AddImageMultiSourceL32(wim, capSrcsL32, num_sources, name, config_file, add_flags);
                    case LongBits.Long64:
                        Utf16.CaptureSourceBaseL64[] capSrcsL64 = new Utf16.CaptureSourceBaseL64[sources.Length];
                        for (int i = 0; i < sources.Length; i++)
                        {
                            CaptureSource src = sources[i];
                            capSrcsL64[i] = new Utf16.CaptureSourceBaseL64
                            {
                                FsSourcePath = src.FsSourcePath,
                                WimTargetPath = src.WimTargetPath,
                            };
                        }
                        return Utf16.AddImageMultiSourceL64(wim, capSrcsL64, num_sources, name, config_file, add_flags);
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
            else
            {
                switch (LongBitType)
                {
                    case LongBits.Long32:
                        Utf8.CaptureSourceBaseL32[] capSrcsL32 = new Utf8.CaptureSourceBaseL32[sources.Length];
                        for (int i = 0; i < sources.Length; i++)
                        {
                            CaptureSource src = sources[i];
                            capSrcsL32[i] = new Utf8.CaptureSourceBaseL32
                            {
                                FsSourcePath = src.FsSourcePath,
                                WimTargetPath = src.WimTargetPath,
                            };
                        }
                        return Utf8.AddImageMultiSourceL32(wim, capSrcsL32, num_sources, name, config_file, add_flags);
                    case LongBits.Long64:
                        Utf8.CaptureSourceBaseL64[] capSrcsL64 = new Utf8.CaptureSourceBaseL64[sources.Length];
                        for (int i = 0; i < sources.Length; i++)
                        {
                            CaptureSource src = sources[i];
                            capSrcsL64[i] = new Utf8.CaptureSourceBaseL64
                            {
                                FsSourcePath = src.FsSourcePath,
                                WimTargetPath = src.WimTargetPath,
                            };
                        }
                        return Utf8.AddImageMultiSourceL64(wim, capSrcsL64, num_sources, name, config_file, add_flags);
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
        }

        internal static ErrorCode AddTree(
            IntPtr wim, 
            int image, 
            string fs_source_path, 
            string wim_target_path,
            AddFlags add_flags)
        {
            return UseUtf16
                ? Utf16.AddTree(wim, image, fs_source_path, wim_target_path, add_flags)
                : Utf8.AddTree(wim, image, fs_source_path, wim_target_path, add_flags);
        }
        #endregion

        #region Create - CreateWim
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_create_new_wim(
            CompressionType ctype,
            out IntPtr wim_ret);
        internal static wimlib_create_new_wim CreateNewWim;
        #endregion

        #region Delete - DeleteImage, DeletePath
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_delete_image(
            IntPtr wim,
            int image);
        internal static wimlib_delete_image DeleteImage;

        internal static ErrorCode DeletePath(
            IntPtr wim, 
            int image, 
            string path, 
            DeleteFlags delete_flags)
        {
            return UseUtf16 
                ? Utf16.DeletePath(wim, image, path, delete_flags)
                : Utf8.DeletePath(wim, image, path, delete_flags);
        }
        #endregion

        #region Export - ExportImage
        internal static ErrorCode ExportImage(
            IntPtr src_wim,
            int src_image,
            IntPtr dest_wim,
            string dest_name,
            string dest_description,
            ExportFlags export_flags)
        {
            return UseUtf16 
                ? Utf16.ExportImage(src_wim, src_image, dest_wim, dest_name, dest_description, export_flags)
                : Utf8.ExportImage(src_wim, src_image, dest_wim, dest_name, dest_description, export_flags);
        }
        #endregion
        
        #region Extract - ExtractImage, ExtractPaths, ExtractPathList
        internal static ErrorCode ExtractImage(
            IntPtr wim,
            int image,
            string target,
            ExtractFlags extract_flags)
        {
            return UseUtf16 
                ? Utf16.ExtractImage(wim, image, target, extract_flags)
                : Utf8.ExtractImage(wim, image, target, extract_flags);
        }

        internal static ErrorCode ExtractPathList(
            IntPtr wim,
            int image,
            string target,
            string path_list_file,
            ExtractFlags extract_flags)
        {
            return UseUtf16 
                ? Utf16.ExtractPathList(wim, image, target, path_list_file, extract_flags)
                : Utf8.ExtractPathList(wim, image, target, path_list_file, extract_flags);
        }

        internal static ErrorCode ExtractPaths(
            IntPtr wim,
            int image,
            string target,
            string[] paths,
            IntPtr num_paths, // size_t
            ExtractFlags extract_flags)
        {
            return UseUtf16 
                ? Utf16.ExtractPaths(wim, image, target, paths, num_paths, extract_flags)
                : Utf8.ExtractPaths(wim, image, target, paths, num_paths, extract_flags);
        }
        #endregion

        #region Free - Free
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void wimlib_free(IntPtr wim);
        internal static wimlib_free Free;
        #endregion

        #region GetImageInfo - GetImageDescription, GetImageName, GetImageProperty
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_image_description(
            IntPtr wim,
            int image);
        internal static wimlib_get_image_description GetImageDescription;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr wimlib_get_image_name(
            IntPtr wim,
            int image);
        internal static wimlib_get_image_name GetImageName;

        internal static IntPtr GetImageProperty(
            IntPtr wim,
            int image,
            string property_name)
        {
            return UseUtf16
                ? Utf16.GetImageProperty(wim, image, property_name)
                : Utf8.GetImageProperty(wim, image, property_name);
        }
        #endregion

        #region GetWimInfo - GetWimInfo, GetXmlData, IsImageNameInUse, ResolveImage
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_get_wim_info(
            IntPtr wim,
            IntPtr info);
        internal static wimlib_get_wim_info GetWimInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_get_xml_data(
            IntPtr wim,
            ref IntPtr buf_ret,
            ref IntPtr bufsize_ret); // size_t
        internal static wimlib_get_xml_data GetXmlData;

        internal static bool IsImageNameInUse(
            IntPtr wim,
            string name)
        {
            return UseUtf16
                ? Utf16.IsImageNameInUse(wim, name)
                : Utf8.IsImageNameInUse(wim, name);
        }

        internal static int ResolveImage(
            IntPtr wim,
            string image_name_or_num)
        {
            return UseUtf16
                ? Utf16.ResolveImage(wim, image_name_or_num)
                : Utf8.ResolveImage(wim, image_name_or_num);
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
        internal static wimlib_get_version GetVersionPtr;
        /// <summary>
        /// Return the version of wimlib as a Version instance.
        /// Major, Minor and Build (Patch) properties will be populated.
        /// </summary>
        public static Version GetVersion()
        {
            if (!Loaded)
                throw new InvalidOperationException(MsgInitFirstError);

            uint dword = GetVersionPtr();
            ushort major = (ushort)(dword >> 20);
            ushort minor = (ushort)((dword % (1 << 20)) >> 10);
            ushort patch = (ushort)(dword % (1 << 10));

            return new Version(major, minor, patch);
        }

        /// <summary>
        /// Return the version of wimlib as a Tuple.
        /// Tuple's items will be populated in a order of Major, Minor, and Patch.
        /// </summary>
        public static Tuple<ushort, ushort, ushort> GetVersionTuple()
        {
            if (!Loaded)
                throw new InvalidOperationException(MsgInitFirstError);

            uint dword = GetVersionPtr();
            ushort major = (ushort)(dword >> 20);
            ushort minor = (ushort)((dword % (1 << 20)) >> 10);
            ushort patch = (ushort)(dword % (1 << 10));

            return new Tuple<ushort, ushort, ushort>(major, minor, patch);
        }
        #endregion

        #region Iterate - IterateDirTree, IterateLookupTable
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate CallbackStatus NativeIterateDirTreeCallback(
            IntPtr dentry,
            IntPtr progctx);

        internal static ErrorCode IterateDirTree(
            IntPtr wim,
            int image,
            string path,
            IterateFlags flags,
            NativeIterateDirTreeCallback cb,
            IntPtr user_ctx)
        {
            return UseUtf16
                ? Utf16.IterateDirTree(wim, image, path, flags, cb, user_ctx)
                : Utf8.IterateDirTree(wim, image, path, flags, cb, user_ctx);   
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate CallbackStatus NativeIterateLookupTableCallback(
            ResourceEntry resoure,
            IntPtr progctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal delegate ErrorCode wimlib_iterate_lookup_table(
            IntPtr wim,
            int image,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeIterateLookupTableCallback cb,
            IntPtr user_ctx);
        internal static wimlib_iterate_lookup_table IterateLookupTable;
        #endregion

        #region Join - Join, JoinWithProgress
        internal static ErrorCode Join(
            string[] swms,
            uint num_swms,
            string output_path,
            OpenFlags swms_open_flags,
            WriteFlags write_flags)
        {
            return UseUtf16
                ? Utf16.Join(swms, num_swms, output_path, swms_open_flags, write_flags)
                : Utf8.Join(swms, num_swms, output_path, swms_open_flags, write_flags);   
        }

        internal static ErrorCode JoinWithProgress(
            string[] swms,
            uint num_swms,
            string output_path,
            OpenFlags swms_open_flags,
            WriteFlags write_flags,
            NativeProgressFunc progfunc,
            IntPtr progctx)
        {
            return UseUtf16
                ? Utf16.JoinWithProgress(swms, num_swms, output_path, swms_open_flags, write_flags, progfunc, progctx)
                : Utf8.JoinWithProgress(swms, num_swms, output_path, swms_open_flags, write_flags, progfunc, progctx);   
        }
        #endregion

        #region Open - OpenWim, OpenWithProgress
        internal static ErrorCode OpenWim(
            string wim_file,
            OpenFlags open_flags,
            out IntPtr wim_ret)
        {
            return UseUtf16
                ? Utf16.OpenWim(wim_file, open_flags, out wim_ret)
                : Utf8.OpenWim(wim_file, open_flags, out wim_ret);
        }

        internal static ErrorCode OpenWimWithProgress(
            string wim_file,
            OpenFlags open_flags,
            out IntPtr wim_ret,
            NativeProgressFunc progfunc,
            IntPtr progctx)
        {
            return UseUtf16
                ? Utf16.OpenWimWithProgress(wim_file, open_flags, out wim_ret, progfunc, progctx)
                : Utf8.OpenWimWithProgress(wim_file, open_flags, out wim_ret, progfunc, progctx);
        }
        #endregion
        
        #region Mount - MountImage (Linux Only)
        internal static ErrorCode MountImage(
            IntPtr wim,
            int image,
            string dir,
            MountFlags mount_flags,
            string staging_dir)
        {
            if (UseUtf16)
            {
                return Utf16.MountImage(wim, image, dir, mount_flags, staging_dir);
            }
            else
            {
                return Utf8.MountImage(wim, image, dir, mount_flags, staging_dir);
            }
            /*
            return UseUtf16
                ? Utf16.MountImage(wim, image, dir, mount_flags, staging_dir)
                : Utf8.MountImage(wim, image, dir, mount_flags, staging_dir);
                */
        }
        #endregion

        #region Reference - ReferenceResourceFiles, ReferenceResources, ReferenceTemplateImage
        internal static ErrorCode ReferenceResourceFiles(
            IntPtr wim,
            string[] resource_wimfiles_or_globs,
            uint count,
            RefFlags ref_flags,
            OpenFlags open_flags)
        {
            return UseUtf16
                ? Utf16.ReferenceResourceFiles(wim, resource_wimfiles_or_globs, count, ref_flags, open_flags)
                : Utf8.ReferenceResourceFiles(wim, resource_wimfiles_or_globs, count, ref_flags, open_flags);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_reference_resources(
            IntPtr wim,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] resource_wims,
            uint num_resource_wims,
            RefFlags ref_flags);
        internal static wimlib_reference_resources ReferenceResources;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_reference_template_image(
            IntPtr wim,
            int new_image,
            IntPtr template_wim,
            int template_image,
            int flags);
        internal static wimlib_reference_template_image ReferenceTemplateImage;
        #endregion

        #region Rename - RenamePath
        internal static ErrorCode RenamePath(
            IntPtr wim,
            int image,
            string source_path,
            string dest_path)
        {
            return UseUtf16
                ? Utf16.RenamePath(wim, image, source_path, dest_path)
                : Utf8.RenamePath(wim, image, source_path, dest_path);
        }
        #endregion

        #region SetImageInfo - SetImageDescription, SetImageFlags, SetImageName, SetImageProperty, SetWimInfo
        internal static ErrorCode SetImageDescription(
            IntPtr wim,
            int image,
            string description)
        {
            return UseUtf16
                ? Utf16.SetImageDescription(wim, image, description)
                : Utf8.SetImageDescription(wim, image, description);
        }

        internal static ErrorCode SetImageFlags(
            IntPtr wim,
            int image,
            string flags)
        {
            return UseUtf16
                ? Utf16.SetImageFlags(wim, image, flags)
                : Utf8.SetImageFlags(wim, image, flags);
        }

        internal static ErrorCode SetImageName(
            IntPtr wim,
            int image,
            string name)
        {
            return UseUtf16
                ? Utf16.SetImageName(wim, image, name)
                : Utf8.SetImageName(wim, image, name);
        }

        internal static ErrorCode SetImageProperty(
            IntPtr wim,
            int image,
            string property_name,
            string property_value)
        {
            return UseUtf16
                ? Utf16.SetImageProperty(wim, image, property_name, property_value)
                : Utf8.SetImageProperty(wim, image, property_name, property_value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_wim_info(
            IntPtr wim,
            ref WimInfo info,
            ChangeFlags which);
        internal static wimlib_set_wim_info SetWimInfo;
        #endregion

        #region SetOutput - SetOutputChunkSize, SetOutputPackChunkSize, SetOutputCompressionType, SetOutputPackCompressionType
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_chunk_size(
            IntPtr wim,
            uint chunk_size);
        internal static wimlib_set_output_chunk_size SetOutputChunkSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_pack_chunk_size(
            IntPtr wim,
            uint chunk_size);
        internal static wimlib_set_output_pack_chunk_size SetOutputPackChunkSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_compression_type(
            IntPtr wim,
            CompressionType ctype);
        internal static wimlib_set_output_compression_type SetOutputCompressionType;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_set_output_pack_compression_type(
            IntPtr wim,
            CompressionType ctype);
        internal static wimlib_set_output_pack_compression_type SetOutputPackCompressionType;
        #endregion

        #region Split - Split
        internal static ErrorCode Split(
            IntPtr wim,
            string swm_name,
            ulong part_size,
            WriteFlags write_flags)
        {
            return UseUtf16
                ? Utf16.Split(wim, swm_name, part_size, write_flags)
                : Utf8.Split(wim, swm_name, part_size, write_flags);
        }
        #endregion

        #region Verify - VerifyWim
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_verify_wim(
            IntPtr wim,
            int verify_flags);
        internal static wimlib_verify_wim VerifyWim;
        #endregion

        #region Unmount - UnmountImage, UnmountImageWithProgress (Linux Only)
        internal static ErrorCode UnmountImage(
            string dir,
            UnmountFlags unmount_flags)
        {
            return UseUtf16
                ? Utf16.UnmountImage(dir, unmount_flags)
                : Utf8.UnmountImage(dir, unmount_flags);
        }
        
        internal static ErrorCode UnmountImageWithProgress(
            string dir,
            UnmountFlags unmount_flags,
            NativeProgressFunc progfunc,
            IntPtr progctx)
        {
            return UseUtf16
                ? Utf16.UnmountImageWithProgress(dir, unmount_flags, progfunc, progctx)
                : Utf8.UnmountImageWithProgress(dir, unmount_flags, progfunc, progctx);
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
        internal static wimlib_update_image_32 UpdateImage32;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_update_image_64(
            IntPtr wim,
            int image,
            [MarshalAs(UnmanagedType.LPArray)] UpdateCommand64[] cmds,
            ulong num_cmds,
            UpdateFlags update_flags);
        internal static wimlib_update_image_64 UpdateImage64;
        #endregion

        #region Write - Write, Overwrite
        internal static ErrorCode Write(
            IntPtr wim,
            string path,
            int image,
            WriteFlags write_flags,
            uint num_threads)
        {
            return UseUtf16
                ? Utf16.Write(wim, path, image, write_flags, num_threads)
                : Utf8.Write(wim, path, image, write_flags, num_threads);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ErrorCode wimlib_overwrite(
            IntPtr wim,
            WriteFlags write_flags,
            uint numThreads);
        internal static wimlib_overwrite Overwrite;
        #endregion
        #endregion

        #region Utility
        internal static void SetBitField(ref uint bitField, int bitShift, bool value)
        {
            if (value)
                bitField = bitField | (uint)(1 << bitShift);
            else
                bitField = bitField & ~(uint)(1 << bitShift);
        }

        internal static bool GetBitField(uint bitField, int bitShift)
        {
            return (bitField & (1 << bitShift)) != 0;
        }
        #endregion
    }
    #endregion
}
