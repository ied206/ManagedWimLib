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
    public class Decompressor : IDisposable
    {
        #region (static) LoadManager
        private static WimLibLoadManager Manager => Wim.Manager;
        private static WimLibLoader Lib => Wim.Manager.Lib;
        #endregion

        #region Fields
        private IntPtr _ptr;
        #endregion

        #region Constructor (private)
        private Decompressor(IntPtr ptr)
        {
            Manager.EnsureLoaded();

            _ptr = ptr;
        }
        #endregion

        #region Disposable Pattern
        ~Decompressor()
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

            Lib.FreeDecompressor(_ptr);
            _ptr = IntPtr.Zero;
        }
        #endregion

        #region Create - (Static) CreateDecompressor
        /// <summary>
        /// Allocate a decompressor for the specified compression type.
        /// This function is part of wimlib's compression API; it is not necessary to call this to process a WIM file.
        /// </summary>
        /// <param name="ctype">
        /// Compression type for which to create the decompressor, as one of the <see cref="CompressionType"/> constants.
        /// </param>
        /// <param name="maxBlockSize">
        /// The maximum compression block size to support. 
        /// <para>This specifies the maximum allowed value for the uncompressedSize parameter of <see cref="Decompress()"/> when called using this decompressor.</para>
        /// <para>In general, this parameter must be the same as the maxBlockSize that was passed to <see cref="Compressor.CreateCompressor()"/> when the data was compressed.
        /// However, some compression types have looser requirements regarding this.</para>
        /// </param>
        /// <returns>
        /// On success, a new instance of the allocated <see cref="Decompressor"/>, which can be used for any number of calls to <see cref="Decompressor.Decompress()"/>.
        ///	This instance must be disposed manually.
        /// </returns>
        /// <exception cref="WimException">wimlib did not return <see cref="ErrorCode.Success"/>.</exception>
        public static Decompressor CreateDecompressor(CompressionType ctype, int maxBlockSize)
        {
            Manager.EnsureLoaded();

            if (maxBlockSize < 0)
                throw new ArgumentOutOfRangeException(nameof(maxBlockSize));

            ErrorCode ret = Lib.CreateDecompressor(ctype, new UIntPtr((uint)maxBlockSize), out IntPtr decompPtr);
            WimException.CheckErrorCode(ret);

            return new Decompressor(decompPtr);
        }
        #endregion

        #region Decompress (Safe)
        /// <summary>
        /// Decompress a buffer of data.
        /// <para>This function requires that the exact uncompressed size of the data be passed as the <paramref name="exactUncompressedSize"/> parameter.<br/>
        /// If this is not done correctly, decompression may fail or the data may be decompressed incorrectly.</para>
        /// </summary>
        /// <param name="compressedSpan">
        /// Buffer containing the data to decompress.
        /// </param>
        /// <param name="uncompressedSpan">
        /// Buffer into which to write the uncompressed data.
        /// </param>
        /// <param name="exactUncompressedSize">
        /// Size, in bytes, of the data when uncompressed.
        /// <para>This cannot exceed the maxBlockSize with which <see cref="CreateDecompressor(CompressionType, int)"/> was called.<br/>
        /// (If it does, the data will not be decompressed and a nonzero value will be returned.)</para>
        /// </param>
        /// <returns>
        /// Return true on success, false on failure.
        /// </returns>
        /// <exception cref="PlatformNotSupportedException">Used a size greater than uint.MaxValue in 32bit platform.</exception>
        public unsafe bool Decompress(ReadOnlySpan<byte> compressedSpan, Span<byte> uncompressedSpan, int exactUncompressedSize)
        {
            if (exactUncompressedSize < 0)
                throw new ArgumentOutOfRangeException(nameof(exactUncompressedSize));
            if (exactUncompressedSize < uncompressedSpan.Length)
                throw new ArgumentOutOfRangeException($"{nameof(uncompressedSpan)} must be equal to or larger than {nameof(exactUncompressedSize)}.");

            fixed (byte* compressedBuf = compressedSpan)
            fixed (byte* uncompressedBuf = uncompressedSpan)
            {
                return Decompress(compressedBuf, (ulong)compressedSpan.Length, uncompressedBuf, (ulong)exactUncompressedSize);
            }
        }

        /// <summary>
        /// Decompress a buffer of data.
        /// <para>This function requires that the exact uncompressed size of the data be passed as the <paramref name="exactUncompressedSize"/> parameter.<br/>
        /// If this is not done correctly, decompression may fail or the data may be decompressed incorrectly.</para>
        /// </summary>
        /// <param name="compressedData">
        /// Buffer containing the data to decompress.
        /// </param>
        /// <param name="compressedOffset">
        /// The offset of the data to decompress.
        /// </param>
        /// <param name="compressedSize">
        /// Size, in bytes, of the data to decompress.
        /// </param>
        /// <param name="uncompressedData">
        /// Buffer into which to write the uncompressed data.
        /// </param>
        /// <param name="uncompressedOffset">
        /// The offset of the buffer into which to write the uncompressed data.
        /// </param>
        /// <param name="exactUncompressedSize">
        /// Size, in bytes, of the data when uncompressed.
        /// <para>This cannot exceed the maxBlockSize with which <see cref="CreateDecompressor(CompressionType, int)"/> was called.<br/>
        /// (If it does, the data will not be decompressed and a nonzero value will be returned.)</para>
        /// </param>
        /// <returns></returns>
        public unsafe bool Decompress(byte[] compressedData, int compressedOffset, int compressedSize,
            byte[] uncompressedData, int uncompressedOffset, int exactUncompressedSize)
        {
            CheckReadWriteArgs(compressedData, compressedOffset, compressedSize);
            CheckReadWriteArgs(uncompressedData, uncompressedOffset, exactUncompressedSize);

            fixed (byte* compressedBuf = compressedData.AsSpan(compressedOffset, compressedSize))
            fixed (byte* uncompressedBuf = uncompressedData.AsSpan(uncompressedOffset, exactUncompressedSize))
            {
                return Decompress(compressedBuf, (ulong)compressedSize, uncompressedBuf, (ulong)exactUncompressedSize);
            }
        }
        #endregion

        #region Decompress (Unsafe)
        /// <summary>
        /// Decompress a buffer of data.
        /// <para>This function requires that the exact uncompressed size of the data be passed as the <paramref name="exactUncompressedSize"/> parameter.<br/>
        /// If this is not done correctly, decompression may fail or the data may be decompressed incorrectly.</para>
        /// </summary>
        /// <param name="compressedBuf">
        /// Buffer containing the data to decompress.
        /// </param>
        /// <param name="compressedSize">
        /// Size, in bytes, of the data to decompress.
        /// </param>
        /// <param name="uncompressedBuf">
        /// Buffer into which to write the uncompressed data.
        /// </param>
        /// <param name="exactUncompressedSize">
        /// Size, in bytes, of the data when uncompressed.
        /// <para>This cannot exceed the maxBlockSize with which <see cref="CreateDecompressor(CompressionType, int)"/> was called.<br/>
        /// (If it does, the data will not be decompressed and a nonzero value will be returned.)</para>
        /// </param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException">Used a size greater than uint.MaxValue in 32bit platform.</exception>
        public unsafe bool Decompress(byte* compressedBuf, ulong compressedSize, byte* uncompressedBuf, ulong exactUncompressedSize)
        {
            UIntPtr compressedSizeInterop = Lib.ToSizeT(compressedSize);
            UIntPtr uncompressedSizeInterop = Lib.ToSizeT(exactUncompressedSize);

            // 0 on success, Non-0 on failure.
            int ret = Lib.Decompress(compressedBuf, compressedSizeInterop, uncompressedBuf, uncompressedSizeInterop, _ptr);
            return ret == 0;
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
