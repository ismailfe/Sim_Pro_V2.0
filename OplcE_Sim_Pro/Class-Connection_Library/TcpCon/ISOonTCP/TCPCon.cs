using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using HexDumping;
using TcpLib;

namespace TCPCon
{
    public class TCPCon
    {
        public bool Connected;

        public int PduSize = 9;
        public List<byte[]> LocalTsaps;
        public bool EnableLocalTsapCheck;

        public _OnReceived OnReceived;
        public _TCPSend TCPSend;
        public _Log Log;

        public delegate void _OnReceived(IsoServiceProvider client, byte[] data);
        public delegate void _TCPSend(ConnectionState state, byte[] data);
        public delegate void _Log(string message);

        public void Process(IsoServiceProvider client, byte[] packet)
        {
            Process(client, packet, packet.Length);
        }

        public void SetValidTsaps(List<byte[]> tsaps)
        {
            LocalTsaps = tsaps;
        }

        public void Process(IsoServiceProvider client, byte[] inPacket, int len)
        {
            int workedLen;
            TPKT PKT;
            TPDU PDU;
            byte[] packet = new byte[len];
            Array.Copy(inPacket, packet, len);
            workedLen = 0;
            while (workedLen < len)
            {
                PKT = new TPKT(packet, len);
                PDU = new TPDU(PKT.Payload);

                switch (PDU.PDUType)
                {
                    case (byte)TPDU.TPDU_TYPES.CR:
                        if (Connected)
                        {
                            break;
                        }

                        TPKT resPkt = new TPKT();
                        TPDU resPdu = new TPDU();
                        resPdu.PDUType = (byte)TPDU.TPDU_TYPES.CC;
                        Connected = false;
        
                        try
                        {
                            resPdu.PduCon = PDU.PduCon.HandleConnectRequest(LocalTsaps, PduSize, EnableLocalTsapCheck);
                            resPkt.SetPayload(resPdu.GetBytes());
                            TCPSend(client.client, resPkt.GetBytes());
                            Connected = true;
                        }
                        catch {

                        }
                        break;
                    case (byte)TPDU.TPDU_TYPES.DT:
                        if (!Connected)
                        {
                            Log("DT packet before state 'connected' received.");
                            break;
                        }
                        OnReceived(client, PDU.PduData.Payload);
                        break;
                    case (byte)TPDU.TPDU_TYPES.DR:
                        client.client.EndConnection();
                        break;
                    default:
                        Log("Cannot handle pdu type " + PDU.PDUType);
                        break;
                }
                workedLen += PKT.Length;
                Array.Copy(inPacket, workedLen, packet, 0, len - workedLen);
            }
        }
        public int Send(ConnectionState state, byte[] data, int len)
        {
            byte[] d = new byte[len];
            Array.Copy(data, d, len);
            return Send(state, d);
        }

        public int Send(ConnectionState state, byte[] data)
        {
            if (!Connected)
            {
                Log("Cannot send in state 'not connected'");
                return -1;
            }
            TPKT resPkt = new TPKT();
            TPDU resPdu = new TPDU();

            resPdu.MakeDataPacket(data);
            resPkt.SetPayload(resPdu.GetBytes());
            TCPSend(state, resPkt.GetBytes());
            return 0;
        }
    }
}
