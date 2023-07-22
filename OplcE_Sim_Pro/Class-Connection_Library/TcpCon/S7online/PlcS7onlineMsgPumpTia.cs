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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;

namespace PlcsimS7online
{
    public class PlcS7onlineMsgPumpTia : PlcS7onlineMsgPump
    {
        const int SIZE_OF_USER_DATA_1 = 1024;
        const int SEG_LENGTH_1 = 1024;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct UserDataConnectionConfigTia
        {
            public byte routing_enabled;
            public byte b01;
            public byte b02;
            public byte b03;
            public byte b04;
            public byte b05;
            public byte b06;
            public byte b07;
            public byte b08;
            public byte destination_1;
            public byte destination_2;
            public byte destination_3;
            public byte destination_4;
            public byte b13;
            public byte b14;
            public byte b15;
            public byte b16;
            public byte accessname_length;
            public byte accessname01;
            public byte accessname02;
            public byte accessname03;
            public byte accessname04;
            public byte accessname05;
            public byte accessname06;
            public byte accessname07;
            public byte accessname08;
            public byte accessname09;
            public byte accessname10;
            public byte accessname11;
            public byte accessname12;
            public byte accessname13;
            public byte accessname14;
            public byte accessname15;
            public byte accessname16;

            public UserDataConnectionConfigTia(bool useConstructor)
                : this()
            {
                b01 = 0x02;
                b02 = 0x06;
                b03 = 0x20;
                b04 = 0x0C;
                b05 = 0x01;
                b15 = 0x01;
                // theoretical this length is variable
                accessname_length = 16;
                accessname01 = Convert.ToByte('S');
                accessname02 = Convert.ToByte('I');
                accessname03 = Convert.ToByte('M');
                accessname04 = Convert.ToByte('A');
                accessname05 = Convert.ToByte('T');
                accessname06 = Convert.ToByte('I');
                accessname07 = Convert.ToByte('C');
                accessname08 = Convert.ToByte('-');
                accessname09 = Convert.ToByte('R');
                accessname10 = Convert.ToByte('O');
                accessname11 = Convert.ToByte('O');
                accessname12 = Convert.ToByte('T');
                accessname13 = Convert.ToByte('-');
                accessname14 = Convert.ToByte('H');
                accessname15 = Convert.ToByte('M');
                accessname16 = Convert.ToByte('I');
            }
        }

