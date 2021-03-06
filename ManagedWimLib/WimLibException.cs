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

using System;
using System.Runtime.Serialization;
using System.Text;

namespace ManagedWimLib
{
    #region WimException
    [Serializable]
    public class WimLibException : Exception
    {
        public ErrorCode ErrorCode;

        #region Constructor
        public WimLibException(ErrorCode errorCode)
            : base(ForgeErrorMessages(errorCode, true))
        {
            ErrorCode = errorCode;
        }

        public WimLibException()
        {
            ErrorCode = ErrorCode.Success;
        }

        public WimLibException(string message)
            : base(message)
        {
            ErrorCode = ErrorCode.Success;
        }

        public WimLibException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = ErrorCode.Success;
        }
        #endregion

        internal static string ForgeErrorMessages(ErrorCode errorCode, bool full)
        {
            StringBuilder b = new StringBuilder();

            if (full)
                b.Append($"[{errorCode}] ");

            b.Append(Wim.GetLastError() ?? Wim.GetErrorString(errorCode));

            return b.ToString();
        }

        internal static void CheckErrorCode(ErrorCode ret)
        {
            if (ret != ErrorCode.Success)
                throw new WimLibException(ret);
        }

        #region Serializable
        protected WimLibException(SerializationInfo info, StreamingContext ctx)
        {
            ErrorCode = (ErrorCode)info.GetValue(nameof(ErrorCode), typeof(ErrorCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.AddValue(nameof(ErrorCode), ErrorCode);
            base.GetObjectData(info, context);
        }
        #endregion
    }
    #endregion
}
