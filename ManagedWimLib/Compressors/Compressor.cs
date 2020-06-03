/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2020 Hajin Jang

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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ManagedWimLib.Compressors
{
    public class Compressor : IDisposable
    {
        #region (static) LoadManager
        private static WimLibLoadManager Manager => Wim.Manager;
        private static WimLibLoader Lib => Wim.Manager.Lib;
        #endregion

        #region Fields
        private IntPtr _ptr;
        #endregion

        #region Constructor (private)
        private Compressor(IntPtr ptr)
        {
            Manager.EnsureLoaded();

            _ptr = ptr;
        }
        #endregion

        #region Disposable Pattern
        ~Compressor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_ptr == IntPtr.Zero)
                return;

            Lib.FreeCompressor(_ptr);
            _ptr = IntPtr.Zero;
        }
        #endregion

        #region Create - (Static) CreateCompressor
        /// <summary>
        /// Allocate a compressor for the specified compression type using the specified parameters.
        /// This function is part of wimlib's compression API; it is not necessary to call this to process a WIM file.
        /// </summary>
        /// <param name="ctype">
        /// Compression type for which to create the compressor, as one of the <see cref="CompressionType"/> constants.
        /// </param>
        /// <param name="maxBlockSize">
        /// The maximum compression block size to support. 
        /// <para>This specifies the maximum allowed value for the uncompressedSize parameter of <see cref="Compressor.Compress()"/> when called using this compressor.</para>
        /// <para>Usually, the amount of memory used by the compressor will scale in proportion to the <paramref name="maxBlockSize"/> parameter.
        /// <see cref="Wim.GetCompressorNeededMemory"/> can be used to query the specific amount of memory that will be required.</para>
        /// <para>This parameter must be at least 1 and must be less than or equal to a compression-type-specific limit.</para>
        /// </param>
        /// <param name="compressionLevel">
        /// The compression level to use.
        /// <para>If 0, the default compression level (50, or another value as set through
        /// <see cref="Wim.SetDefaultCompressionLevel(CompressionType, uint, CompressorFlags)"/>) is used.
        /// Otherwise, a higher value indicates higher compression.<br/>
        /// The values are scaled so that 10 is low compression, 50 is medium compression, and 100 is high compression.
        /// This is not a percentage; values above 100 are also valid.</para>
        /// </param>
        /// <param name="compressorFlags">
        /// Flag <see cref="CompressorFlags.Destructive"/> creates the compressor in a mode where it is allowed to modify the input buffer.
        /// <para>Specifically, in this mode, if compression succeeds, the input buffer may have been modified,
        /// whereas if compression does not succeed the input buffer still may have been written to but will have been restored exactly to its original state.</para>
        /// <para>This mode is designed to save some memory when using large buffer sizes.</para>
        /// </param>
        /// <returns>
        /// On success, a new instance of the allocated <see cref="Compressor"/>, which can be used for any number of calls to <see cref="Compressor.Compress()"/>.
        ///	This instance must be disposed manually.
        /// </returns>
        /// <exception cref="WimLibException">wimlib did not return <see cref="ErrorCode.Success"/>.</exception>
        public static Compressor Create(CompressionType ctype, int maxBlockSize, uint compressionLevel, CompressorFlags compressorFlags)
        {
            Manager.EnsureLoaded();

            if (maxBlockSize < 0)
                throw new ArgumentOutOfRangeException(nameof(maxBlockSize));

            compressionLevel |= (uint)compressorFlags;
            ErrorCode ret = Lib.CreateCompressor(ctype, new UIntPtr((uint)maxBlockSize), compressionLevel, out IntPtr compPtr);
            WimLibException.CheckErrorCode(ret);

            return new Compressor(compPtr);
        }
        #endregion

        #region Compress (Safe)
        /// <summary>
        /// Compress a buffer of data.
        /// </summary>
        /// <param name="uncompressedSpan">
        /// Buffer containing the data to compress.
        /// <para>The length of the span cannot be greater than the maxBlockSize with which <see cref="CreateCompressor()"/> was called.
        /// (If it is, the data will not be compressed and 0 will be returned.)</para>
        /// </param>
        /// <param name="compressedSpan">
        /// Buffer into which to write the compressed data.
        /// <para>The length of the span cannot exceed the maxBlockSize with which <see cref="CreateCompressor()"/> was called.
        /// (If it does, the data will not be decompressed and a nonzero value will be returned.)</para>
        /// </param>
        /// <returns>
        /// The size of the compressed data, in bytes, or 0 if the data could not be compressed to <paramref name="compressedSpan"/> or fewer bytes.
        /// </returns>
        /// <exception cref="OverflowException">Used a size greater than uint.MaxValue in 32bit platform.</exception>
        public unsafe int Compress(ReadOnlySpan<byte> uncompressedSpan, Span<byte> compressedSpan)
        {
            UIntPtr compressedBytes;
            fixed (byte* uncompressedBuf = uncompressedSpan)
            fixed (byte* compressedBuf = compressedSpan)
            {
                UIntPtr uncompressedSize = new UIntPtr((uint)uncompressedSpan.Length);
                UIntPtr compressedSizeAvail = new UIntPtr((uint)compressedSpan.Length);
                compressedBytes = Lib.Compress(uncompressedBuf, uncompressedSize, compressedBuf, compressedSizeAvail, _ptr);
            }

            // Since compressedSizeAvail is int, the returned value cannot be larger than int.MaxValue.
            ulong ret = compressedBytes.ToUInt64();
            return (int)ret;
        }

        /// <summary>
        /// Compress a buffer of data.
        /// </summary>
        /// <param name="uncompressedData">
        /// Buffer containing the data to compress.
        /// </param>
        /// <param name="uncompressedOffset">
        /// The offset of the data in buffer containing the data to compress.
        /// </param>
        /// <param name="uncompressedSize">
        /// Size, in bytes, of the data to compress.
        /// <para>This cannot be greater than the maxBlockSize with which <see cref="CreateCompressor()"/> was called.<br/>
        /// (If it is, the data will not be compressed and 0 will be returned.)</para>
        /// </param>
        /// <param name="compressedData">
        /// Buffer into which to write the compressed data.
        /// </param>
        /// <param name="compressedOffset">
        /// The offset of the data in buffer into which to write the compressed data.
        /// </param>
        /// <param name="compressedSizeAvail">
        /// Number of bytes available in <paramref name="compressedData"/>.
        /// </param>
        /// <returns>
        /// The size of the compressed data, in bytes, or 0 if the data could not be compressed to <paramref name="uncompressedSize"/> or fewer bytes.
        /// </returns>
        public unsafe int Compress(byte[] uncompressedData, int uncompressedOffset, int uncompressedSize,
            byte[] compressedData, int compressedOffset, int compressedSizeAvail)
        {
            CheckReadWriteArgs(uncompressedData, uncompressedOffset, uncompressedSize);
            CheckReadWriteArgs(compressedData, compressedOffset, compressedSizeAvail);

            ulong compressedSize;
            fixed (byte* uncompressedBuf = uncompressedData.AsSpan(uncompressedOffset, uncompressedSize))
            fixed (byte* compressedBuf = compressedData.AsSpan(compressedOffset, compressedSizeAvail))
            {
                compressedSize = Compress(uncompressedBuf, (ulong)uncompressedSize, compressedBuf, (ulong)compressedSizeAvail);
            }

            // Since compressedSizeAvail is int, the returned value cannot be larger than int.MaxValue.
            Debug.Assert(compressedSize <= int.MaxValue);
            return (int)compressedSize;
        }
        #endregion

        #region Compress (Unsafe)
        /// <summary>
        /// Compress a buffer of data.
        /// </summary>
        /// <param name="uncompressedBuf">
        /// Buffer containing the data to compress.
        /// </param>
        /// <param name="uncompressedSize">
        /// Size, in bytes, of the data to compress.
        /// <para>This cannot be greater than the maxBlockSize with which <see cref="CreateCompressor()"/> was called.<br/>
        /// (If it is, the data will not be compressed and 0 will be returned.)</para>
        /// </param>
        /// <param name="compressedBuf">
        /// Buffer into which to write the compressed data.
        /// </param>
        /// <param name="compressedSizeAvail">
        /// Number of bytes available in <paramref name="compressedBuf"/>.
        /// </param>
        /// <returns>
        /// The size of the compressed data, in bytes, or 0 if the data could not be compressed to <paramref name="uncompressedSize"/> or fewer bytes.
        /// </returns>
        /// <exception cref="OverflowException">Used a size greater than uint.MaxValue in 32bit platform.</exception>
        public unsafe ulong Compress(byte* uncompressedBuf, ulong uncompressedSize, byte* compressedBuf, ulong compressedSizeAvail)
        {
            UIntPtr uncompressedSizeInterop = new UIntPtr(uncompressedSize);
            UIntPtr compressedSizeAvailInterop = new UIntPtr(compressedSizeAvail);
            UIntPtr compressedBytes = Lib.Compress(uncompressedBuf, uncompressedSizeInterop, compressedBuf, compressedSizeAvailInterop, _ptr);

            ulong ret = compressedBytes.ToUInt64();
            return ret;
        }
        #endregion

        #region (Utility) CheckReadWriteArgs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckReadWriteArgs(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count)
                throw new ArgumentOutOfRangeException(nameof(count));
        }
        #endregion
    }
}
