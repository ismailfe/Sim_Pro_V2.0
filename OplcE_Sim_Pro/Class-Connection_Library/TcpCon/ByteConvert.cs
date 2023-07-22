/*********************************************************************
 * NetToPLCsim, Netzwerkanbindung fuer PLCSIM
 * 
 * Copyright (C) 2011-2016 Thomas Wiens, th.wiens@gmx.de
 *
 * This file is part of NetToPLCsim.
 *
 * NetToPLCsim is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 /*********************************************************************/

using System;
using System.Net;

namespace TCPCon
{
    class ByteConvert
    {
        public static UInt16 DoReverseEndian(UInt16 x)
        {
            return Convert.ToUInt16((x << 8 & 0xff00) | (x >> 8));
        }

        public static uint DoReverseEndian(uint x)
        {
            return (x << 24 | (x & 0xff00) << 8 | (x & 0xff0000) >> 8 | x >> 24);
        }

        public static ulong DoReverseEndian(ulong x)
        {
            return (x << 56 | (x & 0xff00) << 40 | (x & 0xff0000) << 24 | (x & 0xff000000) << 8 | (x & 0xff00000000) >> 8 | (x & 0xff0000000000) >> 24 | (x & 0xff000000000000) >> 40 | x >> 56);
        }

        public static Int16 GetInt16at(byte[] value, int startindex)
        {
            if (BitConverter.IsLittleEndian)
                return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(value, startindex));
            return BitConverter.ToInt16(value, startindex);
        }
    }
}
