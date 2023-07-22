using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OplcE_Sim_Pro
{
    public partial class Form_BitByteWord : Form
    {
        public Form_BitByteWord()
        {
            InitializeComponent();
        }
        private void Form_BitByteWord_Load(object sender, EventArgs e)
        {
            Lists();
        }
        private void Form_BitByteWord_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
        }

        private void Num_Byte1_ValueChanged(object sender, EventArgs e)
        {
            Byte1_Degisti();
        }

        private void Num_Byte2_ValueChanged(object sender, EventArgs e)
        {
            Byte2_Degisti();
        }

        private void Num_Word1_ValueChanged(object sender, EventArgs e)
        {
            Word1_Degisti();
        }

        private void Num_Byte3_ValueChanged(object sender, EventArgs e)
        {
            Byte3_Degisti();
        }

        private void Num_Byte4_ValueChanged(object sender, EventArgs e)
        {
            Byte4_Degisti();
        }

        private void Num_Word2_ValueChanged(object sender, EventArgs e)
        {
            Word2_Degisti();
        }

        private void Num_DWord_ValueChanged(object sender, EventArgs e)
        {
            DWord_Degisti();
        }



        List<CheckBox> CHB_ForByte1 = new List<CheckBox>();
        List<CheckBox> CHB_ForByte2 = new List<CheckBox>();
        List<CheckBox> CHB_ForByte3 = new List<CheckBox>();
        List<CheckBox> CHB_ForByte4 = new List<CheckBox>();

        void Lists()
        {
            CHB_ForByte1 = this.Panel_Byte1.Controls.OfType<CheckBox>().Where(a => a.Name.StartsWith("CB")).ToList();
            CHB_ForByte2 = this.Panel_Byte2.Controls.OfType<CheckBox>().Where(a => a.Name.StartsWith("CB")).ToList();
            CHB_ForByte3 = this.Panel_Byte3.Controls.OfType<CheckBox>().Where(a => a.Name.StartsWith("CB")).ToList();
            CHB_ForByte4 = this.Panel_Byte4.Controls.OfType<CheckBox>().Where(a => a.Name.StartsWith("CB")).ToList();
         
        }

        void DWord_Degisti()
        {
            long IntVal = (long)Num_DWord.Value;

            byte[] intBytes = BitConverter.GetBytes(IntVal);
            //Array.Reverse(intBytes);
            byte[] ResultBytes = intBytes;

            Num_Byte1.Value = ResultBytes[0];
            Num_Byte2.Value = ResultBytes[1];
            Num_Byte3.Value = ResultBytes[2];
            Num_Byte4.Value = ResultBytes[3];
        }

        void Word1_Degisti()
        {
            int IntVal = (int) Num_Word1.Value;

            byte[] intBytes = BitConverter.GetBytes(IntVal);
            //Array.Reverse(intBytes);
            byte[] ResultBytes = intBytes;

            Num_Byte1.Value = ResultBytes[0];
            Num_Byte2.Value = ResultBytes[1];

            ByteToDWORD(Num_Byte1.Value, Num_Byte2.Value, Num_Byte3.Value, Num_Byte4.Value, out long ResultVal);
            Num_DWord.Value = ResultVal;
        }

        void Word2_Degisti()
        {
            int IntVal = (int)Num_Word2.Value;

            byte[] intBytes = BitConverter.GetBytes(IntVal);
            //Array.Reverse(intBytes);
            byte[] ResultBytes = intBytes;

            Num_Byte3.Value = ResultBytes[0];
            Num_Byte4.Value = ResultBytes[1];


            ByteToDWORD(Num_Byte1.Value, Num_Byte2.Value, Num_Byte3.Value, Num_Byte4.Value, out long ResultVal);
            Num_DWord.Value = ResultVal;
        }



        void Byte1_Degisti()
        {
            int ByteVal     = (int)Num_Byte1.Value;
            string s        = Convert.ToString(ByteVal, 2);
            int[] bits      = s.PadLeft(8, '0').Select(c => int.Parse(c.ToString())).ToArray();
            Lists();
            for (int i = 0; i < bits.Length; i++)
            {
                CHB_ForByte1[i].Checked = Convert.ToBoolean( bits[7-i] ) ;
                CHB_IMG(CHB_ForByte1, i);
            }

            //WORD YAZ
            ByteToWORD(Num_Byte1.Value, Num_Byte2.Value, out int WordVal);
            Num_Word1.Value = WordVal;
        }
        void Byte2_Degisti()
        {
            int ByteVal = (int)Num_Byte2.Value;
            string s = Convert.ToString(ByteVal, 2);
            int[] bits = s.PadLeft(8, '0').Select(c => int.Parse(c.ToString())).ToArray();
          
            for (int i = 0; i < bits.Length; i++)
            {
              
                CHB_ForByte2[i].Checked = Convert.ToBoolean(bits[7-i]);
                CHB_IMG(CHB_ForByte2, i);
            }
            //WORD YAZ
            ByteToWORD(Num_Byte1.Value, Num_Byte2.Value, out int WordVal);
            Num_Word1.Value = WordVal;
        }
        void Byte3_Degisti()
        {
            int ByteVal = (int)Num_Byte3.Value;
            string s = Convert.ToString(ByteVal, 2);
            int[] bits = s.PadLeft(8, '0').Select(c => int.Parse(c.ToString())).ToArray();

            for (int i = 0; i < bits.Length; i++)
            {

                CHB_ForByte3[i].Checked = Convert.ToBoolean(bits[7 - i]);
                CHB_IMG(CHB_ForByte3, i);
            }
            //WORD YAZ
            ByteToWORD(Num_Byte3.Value, Num_Byte4.Value, out int WordVal);
            Num_Word2.Value = WordVal;
        }
        void Byte4_Degisti()
        {
            int ByteVal = (int)Num_Byte4.Value;
            string s = Convert.ToString(ByteVal, 2);
            int[] bits = s.PadLeft(8, '0').Select(c => int.Parse(c.ToString())).ToArray();

            for (int i = 0; i < bits.Length; i++)
            {

                CHB_ForByte4[i].Checked = Convert.ToBoolean(bits[7 - i]);
                CHB_IMG(CHB_ForByte4, i);
            }
            //WORD YAZ
            ByteToWORD(Num_Byte3.Value, Num_Byte4.Value, out int WordVal);
            Num_Word2.Value = WordVal;
        }




        void CHB_ForByte1_CheckedChanged(object sender, EventArgs e)
        {
            bool[] bools = new bool[CHB_ForByte1.Count];
            for (int i = 0; i < CHB_ForByte1.Count; i++)
            {
                CHB_IMG(CHB_ForByte1, i);
                bools[i] = CHB_ForByte1[i].Checked;

            }

            //BYTE YAZ
            BitToBYTE(bools, out int ResultVal);
            Num_Byte1.Value = ResultVal;
        }

        void CHB_ForByte2_CheckedChanged(object sender, EventArgs e)
        {
            bool[] bools = new bool[CHB_ForByte2.Count];
            for (int i = 0; i < CHB_ForByte2.Count; i++)
            {
                CHB_IMG(CHB_ForByte2, i);
                bools[i] = CHB_ForByte2[i].Checked;
            }

            //BYTE YAZ
            BitToBYTE(bools, out int ResultVal);
            Num_Byte2.Value = ResultVal;
        }

        void CHB_ForByte3_CheckedChanged(object sender, EventArgs e)
        {
            bool[] bools = new bool[CHB_ForByte3.Count];
            for (int i = 0; i < CHB_ForByte3.Count; i++)
            {
                CHB_IMG(CHB_ForByte3, i);
                bools[i] = CHB_ForByte3[i].Checked;
            }

            //BYTE YAZ
            BitToBYTE(bools, out int ResultVal);
            Num_Byte3.Value = ResultVal;
        }

        void CHB_ForByte4_CheckedChanged(object sender, EventArgs e)
        {
            bool[] bools = new bool[CHB_ForByte4.Count];
            for (int i = 0; i < CHB_ForByte4.Count; i++)
            {
                CHB_IMG(CHB_ForByte4, i);
                bools[i] = CHB_ForByte4[i].Checked;
            }

            //BYTE YAZ
            BitToBYTE(bools, out int ResultVal);
            Num_Byte4.Value = ResultVal;
        }







        #region CONVERT 
        void CHB_IMG(List<CheckBox> CHB, int i)
        {
            if (CHB[i].Checked)
            {
                CHB[i].BackgroundImage = Properties.Resources.checkbox_ON;
            }
            else
            {
                CHB[i].BackgroundImage = Properties.Resources.checkbox_OFF;
            }

        }

        void BitToBYTE(bool[] Bools, out int ResultVal)
        {
            BitArray bits;

            bits = new BitArray(Bools);
            //byte[] Bytes = new byte[bits.Length / 8];
            byte[] Bytes = new byte[1];
            bits.CopyTo(Bytes, 0);
            ResultVal = Convert.ToInt16(Bytes[0]);
        }

        void ByteToWORD(decimal Bytes1, decimal Bytes2, out int IntVal)
        {
            byte[] Bytes = new Byte[4];

            Bytes[0] = byte.Parse(Bytes1.ToString());
            Bytes[1] = byte.Parse(Bytes2.ToString());

            IntVal = BitConverter.ToInt32(Bytes, 0);
        }

        void ByteToDWORD (decimal Bytes1, decimal Bytes2, decimal Bytes3, decimal Bytes4, out long IntVal)
        {
            byte[] Bytes = new Byte[8];

            Bytes[0] = byte.Parse(Bytes1.ToString());
            Bytes[1] = byte.Parse(Bytes2.ToString());
            Bytes[2] = byte.Parse(Bytes3.ToString());
            Bytes[3] = byte.Parse(Bytes4.ToString());

            IntVal = BitConverter.ToInt64(Bytes, 0);
        }

        #endregion

        private void B_Temizle_Not_Click(object sender, EventArgs e)
        {
            Rtx_Notlar.Clear();
        }

        private void B_Temizle_Bit_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                CHB_ForByte1[i].Checked = false;
                CHB_ForByte2[i].Checked = false;
                CHB_ForByte3[i].Checked = false;
                CHB_ForByte4[i].Checked = false;
            }
        }
    }
}
