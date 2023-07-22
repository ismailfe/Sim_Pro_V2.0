using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using HexDumping;

namespace TCPCon
{

    public enum VP
    {
        ACK_TIME     	= 0x85,      
        RES_ERROR    	= 0x86,      
        PRIORITY     	= 0x87,      
        TRANSIT_DEL  	= 0x88,      
        THROUGHPUT   	= 0x89,      
        SEQ_NR       	= 0x8A,      
        REASSIGNMENT 	= 0x8B,      
        FLOW_CNTL    	= 0x8C,      
        TPDU_SIZE    	= 0xC0,      
        SRC_TSAP     	= 0xC1,      
        DST_TSAP     	= 0xC2,      
        CHECKSUM     	= 0xC3,      
        VERSION_NR   	= 0xC4,      
        PROTECTION   	= 0xC5,      
        OPT_SEL      	= 0xC6,      
        PROTO_CLASS  	= 0xC7,      
        PREF_MAX_TPDU_SIZE = 0xF0,
        INACTIVITY_TIMER = 0xF2,
        ADDICC          = 0xe0       
    }

    public struct VarParam : IComparable
    {
        public byte code;   
        public byte length; 
        public byte[] value;

        public override bool Equals(Object obj)
        {
            return (this.code == (byte)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(Object obj)
        {
            byte v = (byte)obj;
            if (this.code < v)
                return -1;
            else if (this.code > v)
                return 1;
            else
                return 0;
        }
    }

    public class TPDUConnection
    {
        public const int MAX_TSAP_LEN = 32;

        public UInt16 DstRef;     
        public UInt16 SrcRef;     
        public int ClassOption;   
        ArrayList Varpart;        
        private int MaxTDPUSize;

        public TPDUConnection()
        { }

        public TPDUConnection(byte[] packet)
        {
            int pos;
            int li = packet[0];
            TPDU.TPDU_TYPES type = (TPDU.TPDU_TYPES)(packet[1] >> 4);

            if ((type != TPDU.TPDU_TYPES.CR) && (type != TPDU.TPDU_TYPES.CC))
                throw new ApplicationException("TPDU: This can only handle CC/CR TDPUs");

            if (BitConverter.IsLittleEndian)
            {
                DstRef = ByteConvert.DoReverseEndian(BitConverter.ToUInt16(packet, 2));
                SrcRef = ByteConvert.DoReverseEndian(BitConverter.ToUInt16(packet, 4));
            }
            else
            {
                DstRef = BitConverter.ToUInt16(packet, 2);
                SrcRef = BitConverter.ToUInt16(packet, 4);
            }

            ClassOption = ((packet[6] & 0xf0) >> 4);
            if (ClassOption > 4)
                throw new Exception("TPDU: Class option number not allowed.");

            pos = 7;
            Varpart = new ArrayList();
            try
            {
                while (pos < li)
                {
                    VarParam vp = new VarParam();
                    vp.code = packet[pos];
                    pos += 1;
                    vp.length = packet[pos];
                    pos += 1;
                    vp.value = new byte[vp.length];
                    Array.Copy(packet, pos, vp.value, 0, vp.length);
                    pos += vp.length;
                    Varpart.Add(vp);
                }
            }
            catch
            {
                throw new Exception("TPDU: Error parsing variable part of CR/CC PDU.");
            }
        }

        public TPDUConnection HandleConnectRequest(List<byte[]> LocalTsaps, int maxTDPUSize, bool enableTsapCheck)
        {
            ArrayList varpart = new ArrayList();
            TPDUConnection ccpdu = new TPDUConnection();
            Random random = new Random();

            this.MaxTDPUSize = maxTDPUSize;

            ccpdu.DstRef = SrcRef;
            ccpdu.SrcRef = Convert.ToUInt16(random.Next(1, 32766));
            ccpdu.ClassOption = 0;

            int indDstTsap = Varpart.IndexOf((byte)VP.DST_TSAP);
            if (indDstTsap < 0)
                throw new Exception("TPDU: Missing destination tsap in connect request.");
            if (enableTsapCheck)
            {
                bool validTsapFound = false;
                foreach (byte[] tsap in LocalTsaps)
                {
                    if (((VarParam)Varpart[indDstTsap]).length == tsap.Length)
                    {
                        for (int i = 0; i < tsap.Length; i++)
                        {
                            if (((VarParam)Varpart[indDstTsap]).value[i] == tsap[i])
                            {
                                if (i == tsap.Length - 1)
                                {
                                    validTsapFound = true;
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (validTsapFound)
                    {
                        break;
                    }
                }
                if (validTsapFound == false)
                {
                    throw new Exception("TPDU: Destination tsap mismatch.");
                }
            }

            int indSrcTsap = Varpart.IndexOf((byte)VP.SRC_TSAP);
            if (indSrcTsap < 0)
                throw new Exception("TPDU: Missing source tsap in connect request.");

            VarParam vp = new VarParam();
            ccpdu.Varpart = new ArrayList();

            vp.code = (byte)VP.SRC_TSAP;
            vp.length = ((VarParam)Varpart[indSrcTsap]).length;
            vp.value = new byte[vp.length];
            Array.Copy(((VarParam)Varpart[indSrcTsap]).value, vp.value, vp.length);
            ccpdu.Varpart.Add(vp);

            vp.code = (byte)VP.DST_TSAP;
            vp.length = ((VarParam)Varpart[indDstTsap]).length;
            vp.value = new byte[vp.length];
            Array.Copy(((VarParam)Varpart[indDstTsap]).value, vp.value, vp.length);
            ccpdu.Varpart.Add(vp);

            int indPduSize = Varpart.IndexOf((byte)VP.TPDU_SIZE);
            if (indPduSize >= 0)
            {
                if (((VarParam)Varpart[indPduSize]).value[0] < MaxTDPUSize)
                {
                    MaxTDPUSize = ((VarParam)Varpart[indPduSize]).value[0];
                }
            }
            vp.code = (byte)VP.TPDU_SIZE;
            vp.length = 1;
            vp.value = new byte[1];
            vp.value[0] = Convert.ToByte(MaxTDPUSize);
            ccpdu.Varpart.Add(vp);

            return ccpdu;
        }

        public byte[] GetBytes()
        {
            int len = 5; 
            foreach (VarParam v in Varpart)
            {
                len += 1 + 1 + v.length;
            }
            byte[] res = new byte[len];

            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(ByteConvert.DoReverseEndian(DstRef)), 0, res, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(ByteConvert.DoReverseEndian(SrcRef)), 0, res, 2, 2);
            }
            else
            {
                Buffer.BlockCopy(BitConverter.GetBytes(DstRef), 0, res, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(SrcRef), 0, res, 2, 2);
            }
            res[4] = (byte)ClassOption;

            int pos = 5;
            foreach (VarParam v in Varpart)
            {
                res[pos++] = v.code;
                res[pos++] = v.length;
                Buffer.BlockCopy(v.value, 0, res, pos, v.length);
                pos += v.length;
            }
            return res;
        }

        public int GetMaxTDPSize()
        {
            return MaxTDPUSize;
        }

        public string GetStringDump()
        {
            string message;
            message =  "Source reference      : " + SrcRef + Environment.NewLine;
            message += "Destination reference : " + DstRef + Environment.NewLine;
            message += "Class number          : " + ClassOption + Environment.NewLine;
            message += "---------------------" + Environment.NewLine;
            foreach (VarParam v in Varpart)
            {
                message += "Var. part code   : " + v.code + " <" + ((VP)v.code).ToString() + ">" + Environment.NewLine;
                message += "Var. part length : " + v.length + Environment.NewLine;
                message += "Var. part value  : " + Environment.NewLine;
                message += Utils.HexDump(v.value, v.length) + Environment.NewLine;
                message += "---------------------" + Environment.NewLine;
            }
            return message;
        }
    }
}
