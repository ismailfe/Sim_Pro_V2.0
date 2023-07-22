using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TCPCon
{
    public class TPDU
    {
        public enum TPDU_TYPES
        {
            ED = 0x1,   
            EA = 0x2,   
            UD = 0x4,   
            RJ = 0x5,   
            AK = 0x6,   
            ER = 0x7,   
            DR = 0x8,   
            DC = 0xC,   
            CC = 0xD,   
            CR = 0xE,   
            DT = 0xF    
        };

        public UInt16 Li;          
        public int PDUType;       

        public TPDUData PduData;
        public TPDUConnection PduCon;

        public TPDU()
        { }

         public TPDU(byte[] packet)
            : this(packet, packet.Length)
        {
        }

        private TPDU(byte[] packet, int packetLen)
        {

            if (packetLen < 3)
                throw new Exception("TPUD: Packet size lower than 3 bytes.");

            Li = packet[0];
            PDUType = (packet[1] >> 4);

            switch (PDUType)
            {
                case (int)TPDU_TYPES.CR:
                    PduCon = new TPDUConnection(packet);
                    break;
                case (int)TPDU_TYPES.CC:
                    PduCon = new TPDUConnection(packet);
                    break;
                case (int)TPDU_TYPES.DT:
                    PduData = new TPDUData(packet);
                    break;
            }
        }

        public byte[] GetBytes()
        {
            int size = 0;
            byte[] tpdu = null;
            switch (PDUType)
            {
                case (int)TPDU_TYPES.DT:
                    size = PduData.PayloadLength + TPDUData.DT_HLEN;
                    tpdu = new byte[size];
                    Li = 2;
                    tpdu[0] = Convert.ToByte(Li);
                    tpdu[1] = Convert.ToByte(PDUType << 4);
                    tpdu[2] = Convert.ToByte(PduData.TPDUNr);
                    if (PduData.EOT)
                       tpdu[2] += 128;

                    Array.Copy(PduData.Payload, 0, tpdu, TPDUData.DT_HLEN, PduData.Payload.Length);
                    break;
                case (int)TPDU_TYPES.CC:
                case (int)TPDU_TYPES.CR:
                    byte[] pduc;
                    pduc = PduCon.GetBytes();
                    int len = pduc.Length + 2;
                    Li = Convert.ToByte(pduc.Length + 1);
                    tpdu = new byte[len];
                    tpdu[0] = Convert.ToByte(Li);
                    tpdu[1] = Convert.ToByte(PDUType << 4);
                    Array.Copy(pduc, 0, tpdu, 2, pduc.Length);
                    break;
            }
            return tpdu;
        }

        public void MakeDataPacket(byte[] data)
        {
            PDUType = (int)TPDU_TYPES.DT;
            Li = 2;
            PduData = new TPDUData();
            PduData.TPDUNr = 0;
            PduData.EOT = true;
            PduData.Payload = new byte[data.Length];
            PduData.PayloadLength = data.Length;
            Array.Copy(data, PduData.Payload, data.Length);
        }
    }
}
