
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using TcpLib;
using PlcsimS7online;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TCPCon
{
    public class IsoToS7online : IDisposable
    {
        private TcpServer m_Server;
        private IsoServiceProvider m_Provider;
        private bool m_enableTsapCheck;
        private bool m_Disposed = false;
        public Queue<QueueEntry> m_ReceiveQueue = new Queue<QueueEntry>(100);
        public Queue<QueueEntry> m_SendQueue = new Queue<QueueEntry>(100);
        private IPAddress m_NetworkIpAdress;

        public IsoToS7online(bool enableTsapCheck)
        {
            m_enableTsapCheck = enableTsapCheck;
        }

        ~IsoToS7online()
        {
            Dispose(false);
        }

        public void start(string name, IPAddress networkIpAdress, List<byte[]> tsaps, IPAddress plcsimIp, int plcsimRackNumber, int plcsimSlotNumber)
        {
            m_Provider = new IsoServiceProvider();
            m_Provider.ISOsrv.OnReceived = this.IsoReceived;
            m_Provider.ISOsrv.SetValidTsaps(tsaps);
            m_Provider.ISOsrv.EnableLocalTsapCheck = m_enableTsapCheck;
            m_NetworkIpAdress = networkIpAdress;
            m_Provider.m_PlcsimIpAdress = plcsimIp;
            m_Provider.m_PlcsimRackNumber = plcsimRackNumber;
            m_Provider.m_PlcsimSlotNumber = plcsimSlotNumber;
            m_Provider.m_Name = name;

            m_Server = new TcpServer(m_Provider, 102);
            m_Server.Start(m_NetworkIpAdress);
        }

        public void stop()
        {
            m_Server.Stop();
        }

        public string Name { get { return m_Provider.m_Name; } }
        public IPAddress NetworkIpAdress { get { return m_NetworkIpAdress; } }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!m_Disposed)
                {
                    try
                    {
                        stop();
                    }
                    catch
                    {

                    }
                }
            }
            this.m_Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void IsoReceived(IsoServiceProvider client, byte[] data)
        {
            // Skip short messages
            if (data.Length < 5) return;

            // On the first S7 protocol telegram, determine the Plcsim version which has to be used
            // by checking the protocol type in S7 protocol header
            // 0x32 = S7comm
            // 0x72 = S7commPlus (1200/1500)
            if (client.m_S7ProtocolVersionDetected == false)
            {
                string message = String.Empty;
                bool plcsim_success = false;
                if (data[0] == 0x72)
                {
                    plcsim_success = client.InitPlcsim(PlcsimProtocolType.S7commPlus);
                    message = "Connecting to Plcsim using S7Comm-Plus mode for 1200/1500";
                }
                else
                {
                    plcsim_success = client.InitPlcsim(PlcsimProtocolType.S7comm);
                    message = "Connecting to Plcsim using S7Comm mode for 300/400 or 1200/1500 (not optimized)";
                }
                if (plcsim_success == false)
                {
                    if (monitorDataReceived != null)
                    {
                        monitorDataReceived(client.client.RemoteEndPoint.ToString(), null, "Failed to connect to Plcsim");
                    }  
                    client.client.EndConnection();
                    return;
                }
                if (monitorDataReceived != null)
                {
                    monitorDataReceived(client.client.RemoteEndPoint.ToString(), null, message);
                }   
                client.m_S7ProtocolVersionDetected = true;
            }

            PlcS7onlineMsgPump.WndProcMessage msg = new PlcS7onlineMsgPump.WndProcMessage();
            msg.pdu = data;
            msg.pdulength = data.Length;

            if (monitorDataReceived != null)
            {
                monitorDataReceived(client.client.RemoteEndPoint.ToString(), data, String.Empty);
            }
            byte[] res = null;
            
            // Test if we have to generate our own answer
            res = SieToCon.RequestExchange(data);
            if (res == null)
            {
                client.SendDataToPlcsim(msg);
            }
            else
            {
                client.IsoSend(client.client, res);
            }
        }

        // delegate an event for monitor output
        public delegate void dataReceived(string sourceIP, byte[] data, string message);
        public event dataReceived monitorDataReceived;
    }

    public class QueueEntry
    {
        public IsoServiceProvider isoclient;
        public ConnectionState client;
        public byte[] message;
    }

    public enum PlcsimProtocolType
    {
        S7comm = 0,             // Used for Step7 V5 Plcsim, and TIA-Plcsim for 1200/1500 when using absolute address mode (put/get) -> 0x32 protocol header
        S7commPlus              // Used for TIA-Plcsimusing new protocol used for 120071500 -> 0x72 protocol header
    }

    public class IsoServiceProvider : TcpServiceProvider
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hwnd, [MarshalAs(UnmanagedType.U4)] int Msg, IntPtr wParam, IntPtr lParam);

        public PlcS7onlineMsgPump m_PlcS7onlineMsgPump;
        IntPtr m_PlcS7onlineMsgPump_Handle;
        AutoResetEvent m_autoEvent_MsgPumpThreadStart;
        AutoResetEvent m_autoEvent_ConnectPlcsim;
        bool m_ConnectPlcsimSuccess;
        public int m_PlcsimRackNumber;
        public int m_PlcsimSlotNumber;
        public IPAddress m_PlcsimIpAdress;
        public bool m_S7ProtocolVersionDetected;
        public string m_Name;

        public TCPCon ISOsrv = new TCPCon();
        public ConnectionState client;

        public IsoServiceProvider()
        {
            ISOsrv.Log = this.IsoLog;
            ISOsrv.TCPSend = this.TCPSend;
            m_PlcS7onlineMsgPump_Handle = IntPtr.Zero;
        }

        ~IsoServiceProvider()
        {
            ExitPlcsimMessagePump();
        }

        public void IsoLog(string message)
        {
            Console.WriteLine(message);
        }

        public void TCPSend(ConnectionState state, byte[] data)
        {
            client = state;
            if (!client.Write(data, 0, data.Length))
            {
                client.EndConnection();
            }
        }

        public void IsoSend(ConnectionState state, byte[] data)
        {
            try
            {
                ISOsrv.Send(state, data);
            }
            catch
            {
                client.EndConnection();
            }
        }

        public override object Clone()
        {
            IsoServiceProvider newProvider = new IsoServiceProvider();
            newProvider.ISOsrv.OnReceived = this.ISOsrv.OnReceived;   // Copy callback
            newProvider.ISOsrv.EnableLocalTsapCheck = this.ISOsrv.EnableLocalTsapCheck;
            newProvider.ISOsrv.LocalTsaps = this.ISOsrv.LocalTsaps;
            newProvider.m_PlcsimIpAdress = this.m_PlcsimIpAdress;
            newProvider.m_PlcsimRackNumber = this.m_PlcsimRackNumber;
            newProvider.m_PlcsimSlotNumber = this.m_PlcsimSlotNumber;
            newProvider.m_PlcS7onlineMsgPump_Handle = this.m_PlcS7onlineMsgPump_Handle;
            newProvider.m_S7ProtocolVersionDetected = this.m_S7ProtocolVersionDetected;
            newProvider.m_Name = this.m_Name;

            return newProvider;
        }

        public override void OnReceiveData(ConnectionState state)
        {
            client = state;
            byte[] buffer = new byte[1460];
            int tpktlen = 0;

            while (state.AvailableData > 0)
            {
                int readBytes;
                // Read length from TPKT header first
                readBytes = client.Read(buffer, 0, 4);
                if (readBytes > 0)
                {
                    // TPKT Header
                    if (buffer[0] == 3 && buffer[1] == 0)   // Version = 3 / Reserved = 0
                    {
                        if (BitConverter.IsLittleEndian)
                        {
                            tpktlen = ByteConvert.DoReverseEndian(BitConverter.ToUInt16(buffer, 2));
                        }
                        else
                        {
                            tpktlen = BitConverter.ToUInt16(buffer, 2);
                        }
                        // try to read the TPDU to offset 4 in buffer, this is the length from TPKT minus length of TPKT header
                        readBytes = client.Read(buffer, 4, tpktlen - 4);
                    }
                    else
                    {
                        // Wrong TPKT header
                        client.EndConnection();
                        return;
                    }
                }
                // If TDPU could be read completely
                if (readBytes == (tpktlen - 4))
                {
                    try
                    {
                        ISOsrv.Process(this, buffer, readBytes + 4);
                        if (ISOsrv.Connected == false)
                        {
                            client.EndConnection();
                        }
                    }
                    catch
                    {
                        client.EndConnection();
                    }
                }
                else
                {
                    client.EndConnection();
                }
            }
        }

        public override void OnAcceptConnection(ConnectionState state)
        {
            client = state;
        }

        public override void OnDropConnection(ConnectionState state)
        {
            ExitPlcsimMessagePump();
            client = state;
        }

        private void ExitPlcsimMessagePump()
        {
            SendMessage(m_PlcS7onlineMsgPump_Handle, PlcS7onlineMsgPump.WM_M_EXIT, IntPtr.Zero, IntPtr.Zero);
        }

        public bool InitPlcsim(PlcsimProtocolType plcsimVersion)
        {
            m_autoEvent_MsgPumpThreadStart = new AutoResetEvent(false);
            StartPlcS7onlineMsgPump(plcsimVersion);
            m_autoEvent_MsgPumpThreadStart.WaitOne();       // Wait until the message pumpe thread has started
            try
            {
                m_ConnectPlcsimSuccess = false;
                m_autoEvent_ConnectPlcsim = new AutoResetEvent(false);
                SendMessage(m_PlcS7onlineMsgPump_Handle, PlcS7onlineMsgPump.WM_M_CONNECTPLCSIM, IntPtr.Zero, IntPtr.Zero);
                m_autoEvent_ConnectPlcsim.WaitOne();        // Wait until a connect success or connect error was received
            }
            catch
            {
                return false;
            }
            return m_ConnectPlcsimSuccess;
        }

        public void SendDataToPlcsim(PlcS7onlineMsgPump.WndProcMessage msg)
        {
            int length = Marshal.SizeOf(msg);
            byte[] buffer = new byte[length];

            IntPtr ptr1 = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(msg, ptr1, false);
            Marshal.Copy(ptr1, buffer, 0, length);
            Marshal.FreeHGlobal(ptr1);

            IntPtr ptr = Marshal.AllocHGlobal(IntPtr.Size * 3 + length);
            Marshal.WriteIntPtr(ptr, 0, IntPtr.Zero);
            Marshal.WriteIntPtr(ptr, IntPtr.Size, (IntPtr)length);
            IntPtr dataPtr = new IntPtr(ptr.ToInt64() + IntPtr.Size * 3);
            Marshal.WriteIntPtr(ptr, IntPtr.Size * 2, dataPtr);
            Marshal.Copy(buffer, 0, dataPtr, length);

            SendMessage(m_PlcS7onlineMsgPump_Handle, PlcS7onlineMsgPump.WM_M_SENDDATA, IntPtr.Zero, ptr);

            Marshal.FreeHGlobal(ptr);
        }

        public void StartPlcS7onlineMsgPump(PlcsimProtocolType plcsimVersion)
        {
            Thread PlcS7onlineMsgPumpThread = new Thread(StartPlcS7onlineMsgPumpThread);
            if (plcsimVersion == PlcsimProtocolType.S7commPlus)
            {
                m_PlcS7onlineMsgPump = new PlcS7onlineMsgPumpTia(m_PlcsimIpAdress, m_PlcsimRackNumber, m_PlcsimSlotNumber);
            }
            else 
            {
                m_PlcS7onlineMsgPump = new PlcS7onlineMsgPumpS7(m_PlcsimIpAdress, m_PlcsimRackNumber, m_PlcsimSlotNumber);
            }
            m_PlcS7onlineMsgPump.eventOnDataFromPlcsimReceived += new PlcS7onlineMsgPump.OnDataFromPlcsimReceived(OnDataFromPlcsimReceived);
            PlcS7onlineMsgPumpThread.Start();
        }

        private void StartPlcS7onlineMsgPumpThread()
        {
            m_PlcS7onlineMsgPump_Handle = m_PlcS7onlineMsgPump.Handle;
            m_autoEvent_MsgPumpThreadStart.Set();
            m_PlcS7onlineMsgPump.Run();
        }

        private void OnDataFromPlcsimReceived(PlcS7onlineMsgPump.MessageFromPlcsim message)
        {
            switch (message.type)
            {
                case PlcS7onlineMsgPump.MessageFromPlcsimType.Pdu:
                    // Some telegrams may have to be modified by Nettoplcsim, e.g. response to connection setup
                    // 30.1.2016: This should be no longer needed
                    //S7ProtoHook.ResponseExchange(ref message.pdu);
                    IsoSend(client, message.pdu);
                    break;
                case PlcS7onlineMsgPump.MessageFromPlcsimType.ConnectError:
                    m_ConnectPlcsimSuccess = false;
                    m_autoEvent_ConnectPlcsim.Set();
                    break;
                case PlcS7onlineMsgPump.MessageFromPlcsimType.ConnectSuccess:
                    m_ConnectPlcsimSuccess = true;
                    m_autoEvent_ConnectPlcsim.Set();
                    break;
                //default:
                    // don't care about other messages at this time
                    //System.Diagnostics.Debug.Print("OnDataFromPlcsimReceived(): Type=" + message.type.ToString() + " Message=" + message.textmessage);
            }
        }
    }
}
