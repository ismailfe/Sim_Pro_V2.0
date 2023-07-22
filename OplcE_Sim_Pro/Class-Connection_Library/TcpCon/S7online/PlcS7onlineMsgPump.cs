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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace PlcsimS7online
{
    public abstract class PlcS7onlineMsgPump : Form
    {
        private const int WM_USER = 0x0400;
        public const int WM_M_CONNECTPLCSIM = WM_USER + 1001;      // connect to Plcsim
        public const int WM_M_SENDDATA = WM_USER + 1002;           // send data to Plcsim
        public const int WM_M_EXIT = WM_USER + 1003;               // disconnect from Plcsim, and end own thread

        protected const int WM_SINEC = WM_USER + 500;

        protected byte m_application_block_opcode;
        protected byte m_application_block_subsystem;
        protected ushort m_user;
        protected int m_connectionHandle;

        protected PlcsimConnectionState m_PlcsimConnectionState;

        protected IPAddress m_PlcIPAddress;
        protected int m_PlcRack;
        protected int m_PlcSlot;
        protected int m_FdrLen;

        protected static bool m_TraceEnabled;

        public PlcS7onlineMsgPump(IPAddress plc_ipaddress, int rack, int slot)
        {
            m_PlcIPAddress = plc_ipaddress;
            m_PlcRack = rack;
            m_PlcSlot = slot;
            m_connectionHandle = -1;
            m_user = 1;
            m_PlcsimConnectionState = PlcsimConnectionState.NotConnected;

            S7OexchangeBlock fdr = new S7OexchangeBlock();
            m_FdrLen = Marshal.SizeOf(fdr);

            m_TraceEnabled = false;
            if (m_TraceEnabled)
            {
                Trace.Listeners.Add(new TextWriterTraceListener("PlcsimS7online.log"));
                DoTrace("Start trace...");
            }
        }

        protected enum PlcsimConnectionState
        {
            NotConnected,
            ConnectState1,
            ConnectState2,
            ConnectState3,
            Connected,
            DisconnectState1,
            DisconnectState2
        }

        #region External DLL References

        [DllImport("S7onlinx.dll")]
        protected static extern int SetSinecHWnd(int handle, IntPtr hwnd);

        [DllImport("S7onlinx.dll")]
        protected static extern int SetSinecHWndMsg(int handle, IntPtr hwnd, uint msg_id);

        [DllImport("S7onlinx.dll")]
        protected static extern int SCP_open([MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport("S7onlinx.dll")]
        protected static extern int SCP_close(int handle);

        [DllImport("S7onlinx.dll")]
        protected static extern int SCP_send(int handle, ushort length, byte[] data);

        [DllImport("S7onlinx.dll")]
        protected static extern int SCP_receive(int handle, ushort timeout, int[] recievendlength, ushort length, byte[] data);

        [DllImport("S7onlinx.dll")]
        protected static extern int SCP_get_errno();

        #endregion


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        protected struct S7OexchangeBlock
        {
            //Header
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public ushort[] unknown;
            public byte headerlength;               // Length of the Request Block without Userdata_1 and 2 (80 Bytes!)
            public ushort user;                     // Application Specific
            public byte rb_type;                    // Request Block type (always 2)
            public byte priority;                   // Priority of the Task, identical like serv_class in the application block
            public byte reserved_1;
            public ushort reserved_2;
            public byte subsystem;                  // For FDL Communication this is 22h = 34
            public byte opcode;                     // request, confirm, indication => same as opcode in application block
            public ushort response;                 // return-parameter => same as l_status in application block
            public ushort fill_length_1;
            public byte reserved_3;
            public ushort seg_length_1;             // Length of Userdata_1
            public ushort offset_1;
            public ushort reserved_4;
            public ushort fill_length_2;
            public byte reserved_5;
            public ushort seg_length_2;
            public ushort offset_2;
            public ushort reserved_6;
            //End of Header

            //Application Block
            public byte application_block_opcode;                       // class of communication   (00 = request, 01=confirm, 02=indication)
            public byte application_block_subsystem;                    // number of source-task (only necessary for MTK-user !!!!!)
            public ushort application_block_id;                         // identification of FDL-USER
            public ushort application_block_service;                    // identification of service (00 -> SDA, send data with acknowlege)
            public byte application_block_local_address_station;        // only for network-connection !!!
            public byte application_block_local_address_segment;        // only for network-connection !!!
            public byte application_block_ssap;                         // source-service-access-point
            public byte application_block_dsap;                         // destination-service-access-point
            public byte application_block_remote_address_station;       // address of the remote-station
            public byte application_block_remote_address_segment;       // only for network-connection !!!
            public ushort application_block_service_class;              // priority of service
            public Int32 application_block_receive_l_sdu_buffer_ptr;    // address and length of received netto-data, exception:
            public byte application_block_receive_l_sdu_length;         // address and length of received netto-data, exception:
            public byte application_block_reserved_1;                   // (reserved for FDL !!!!!!!!!!)
            public byte application_block_reserved;                     // (reserved for FDL !!!!!!!!!!)
            public Int32 application_block_send_l_sdu_buffer_ptr;       // address and length of send-netto-data, exception:
            public byte application_block_send_l_sdu_length;            // address and length of send-netto-data, exception:
            public ushort application_block_l_status;                   // link-status of service or update_state for srd-indication
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public ushort[] application_block_reserved_2;               // for concatenated lists       (reserved for FDL !!!!!!!!!!)
            //End Application block

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] reference;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte[] user_data_1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class WndProcMessage
        {
            public int pdulength;
            public byte[] pdu;
        }

        public enum MessageFromPlcsimType
        {
            Pdu,
            ConnectSuccess,
            ConnectError,
            StatusMessage,
            ErrorMessage
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class MessageFromPlcsim
        {
            public MessageFromPlcsimType type;
            public string textmessage;
            public byte[] pdu;
        }

        public void Run()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Run();
        }

        public delegate void OnDataFromPlcsimReceived(MessageFromPlcsim message);
        public event OnDataFromPlcsimReceived eventOnDataFromPlcsimReceived;

        protected void SendS7OExchangeBlockToPlcsim(S7OexchangeBlock data)
        {
            int len = Marshal.SizeOf(data);
            byte[] buffer = new byte[len];

            ushort send_len = (ushort)(data.seg_length_1 + data.headerlength);

            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(data, ptr, false);
            Marshal.Copy(ptr, buffer, 0, len);
            Marshal.DestroyStructure(ptr, typeof(S7OexchangeBlock));
            Marshal.FreeHGlobal(ptr);

            int ret = SCP_send(m_connectionHandle, send_len, buffer);
            if (ret < 0)
            {
                SendErrorMessage("ERROR: SCP_send failed");
                ExitThisThread();
            }
        }

        protected WndProcMessage GetReceivedMessage(ref Message m)
        {
            byte[] b = new Byte[Marshal.ReadInt32(m.LParam, IntPtr.Size)];
            IntPtr dataPtr = Marshal.ReadIntPtr(m.LParam, IntPtr.Size * 2);
            Marshal.Copy(dataPtr, b, 0, b.Length);
            GCHandle handle = GCHandle.Alloc(b, GCHandleType.Pinned);
            WndProcMessage rec = (WndProcMessage)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(WndProcMessage));
            handle.Free();

            return rec;
        }

        public List<string> ListReachablePartners()
        {
            List<string> reachablePartners = new List<string>();

            S7OexchangeBlock fdr;
            S7OexchangeBlock rec;

            // Shows only TCP/IP reachable Partners
            m_connectionHandle = SCP_open("S7ONLINE");
            if (m_connectionHandle >= 0)
            {
                // SetSinecHWndMsg is not needed here, as we don't use asynchron modus in this function
                // SetSinecHWndMsg(m_connectionHandle, this.Handle, WM_SINEC);
                fdr = GetNewS7OexchangeBlock(user: 1, rb_type: 2, subsystem: 0x7a, opcode: 128, response: 0xff);
                fdr.priority = 1;
                fdr.fill_length_1 = 13;
                fdr.seg_length_1 = 0;
                fdr.offset_1 = 80;

                SendS7OExchangeBlockToPlcsim(fdr);
                rec = ReceiveFromPlcsimDirect();

                while (rec.user != 0 && rec.fill_length_1 >= 64)
                {
                    byte[] ipadr = new byte[4];
                    ipadr[0] = rec.user_data_1[4];
                    ipadr[1] = rec.user_data_1[5];
                    ipadr[2] = rec.user_data_1[6];
                    ipadr[3] = rec.user_data_1[7];
                    IPAddress ip = new IPAddress(ipadr);

                    // There are two null-terminated strings in the result.
                    // The first is something like "S7-400 CP", the second like "S7-300 CP: 192.168.  0. 12"
                    // The first gives a different type of CPU, thus only use the second one, as there is no additional info.
                    // string typename = GetAsciiStringFromBytes(rec.user_data_1, 10, 22);
                    string objectname = GetAsciiStringFromBytes(rec.user_data_1, 32, 40);
                    reachablePartners.Add(ip.ToString() + " [" + objectname + "]");

                    rec = ReceiveFromPlcsimDirect();
                }
            }
            return reachablePartners;
        }

        private string GetAsciiStringFromBytes(byte[] buffer, int start, int maxlen)
        {
            for (int i = start; i < start + maxlen; i++)
            {
                if (buffer[i] != 0) continue;
                return Encoding.ASCII.GetString(buffer, start, i - start);
            }
            // No terminating null found
            return Encoding.ASCII.GetString(buffer, start, maxlen);
        }

        protected S7OexchangeBlock ReceiveFromPlcsimDirect()
        {
            S7OexchangeBlock rec = new S7OexchangeBlock();
            int[] rec_len = new int[1];
            int len = Marshal.SizeOf(rec);
            byte[] buffer = new byte[len];

            if (SCP_receive(m_connectionHandle, 0, rec_len, (ushort)len, buffer) == 0)
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                rec = (S7OexchangeBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(S7OexchangeBlock));
                handle.Free();
            }
            return rec;
        }

        protected void ExitThisThread()
        {
            DoTrace("ExitThisThread()");
            // First step is to exit the WndProc message loop
            Application.ExitThread();
            if (m_connectionHandle >= 0)
            {
                SCP_close(m_connectionHandle);
                m_connectionHandle = -1;
            }
        }

        protected void SendErrorMessage(string text)
        {
            if (eventOnDataFromPlcsimReceived != null)
            {
                MessageFromPlcsim message = new MessageFromPlcsim();
                message.type = MessageFromPlcsimType.ErrorMessage;
                message.textmessage = text;
                message.pdu = null;
                eventOnDataFromPlcsimReceived(message);
            }
        }

        protected void SendConnectSuccessMessage(string text)
        {
            if (eventOnDataFromPlcsimReceived != null)
            {
                MessageFromPlcsim message = new MessageFromPlcsim();
                message.type = MessageFromPlcsimType.ConnectSuccess;
                message.textmessage = text;
                message.pdu = null;
                eventOnDataFromPlcsimReceived(message);
            }
        }

        protected void SendConnectErrorMessage(string text)
        {
            if (eventOnDataFromPlcsimReceived != null)
            {
                MessageFromPlcsim message = new MessageFromPlcsim();
                message.type = MessageFromPlcsimType.ConnectError;
                message.textmessage = text;
                message.pdu = null;
                eventOnDataFromPlcsimReceived(message);
            }
        }

        protected void SendStatusMessage(string text)
        {
            if (eventOnDataFromPlcsimReceived != null)
            {
                MessageFromPlcsim message = new MessageFromPlcsim();
                message.type = MessageFromPlcsimType.StatusMessage;
                message.textmessage = text;
                message.pdu = null;
                eventOnDataFromPlcsimReceived(message);
            }
        }

        protected void SendPduMessage(byte[] data)
        {
            if (eventOnDataFromPlcsimReceived != null)
            {
                MessageFromPlcsim message = new MessageFromPlcsim();
                message.type = MessageFromPlcsimType.Pdu;
                message.textmessage = String.Empty;
                message.pdu = data;
                eventOnDataFromPlcsimReceived(message);
            }
        }

        protected S7OexchangeBlock GetNewS7OexchangeBlock(ushort user, byte rb_type, byte subsystem, byte opcode, ushort response)
        {
            S7OexchangeBlock block = new S7OexchangeBlock();
            block.headerlength = 80;
            block.user = user;
            block.rb_type = rb_type;
            block.subsystem = subsystem;
            block.opcode = opcode;
            block.response = response;

            return block;
        }

        protected void DumpS7OexchangeBlock(S7OexchangeBlock fd)
        {
            string dump;
            dump = ("******************** S7OexchangeBlock **********************************" + Environment.NewLine);
            dump += ("** Header Start **" + Environment.NewLine);
            //dump += ("unknown      : 0x" + String.Format("{0:X02}", fd.unknown[0] + fd.unknown[1] + Environment.NewLine);
            dump += ("headerlength : " + fd.headerlength + Environment.NewLine);
            dump += ("user         : 0x" + String.Format("{0:X02}", fd.user) + Environment.NewLine);
            dump += ("rb_type      : 0x" + String.Format("{0:X02}", fd.rb_type) + Environment.NewLine);
            dump += ("priority     : 0x" + String.Format("{0:X02}", fd.priority) + Environment.NewLine);
            dump += ("reserved_1   : 0x" + String.Format("{0:X02}", fd.reserved_1) + Environment.NewLine);
            dump += ("reserved_2   : 0x" + String.Format("{0:X02}", fd.reserved_2) + Environment.NewLine);
            dump += ("subsystem    : 0x" + String.Format("{0:X02}", fd.subsystem) + Environment.NewLine);
            dump += ("opcode       : 0x" + String.Format("{0:X02}", fd.opcode) + Environment.NewLine);
            dump += ("response     : 0x" + String.Format("{0:X02}", fd.response) + Environment.NewLine);
            dump += ("fill_length_1: 0x" + String.Format("{0:X02}", fd.fill_length_1) + Environment.NewLine);
            dump += ("reserved_3   : 0x" + String.Format("{0:X02}", fd.reserved_3) + Environment.NewLine);
            dump += ("seg_length_1 : 0x" + String.Format("{0:X02}", fd.seg_length_1) + Environment.NewLine);
            dump += ("offset_1     : 0x" + String.Format("{0:X02}", fd.offset_1) + Environment.NewLine);
            dump += ("reserved_4   : 0x" + String.Format("{0:X02}", fd.reserved_4) + Environment.NewLine);
            dump += ("fill_length_2: 0x" + String.Format("{0:X02}", fd.fill_length_2) + Environment.NewLine);
            dump += ("reserved_5   : 0x" + String.Format("{0:X02}", fd.reserved_5) + Environment.NewLine);
            dump += ("seg_length_2 : 0x" + String.Format("{0:X02}", fd.seg_length_2) + Environment.NewLine);
            dump += ("offset_2     : 0x" + String.Format("{0:X02}", fd.offset_2) + Environment.NewLine);
            dump += ("reserved_6   : 0x" + String.Format("{0:X02}", fd.reserved_6) + Environment.NewLine);
            dump += ("** End of Header **" + Environment.NewLine);

            dump += ("** Start of application Block **" + Environment.NewLine);
            dump += ("application_block_opcode                 : 0x" + String.Format("{0:X02}", fd.application_block_opcode) + Environment.NewLine);
            dump += ("application_block_subsystem              : 0x" + String.Format("{0:X02}", fd.application_block_subsystem) + Environment.NewLine);
            dump += ("application_block_id                     : 0x" + String.Format("{0:X02}", fd.application_block_id) + Environment.NewLine);
            dump += ("application_block_service                : 0x" + String.Format("{0:X02}", fd.application_block_service) + Environment.NewLine);
            dump += ("application_block_local_address_station  : 0x" + String.Format("{0:X02}", fd.application_block_local_address_station) + Environment.NewLine);
            dump += ("application_block_local_address_segment  : 0x" + String.Format("{0:X02}", fd.application_block_local_address_segment) + Environment.NewLine);
            dump += ("application_block_ssap                   : 0x" + String.Format("{0:X02}", fd.application_block_ssap) + Environment.NewLine);
            dump += ("application_block_dsap                   : 0x" + String.Format("{0:X02}", fd.application_block_dsap) + Environment.NewLine);
            dump += ("application_block_remote_address_station : 0x" + String.Format("{0:X02}", fd.application_block_remote_address_station) + Environment.NewLine);
            dump += ("application_block_remote_address_segment : 0x" + String.Format("{0:X02}", fd.application_block_remote_address_segment) + Environment.NewLine);
            dump += ("application_block_service_class          : 0x" + String.Format("{0:X02}", fd.application_block_service_class) + Environment.NewLine);
            dump += ("application_block_receive_l_sdu_length   : 0x" + String.Format("{0:X02}", fd.application_block_receive_l_sdu_length) + Environment.NewLine);
            dump += ("application_block_reserved_1             : 0x" + String.Format("{0:X02}", fd.application_block_reserved_1) + Environment.NewLine);
            dump += ("application_block_reserved               : 0x" + String.Format("{0:X02}", fd.application_block_reserved) + Environment.NewLine);
            dump += ("application_block_send_l_sdu_length      : 0x" + String.Format("{0:X02}", fd.application_block_send_l_sdu_length) + Environment.NewLine);
            dump += ("application_block_l_status               : 0x" + String.Format("{0:X02}", fd.application_block_l_status) + Environment.NewLine);
            dump += ("** End of application Block **" + Environment.NewLine);
            dump += ("** Start user_data_1 **" + Environment.NewLine);
            if (fd.user_data_1 != null)
                dump += HexDumping.Utils.HexDump(fd.user_data_1, fd.user_data_1.Length);
            dump += ("** End user_data_1 **" + Environment.NewLine);
            dump += ("**************************************************************");

            DoTrace(dump, false);
        }

        protected string GetErrTxt(int number)
        {
            switch (number)
            {
                case 202:
                    return "S7Online: Ressourcenengpass im Treiber oder in der Library";
                case 203:
                    return "S7Online: Konfigurationsfehler";
                case 205:
                    return "S7Online: Auftrag zur Zeit nicht erlaubt";
                case 206:
                    return "S7Online: Parameterfehler";
                case 207:
                    return "S7Online: Gerät bereits/noch nicht geöffnet.";
                case 208:
                    return "S7Online: CP reagiert nicht";
                case 209:
                    return "S7Online: Fehler in der Firmware";
                case 210:
                    return "S7Online: Speicherengpaß im Treiber";
                case 215:
                    return "S7Online: Keine Nachricht vorhanden";
                case 216:
                    return "S7Online: Fehler bei Zugriff auf Anwendungspuffer";
                case 219:
                    return "S7Online: Timeout abgelaufen";
                case 225:
                    return "S7Online: Die maximale Anzahl an Anmeldungen ist überschritten";
                case 226:
                    return "S7Online: Der Auftrag wurde abgebrochen";
                case 233:
                    return "S7Online: Ein Hilfsprogramm konnte nicht gestartet werden";
                case 234:
                    return "S7Online: Keine Autorisierung für diese Funktion vorhanden";
                case 304:
                    return "S7Online: Initialisierung noch nicht abgeschlossen";
                case 305:
                    return "S7Online: Funktion nicht implementiert";
                case 4865:
                    return "S7Online: CP-Name nicht vorhanden";
                case 4866:
                    return "S7Online: CP-Name nicht konfiguriert";
                case 4867:
                    return "S7Online: Kanalname nicht vorhanden";
                case 4868:
                    return "S7Online: Kanalname nicht konfiguriert";
                default:
                    return "S7Online: fehler nicht definiert";
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception).Message, "Unhandled UI Exception");
        }

        protected static void DoTrace(string message)
        {
            DoTrace(message, true);
        }

        protected static void DoTrace(string message, bool includeDate)
        {
            if (m_TraceEnabled)
            {
                if (includeDate)
                {
                    Trace.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + ": " + message);
                }
                else
                {
                    Trace.WriteLine(message);
                }
                Trace.Flush();
            }
        }
    }
}
