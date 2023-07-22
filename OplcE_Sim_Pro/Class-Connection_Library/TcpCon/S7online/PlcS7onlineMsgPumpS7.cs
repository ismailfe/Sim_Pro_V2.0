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
    public class PlcS7onlineMsgPumpS7 : PlcS7onlineMsgPump
    {
        const int SIZE_OF_USER_DATA_1 = 1024;
        const int SEG_LENGTH_1 = 480;

        bool m_opcode6_response1_was_received;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct UserDataConnectionConfigS7
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
            public byte b17;
            public byte connection_type;
            public byte rack_slot;
            public byte b20;
            public byte size_to_end;
            public byte size_of_subnet;
            public byte subnet_1;
            public byte subnet_2;
            public byte b25;
            public byte b26;
            public byte subnet_3;
            public byte subnet_4;
            public byte size_of_routing_destination;
            public byte routing_destination_1;
            public byte routing_destination_2;
            public byte routing_destination_3;
            public byte routing_destination_4;

            public UserDataConnectionConfigS7(bool useConstructor)
                : this()
            {
                b01 = 0x02;
                b02 = 0x01;
                b04 = 0x0C;
                b05 = 0x01;
                b15 = 0x01;
                b17 = 0x02;
            }
        }

        public PlcS7onlineMsgPumpS7(IPAddress plc_ipaddress, int rack, int slot)
            : base (plc_ipaddress, rack, slot)
        {
            m_opcode6_response1_was_received = false;
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
                    ExitThisThread();
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
                // This part is essential for the work with Plcsim. This response must be sent only once,
                // otherwise the communication won't work.
                if (m_opcode6_response1_was_received == false)
                {
                    S7OexchangeBlock snd = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 7, response: 0xff);
                    snd.offset_1 = 80;
                    snd.fill_length_1 = 0;
                    snd.seg_length_1 = SEG_LENGTH_1;
                    snd.application_block_opcode = rec.application_block_opcode;
                    snd.application_block_subsystem = rec.application_block_subsystem;

                    SendS7OExchangeBlockToPlcsim(snd);
                    m_opcode6_response1_was_received = true;
                }
            }
            else if (rec.opcode == 7 && rec.response == 3)
            {
                byte[] recbytes = new byte[rec.fill_length_1];
                Array.Copy(rec.user_data_1, recbytes, rec.fill_length_1);

                SendPduMessage(recbytes);

                S7OexchangeBlock snd = GetNewS7OexchangeBlock(user: 0, rb_type: 2, subsystem: 0x40, opcode: 7, response: 0x0);
                snd.offset_1 = 80;
                snd.fill_length_1 = 0;
                snd.seg_length_1 = SEG_LENGTH_1;
                snd.application_block_opcode = rec.application_block_opcode;
                snd.application_block_subsystem = rec.application_block_subsystem;
                snd.user_data_1 = rec.user_data_1;  // don't know if this is really neccessary, as the fill_length_1 must be zero.

                SendS7OExchangeBlockToPlcsim(snd);
            }
        }

        private void SendDataToPlcsim(WndProcMessage msg)
        {
            if (++m_user >= 32000) m_user = 1;
            S7OexchangeBlock snd = GetNewS7OexchangeBlock(user: m_user, rb_type: 2, subsystem: 0x40, opcode: 6, response: 0xff);
            snd.offset_1 = 80;

            snd.application_block_opcode = m_application_block_opcode;
            snd.application_block_subsystem = m_application_block_subsystem;

            snd.fill_length_1 = (ushort)msg.pdu.Length;
            snd.seg_length_1 = (ushort)msg.pdu.Length;

            snd.user_data_1 = new byte[SIZE_OF_USER_DATA_1];
            Array.Copy(msg.pdu, snd.user_data_1, msg.pdu.Length);

            SendS7OExchangeBlockToPlcsim(snd);
        }

        private void ConnectPlcsimStatemachine()
        {
            S7OexchangeBlock fdr;
            S7OexchangeBlock rec;
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

                    fdr = GetNewS7OexchangeBlock(user: 1, rb_type: 2, subsystem: 0x40, opcode: 0, response: 0xff);
                    fdr.offset_1 = 0;

                    SendS7OExchangeBlockToPlcsim(fdr);

                    m_PlcsimConnectionState = PlcsimConnectionState.ConnectState1;
                    break;

                case PlcsimConnectionState.ConnectState1:
                    rec = ReceiveFromPlcsimDirect();
                    // save connection parameters for usage in all following telegrams
                    m_application_block_opcode = rec.application_block_opcode;
                    m_application_block_subsystem = rec.application_block_subsystem;

                    fdr = GetNewS7OexchangeBlock(user: 1, rb_type: 2, subsystem: 0x40, opcode: 1, response: 0xff);
                    fdr.offset_1 = 80;
                    fdr.fill_length_1 = 126;
                    fdr.seg_length_1 = 126;
                    fdr.application_block_opcode = m_application_block_opcode;
                    fdr.application_block_ssap = 2;
                    fdr.application_block_remote_address_station = 114;
                    fdr.application_block_subsystem = m_application_block_subsystem;

                    UserDataConnectionConfigS7 ud_cfg = new UserDataConnectionConfigS7(true);
                    ud_cfg.rack_slot = (byte)(m_PlcSlot + m_PlcRack * 32);
                    ud_cfg.connection_type = 1;
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
                    if (rec.response != 0x01)
                    {
                        SendConnectErrorMessage("ERROR: Connect failed at ConnectState2");
                        m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;
                        ExitThisThread();
                    }
                    else
                    {
                        SendConnectSuccessMessage("Connected");
                        m_PlcsimConnectionState = PlcsimConnectionState.Connected;
                    }
                    break;
            }
        }
    }
}
