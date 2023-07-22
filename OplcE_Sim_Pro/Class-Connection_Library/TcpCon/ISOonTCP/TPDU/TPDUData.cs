using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HexDumping;

namespace TCPCon
{
    public class TPDUData
    {
        public const int HLEN_LI_NORMAL_DT_CLASS_01 = 2; 
        public const int DT_HLEN = 3;                    

        public int TPDUNr;     
        public bool EOT;       
        public byte[] Payload; 
        public int PayloadLength;

        public TPDUData()
        { }

        public TPDUData(byte[] packet)
            : this(packet, packet.Length)
        {
        }

        public TPDUData(byte[] packet, int packetLen)
        {
            if (packetLen < DT_HLEN)
                throw new Exception("TPDU: DT packet size lower than minimum of 3 bytes.");

            int li = packet[0];
            TPDU.TPDU_TYPES type = (TPDU.TPDU_TYPES)(packet[1] >> 4);

            if (type != TPDU.TPDU_TYPES.DT)
                throw new ApplicationException("TPDU: This can only handle DT TDPUs");

            if (li != HLEN_LI_NORMAL_DT_CLASS_01)
                throw new Exception("TPDU: Header length indicator in DT packet other than 2 in class 0/1 PDUs are not allowed.");

            TPDUNr = (packet[2] & 0x7f);

            if ((packet[2] & 0x80) != 0)
                EOT = true;
            else
                EOT = false;

            PayloadLength = packetLen - DT_HLEN;
            Payload = new byte[PayloadLength];
            Array.Copy(packet, DT_HLEN, Payload, 0, PayloadLength);
        }
    }
}