        public PlcS7onlineMsgPumpTia(IPAddress plc_ipaddress, int rack, int slot)
            : base(plc_ipaddress, rack, slot)
        {

        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_M_CONNECTPLCSIM:
                    ConnectPlcsimStatemachine();
                    break;
                case WM_M_SENDDATA:
                    if (m.LParam != IntPtr.Zero)
                    {
                        if (m_PlcsimConnectionState == PlcsimConnectionState.Connected)
                        {
                            WndProcMessage msg = GetReceivedMessage(ref m);
                            SendDataToPlcsim(msg);
                        }
                    }
                    break;
                case WM_M_EXIT:
                    if (m_PlcsimConnectionState == PlcsimConnectionState.Connected)
                    {
                        m_PlcsimConnectionState = PlcsimConnectionState.DisconnectState1;
                        DisconnectPlcsimStatemachine();
                    }
                    break;
                case WM_SINEC:
                    switch (m_PlcsimConnectionState)
                    {
                        case PlcsimConnectionState.NotConnected:
                        case PlcsimConnectionState.ConnectState1:
                        case PlcsimConnectionState.ConnectState2:
                        case PlcsimConnectionState.ConnectState3:
                            ConnectPlcsimStatemachine();
                            break;
                        case PlcsimConnectionState.Connected:
                            ReceiveFromPlcsimAndSendBack();
                            break;
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void ReceiveFromPlcsimAndSendBack()
        {
            int[] rec_len = new int[1];
            byte[] buffer = new byte[m_FdrLen];

            if (SCP_receive(m_connectionHandle, 0, rec_len, (ushort)m_FdrLen, buffer) == -1)
            {
                if (m_connectionHandle > 0)
                {
                    if (m_PlcsimConnectionState == PlcsimConnectionState.ConnectState1 ||
                        m_PlcsimConnectionState == PlcsimConnectionState.ConnectState2  ||
                        m_PlcsimConnectionState == PlcsimConnectionState.ConnectState3)
                    {
                        SendConnectErrorMessage("ERROR: Connect failed in SCP_receive at " + m_PlcsimConnectionState.ToString());
                    }
                    else
                    {
                        int errno = SCP_get_errno();
                        string errtxt = GetErrTxt(errno);
                        SendErrorMessage("ERROR: SCP_receive() failed. m_connectionHandle = " + m_connectionHandle + " SCP_get_errno=" + errno + " (" + errtxt +")");
                    }
                }
                ExitThisThread();
                return;
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            S7OexchangeBlock rec = (S7OexchangeBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(S7OexchangeBlock));
            handle.Free();

            if (rec.opcode == 6 && rec.response == 1)
            {
                // don't care
            }
            else if (rec.opcode == 7 && rec.response == 3)
            {
                byte[] recbytes = new byte[rec.fill_length_1];
                Array.Copy(rec.user_data_1, recbytes, rec.fill_length_1);
                SendPduMessage(recbytes);

                S7OexchangeBlock snd = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 7, response: 0xffff);
                snd.offset_1 = 80;
                snd.fill_length_1 = 0;
                snd.seg_length_1 = SEG_LENGTH_1;
                snd.application_block_opcode = rec.application_block_opcode;
                snd.application_block_subsystem = rec.application_block_subsystem;

                SendS7OExchangeBlockToPlcsim(snd);
            }
        }

        private void SendDataToPlcsim(WndProcMessage msg)
        {
            if (++m_user >= 32000) m_user = 1;
            S7OexchangeBlock snd = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 6, response: 0xffff);
            snd.offset_1 = 80;

            snd.application_block_opcode = m_application_block_opcode;
            snd.application_block_subsystem = m_application_block_subsystem;

            snd.fill_length_1 = (ushort)msg.pdu.Length;
            snd.seg_length_1 = SEG_LENGTH_1;

            snd.user_data_1 = new byte[SIZE_OF_USER_DATA_1];
            Array.Copy(msg.pdu, snd.user_data_1, msg.pdu.Length);

            SendS7OExchangeBlockToPlcsim(snd);
        }

        private void ConnectPlcsimStatemachine()
        {
            S7OexchangeBlock rec;
            S7OexchangeBlock fdr;
            IntPtr ptr;

            switch (m_PlcsimConnectionState)
            {
                case PlcsimConnectionState.NotConnected:
                    m_connectionHandle = SCP_open("S7ONLINE");
                    if (m_connectionHandle < 0)
                    {
                        SendConnectErrorMessage("ERROR: SCP_open failed");
                        m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;
                        ExitThisThread();
                    }
                    SetSinecHWndMsg(m_connectionHandle, this.Handle, WM_SINEC);

                    fdr = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 0, response: 0xffff);
                    fdr.offset_1 = 80;

                    fdr.application_block_opcode = 0xff;
                    fdr.application_block_subsystem = 0xff;

                    SendS7OExchangeBlockToPlcsim(fdr);

                    m_PlcsimConnectionState = PlcsimConnectionState.ConnectState1;
                    break;

                case PlcsimConnectionState.ConnectState1:
                    rec = ReceiveFromPlcsimDirect();

                    if (!(rec.opcode == 0x00 && rec.response == 0x01))
                    {
                        SendConnectErrorMessage("ERROR: Connect failed at ConnectState1");
                        m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;
                        ExitThisThread();
                    }
                    // save connection parameters for usage in all following telegrams
                    m_application_block_opcode = rec.application_block_opcode;
                    m_application_block_subsystem = rec.application_block_subsystem;

                    fdr = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 1, response: 0xffff);
                    fdr.fill_length_1 = 34;
                    fdr.seg_length_1 = 34;
                    fdr.offset_1 = 80;
                    fdr.application_block_opcode = m_application_block_opcode;
                    fdr.application_block_subsystem = m_application_block_subsystem;

                    UserDataConnectionConfigTia ud_cfg = new UserDataConnectionConfigTia(true);
                    ud_cfg.destination_1 = m_PlcIPAddress.GetAddressBytes()[0];
                    ud_cfg.destination_2 = m_PlcIPAddress.GetAddressBytes()[1];
                    ud_cfg.destination_3 = m_PlcIPAddress.GetAddressBytes()[2];
                    ud_cfg.destination_4 = m_PlcIPAddress.GetAddressBytes()[3];

                    ptr = Marshal.AllocHGlobal(Marshal.SizeOf(ud_cfg));
                    Marshal.StructureToPtr(ud_cfg, ptr, false);
                    fdr.user_data_1 = new byte[SIZE_OF_USER_DATA_1];
                    Marshal.Copy(ptr, fdr.user_data_1, 0, Marshal.SizeOf(ud_cfg));
                    Marshal.FreeHGlobal(ptr);

                    SendS7OExchangeBlockToPlcsim(fdr);

                    m_PlcsimConnectionState = PlcsimConnectionState.ConnectState2;
                    break;

                case PlcsimConnectionState.ConnectState2:
                    rec = ReceiveFromPlcsimDirect();

                    if (!(rec.opcode == 0x01 && rec.response == 0x01))
                    {
                        SendConnectErrorMessage("ERROR: Connect failed at ConnectState2");
                        m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;
                        ExitThisThread();
                    }
                    else
                    {
                        fdr = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 13, response: 0xffff);
                        fdr.fill_length_1 = 0;
                        fdr.seg_length_1 = SEG_LENGTH_1;
                        fdr.offset_1 = 80;
                        fdr.application_block_opcode = m_application_block_opcode;
                        fdr.application_block_subsystem = m_application_block_subsystem;

                        SendS7OExchangeBlockToPlcsim(fdr);

                        // Don't know if this is a type of acknowledge or something else..
                        fdr = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 7, response: 0xffff);
                        fdr.fill_length_1 = 0;
                        fdr.seg_length_1 = 0;
                        fdr.offset_1 = 80;
                        fdr.application_block_opcode = m_application_block_opcode;
                        fdr.application_block_subsystem = m_application_block_subsystem;

