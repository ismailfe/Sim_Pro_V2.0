using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPCon
{
    public class TPKT
    {
        public const int TPKT_HEADER_LENGTH = 4;
        public const int TPKT_VERSION = 3;

        public int Version = TPKT_VERSION;         
        public int Reserved = 0;                   
        public UInt16 Length;                      

        private byte[] _Payload;                   
        public byte [] Payload {
            get {return _Payload;}
            set { SetPayload(value); }
        }

        public int PayloadLength;                  

        public TPKT()
        {
        }

        public TPKT(byte[] packet)
            : this(packet, packet.Length)
        {
        }

        public TPKT(byte[] packet, int packetLen)
        {
            if (packetLen < TPKT_HEADER_LENGTH)
                throw new Exception("TPKT: The packet did not contain the minimum number of bytes for an TPKT header packet.");

            Version = packet[0];

            if (Version != 3)
                throw new Exception("TPKT: Version in header is not valid (!=3).");

            if (BitConverter.IsLittleEndian)
            {
                Length = ByteConvert.DoReverseEndian(BitConverter.ToUInt16(packet, 2));
            }
            else
            {
                Length = BitConverter.ToUInt16(packet, 2);
            }

            if (Length > packetLen)
                throw new Exception("TPKT: Length in header is greater than packet length.");

            PayloadLength = Length - TPKT_HEADER_LENGTH;
            _Payload = new byte[PayloadLength];
            Array.Copy(packet, 4, _Payload, 0, PayloadLength);
        }

        public void SetPayload(byte[] data)
        {
            _Payload = new byte[data.Length];
            Array.Copy(data, 0, _Payload, 0, data.Length);
            PayloadLength = data.Length;
            Length = Convert.ToUInt16(PayloadLength + TPKT_HEADER_LENGTH);
        }

        public byte[] GetBytes()
        {
            byte[] tpkt = new byte[Length];

            tpkt[0] = Convert.ToByte(Version);
            tpkt[1] = Convert.ToByte(Reserved);

            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(ByteConvert.DoReverseEndian(Length)), 0, tpkt, 2, 2);
            }
            else
            {
                Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, tpkt, 2, 2);
            }

            Array.Copy(_Payload, 0, tpkt, TPKT_HEADER_LENGTH, PayloadLength);
            return tpkt;
        }
    }
}
