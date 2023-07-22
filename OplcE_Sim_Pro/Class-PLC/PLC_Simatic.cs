using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using S7PROSIMLib;

namespace OplcE_Sim_Pro
{
    public class PLC_Simatic
    {
        public CLS CLS;


        #region S7Prosim Variable
        string ConnectStart;


        public string PLCIP;
        public string ConnectSatatus;
        public string ConnectError;
        public string PLCSatatus;

        S7PROSIMLib.S7ProSimClass MyPLC1 = new S7PROSIMLib.S7ProSimClass();

        object[] I0 = new object[8];
        object[] I1 = new object[8];
        object[] I2 = new object[8];
        object[] I3 = new object[8];

        object[] Q0 = new object[8];
        object[] Q1 = new object[8];
        object[] Q2 = new object[8];
        object[] Q3 = new object[8];

        object[] AI = new object[8];
        object[] AQ = new object[8];

        public string[] AOutput = new string[8];
        public string[] AInput = new string[8];

        Thread TH_PLC_RW;
        #endregion

        #region PLC Simülasyon Prosedür

        public void PLC_Connect()
        {
            try
            {
                MyPLC1.Connect();
                MyPLC1.SetScanMode(ScanModeConstants.ContinuousScan);
                ConnectStart = "OK";
                PLCSatatus = MyPLC1.GetState().ToString();

                TH_PLC_RW = new Thread(PLC_RW);
                TH_PLC_RW.Start();
            }
            catch (Exception HATA)
            {

            }
        }


        public void PLC_Disconnect()
        {
            MyPLC1.Disconnect();
            TH_PLC_RW.Abort();

            CLS.Form1.PLC_SignalOutput.BackColor = SystemColors.Control;
            CLS.Form1.PLC_SignalInput.BackColor = SystemColors.Control;
        }

        public void PLC_RW()
        {
            int Mybyte = 0;
            int Mybit = 0;

            object Data = 0;
            MyPLC1.ReadFlagValue(Mybyte, Mybit, PointDataTypeConstants.S7_Word, ref Data);

            if (Data != null)

                while (true)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        try
                        {
                            CLS.Form1.Lb_PLCMode.Text = MyPLC1.GetState().ToString();

                            MyPLC1.ReadOutputPoint(0, i, PointDataTypeConstants.S7_Bit, ref Q0[i]);
                            CLS.Var.Output0[i].Checked = bool.Parse(Q0[i].ToString());
                            // if (Q0[i].ToString() == "true") { CLS.Var.Output0[i].Checked = true; } else if (Q0[i].ToString() == "false") { CLS.Var.Output0[i].Checked = false; }

                            MyPLC1.ReadOutputPoint(1, i, PointDataTypeConstants.S7_Bit, ref Q1[i]);
                            CLS.Var.Output1[i].Checked = bool.Parse(Q1[i].ToString());
                            //if (Q1[i].ToString() == "true") { CLS.Var.Output1[i].Checked = true; } else if (Q1[i].ToString() == "false") { CLS.Var.Output1[i].Checked = false; }

                            MyPLC1.ReadOutputPoint(2, i, PointDataTypeConstants.S7_Bit, ref Q2[i]);
                            CLS.Var.Output2[i].Checked = bool.Parse(Q2[i].ToString());
                            //if (Q2[i].ToString() == "true") { CLS.Var.Output2[i].Checked = true; } else if (Q2[i].ToString() == "false") { CLS.Var.Output2[i].Checked = false; }

                            MyPLC1.ReadOutputPoint(3, i, PointDataTypeConstants.S7_Bit, ref Q3[i]);
                            CLS.Var.Output3[i].Checked = bool.Parse(Q3[i].ToString());
                            //if (Q3[i].ToString() == "true") { CLS.Var.Output3[i].Checked = true; } else if (Q3[i].ToString() == "false") { CLS.Var.Output3[i].Checked = false; }

                            MyPLC1.ReadOutputPoint((200 + i * 2), 0, PointDataTypeConstants.S7_Word, ref AQ[i]);
                            AOutput[i] = AQ[i].ToString();
                            CLS.Var.LB_AOutput[i].Text = AQ[i].ToString();

                            //if (CLS.Var.Input0[i].Checked) { I0[i] = true; } else { I0[i] = false; }
                            I0[i] = CLS.Var.Input0[i].Checked;
                            MyPLC1.WriteInputPoint(0, i, ref I0[i]);


                            //if (CLS.Var.Input1[i].Checked) { I1[i] = true; } else { I1[i] = false; }
                            I1[i] = CLS.Var.Input1[i].Checked;
                            MyPLC1.WriteInputPoint(1, i, ref I1[i]);


                            if (CLS.Var.Input2[i].Checked) { I2[i] = true; } else { I2[i] = false; }
                            MyPLC1.WriteInputPoint(2, i, ref I2[i]);


                            if (CLS.Var.Input3[i].Checked) { I3[i] = true; } else { I3[i] = false; }
                            MyPLC1.WriteInputPoint(3, i, ref I3[i]);

                            
                            AI[i] = short.Parse(CLS.Var.LB_AInput[i].Text);
                            MyPLC1.WriteInputPoint((200 + i * 2), 0, ref AI[i]);

                            //if (SimTab.OkuAInput[i] != null) { AI[i] = short.Parse(SimTab.OkuAInput[i]); };
                            //MyPLC1.WriteInputPoint((200 + i * 2), 0, ref AI[i]);

                            //AInput[i] = AI[i].ToString();
                            //MyVar.LB_AInput[i].Text = AI[i].ToString();

                            Thread.Sleep(10);
                            if (i >= 7)
                            {
                                if (CLS.Form1.PLC_SignalOutput.BackColor == Color.Orange) { CLS.Form1.PLC_SignalOutput.BackColor = SystemColors.Control; }
                                else { CLS.Form1.PLC_SignalOutput.BackColor = Color.Orange; }

                                if (CLS.Form1.PLC_SignalInput.BackColor == Color.Orange) { CLS.Form1.PLC_SignalInput.BackColor = SystemColors.Control; }
                                else { CLS.Form1.PLC_SignalInput.BackColor = Color.Orange; }

                                //i = -1;
                            }
                        }
                        catch (Exception)
                        {

                        }

                    }
                }
               



        }



        #endregion






    }
}