                        SendS7OExchangeBlockToPlcsim(fdr);

                        SendConnectSuccessMessage("Connected");
                        m_PlcsimConnectionState = PlcsimConnectionState.Connected;
                    }
                    break;
            }
        }

        private void DisconnectPlcsimStatemachine()
        {
            switch (m_PlcsimConnectionState)
            {
                case PlcsimConnectionState.DisconnectState1:
                    S7OexchangeBlock fdr = new S7OexchangeBlock();
                    fdr = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 12, response: 0xffff);
                    fdr.fill_length_1 = 0;
                    fdr.seg_length_1 = 0;
                    fdr.offset_1 = 80;
                    fdr.application_block_opcode = m_application_block_opcode;
                    fdr.application_block_subsystem = m_application_block_subsystem;

                    SendS7OExchangeBlockToPlcsim(fdr);
                    // Simatic applications use a two-step disconnect, but this sometimes makes problems.
                    // So don't use it anymore, works also without this.
                    //
                    //m_PlcsimConnectionState = PlcsimConnectionState.DisconnectState2;
                    //break;
                //case PlcsimConnectionState.DisconnectState2:
                    //rec = ReceiveFromPlcsimDirect();
                    //if (rec.opcode == 0x0c && rec.response == 0x0001)
                    //{
                    //    m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;
                    //    ExitThisThread();
                    //}
                    m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;
                    ExitThisThread();
                    break;
            }
        }
    }
}
