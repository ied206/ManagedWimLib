/*
    Licensed under LGPLv3

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

namespace ManagedWimLib
{
    internal class WimLibLoadManager : LoadManagerBase<WimLibLoader>
    {
        protected override string ErrorMsgInitFirst => "Please call Wim.GlobalInit() first!";
        protected override string ErrorMsgAlreadyLoaded => "ManagedWimLib is already initialized.";

        protected override WimLibLoader CreateLoader() => new WimLibLoader();
    }
}
