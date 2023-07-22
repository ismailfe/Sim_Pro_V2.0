using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace OplcE_Sim_Pro
{
   public class Var
    {
        public CLS CLS;
        public List<Label> LB_AInput = new List<Label>();
        public List<Label> LB_AOutput = new List<Label>();

        public void AnalogIO_Seri()
        {
            //ANALOG INPUT
            LB_AInput.Add(CLS.Form1.LB_AI0);
            LB_AInput.Add(CLS.Form1.LB_AI1);
            LB_AInput.Add(CLS.Form1.LB_AI2);
            LB_AInput.Add(CLS.Form1.LB_AI3);
            LB_AInput.Add(CLS.Form1.LB_AI4);
            LB_AInput.Add(CLS.Form1.LB_AI5);
            LB_AInput.Add(CLS.Form1.LB_AI6);
            LB_AInput.Add(CLS.Form1.LB_AI7);

            //ANALOG INPUT
            LB_AOutput.Add(CLS.Form1.LB_AQ0);
            LB_AOutput.Add(CLS.Form1.LB_AQ1);
            LB_AOutput.Add(CLS.Form1.LB_AQ2);
            LB_AOutput.Add(CLS.Form1.LB_AQ3);
            LB_AOutput.Add(CLS.Form1.LB_AQ4);
            LB_AOutput.Add(CLS.Form1.LB_AQ5);
            LB_AOutput.Add(CLS.Form1.LB_AQ6);
            LB_AOutput.Add(CLS.Form1.LB_AQ7);
        }

        public List<CheckBox> Input0 = new List<CheckBox>();
        public List<CheckBox> Input1 = new List<CheckBox>();
        public List<CheckBox> Input2 = new List<CheckBox>();
        public List<CheckBox> Input3 = new List<CheckBox>();

        public List<CheckBox> Output0 = new List<CheckBox>();
        public List<CheckBox> Output1 = new List<CheckBox>();
        public List<CheckBox> Output2 = new List<CheckBox>();
        public List<CheckBox> Output3 = new List<CheckBox>();

      public void CheckBox_Seri()
       {
            AnalogIO_Seri();

        //Input
           Input0.Add(CLS.Form1.Cb_I00);
           Input0.Add(CLS.Form1.Cb_I01);
           Input0.Add(CLS.Form1.Cb_I02);
           Input0.Add(CLS.Form1.Cb_I03);
           Input0.Add(CLS.Form1.Cb_I04);
           Input0.Add(CLS.Form1.Cb_I05);
           Input0.Add(CLS.Form1.Cb_I06);
           Input0.Add(CLS.Form1.Cb_I07);
                   
           Input1.Add(CLS.Form1.Cb_I10);
           Input1.Add(CLS.Form1.Cb_I11);
           Input1.Add(CLS.Form1.Cb_I12);
           Input1.Add(CLS.Form1.Cb_I13);
           Input1.Add(CLS.Form1.Cb_I14);
           Input1.Add(CLS.Form1.Cb_I15);
           Input1.Add(CLS.Form1.Cb_I16);
           Input1.Add(CLS.Form1.Cb_I17);
                   
           Input2.Add(CLS.Form1.Cb_I20);
           Input2.Add(CLS.Form1.Cb_I21);
           Input2.Add(CLS.Form1.Cb_I22);
           Input2.Add(CLS.Form1.Cb_I23);
           Input2.Add(CLS.Form1.Cb_I24);
           Input2.Add(CLS.Form1.Cb_I25);
           Input2.Add(CLS.Form1.Cb_I26);
           Input2.Add(CLS.Form1.Cb_I27);
                    
           Input3.Add(CLS.Form1.Cb_I30);
           Input3.Add(CLS.Form1.Cb_I31);
           Input3.Add(CLS.Form1.Cb_I32);
           Input3.Add(CLS.Form1.Cb_I33);
           Input3.Add(CLS.Form1.Cb_I34);
           Input3.Add(CLS.Form1.Cb_I35);
           Input3.Add(CLS.Form1.Cb_I36);
           Input3.Add(CLS.Form1.Cb_I37);

   
           Output0.Add(CLS.Form1.Cb_Q00);
           Output0.Add(CLS.Form1.Cb_Q01);
           Output0.Add(CLS.Form1.Cb_Q02);
           Output0.Add(CLS.Form1.Cb_Q03);
           Output0.Add(CLS.Form1.Cb_Q04);
           Output0.Add(CLS.Form1.Cb_Q05);
           Output0.Add(CLS.Form1.Cb_Q06);
           Output0.Add(CLS.Form1.Cb_Q07);
            
           Output1.Add(CLS.Form1.Cb_Q10);
           Output1.Add(CLS.Form1.Cb_Q11);
           Output1.Add(CLS.Form1.Cb_Q12);
           Output1.Add(CLS.Form1.Cb_Q13);
           Output1.Add(CLS.Form1.Cb_Q14);
           Output1.Add(CLS.Form1.Cb_Q15);
           Output1.Add(CLS.Form1.Cb_Q16);
           Output1.Add(CLS.Form1.Cb_Q17);
                       
           Output2.Add(CLS.Form1.Cb_Q20);
           Output2.Add(CLS.Form1.Cb_Q21);
           Output2.Add(CLS.Form1.Cb_Q22);
           Output2.Add(CLS.Form1.Cb_Q23);
           Output2.Add(CLS.Form1.Cb_Q24);
           Output2.Add(CLS.Form1.Cb_Q25);
           Output2.Add(CLS.Form1.Cb_Q26);
           Output2.Add(CLS.Form1.Cb_Q27);
                     
           Output3.Add(CLS.Form1.Cb_Q30);
           Output3.Add(CLS.Form1.Cb_Q31);
           Output3.Add(CLS.Form1.Cb_Q32);
           Output3.Add(CLS.Form1.Cb_Q33);
           Output3.Add(CLS.Form1.Cb_Q34);
           Output3.Add(CLS.Form1.Cb_Q35);
           Output3.Add(CLS.Form1.Cb_Q36);
           Output3.Add(CLS.Form1.Cb_Q37);

       }
      public void AnalogIO_Animasyon()
      {
          //for (int i = 0; i < 8; i++)
          //{
          //    // Analog Input Animasyonu
          //    if (LB_AInput[i].Text != "0" && LB_AInput[i].Text != null)
          //    {
          //        LB_AInput[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        LB_AInput[i].BackColor = Color.Transparent;
          //    }

          //    // Analog Output Animasyonu
          //    if (LB_AOutput[i].Text != "0" && LB_AOutput[i].Text != null)
          //    {
          //        LB_AOutput[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        LB_AOutput[i].BackColor = Color.Transparent;
          //    }



          //    if (input0[i].Checked == true)
          //    {
          //        input0[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        input0[i].BackColor = Color.Transparent;
          //    }

          //    if (input1[i].Checked == true)
          //    {
          //        input1[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        input1[i].BackColor = Color.Transparent;
          //    }


          //    if (input2[i].Checked == true)
          //    {
          //        input2[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        input2[i].BackColor = Color.Transparent;
          //    }
            

          //    if (input3[i].Checked == true)
          //    {
          //        input3[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        input3[i].BackColor = Color.Transparent;
          //    }


          //    if (Output0[i].Checked == true)
          //    {
          //        Output0[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        Output0[i].BackColor = Color.Transparent;
          //    }

          //    if (Output1[i].Checked == true)
          //    {
          //        Output1[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        Output1[i].BackColor = Color.Transparent;
          //    }


          //    if (Output2[i].Checked == true)
          //    {
          //        Output2[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        Output2[i].BackColor = Color.Transparent;
          //    }

          //    if (Output3[i].Checked == true)
          //    {
          //        Output3[i].BackColor = Color.LightYellow;
          //    }
          //    else
          //    {
          //        Output3[i].BackColor = Color.Transparent;
          //    }

          //}



      }
      public void MarkerBit()
      {



      }



        public string PLC_Type = "S7300_400"; // S7300_400 - S71200_1500
        public string PLC_Connection;// = "connect"; // connect - disconnect

        bool LoginCheckBasladi;
        int Animasyon_Count;

       public Thread Th_XML_Oku;
       public Thread Th_XML_Yaz;
        Thread Th_MysqlRead;
        Thread Th_MySqlUpdate;
        Thread Th_PlcRead;
        Thread Th_PlcWrite;
        Thread Th_Port102_AL;

        bool XMLConnOK;
        bool V3OutputsifirlamaOK;
        bool V1215OutputsifirlamaOK;
        bool IPyenilemeOK;

        public bool[] M2 = new bool[8];
        public bool[] M3 = new bool[8];
        public bool[] M4 = new bool[8];
        public bool[] M5 = new bool[8];

        public byte MB2, MB3, MB4, MB5;
        public int MW2, MW4;
        public long MD2;

        public bool Bit_W;
        public bool Byte_W;
        public bool Word_W;
        public bool DWord_W;



    }
}
