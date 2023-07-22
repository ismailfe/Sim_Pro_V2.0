using conS7;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OplcE_Sim_Pro
{
    public class PLC_TiaPortal
    {
#region VARIABLE

        public CLS CLS;
        S7Client MyPLC1 = new S7Client();
        //public string PLCIP             = "192.168.1.1";
        //public string PLCRACK           = "0";
        //public string PLCSLOT           = "1";  // s7300/400= 2   S71200/1500=1
        bool PLC_READ_ENB;
        bool PLC_WRITE_ENB;
        public string ConnectSatatus;
        public string ConnectError;
        public string PLCStatus;
        public int status;
        public int ConnectCheck = -1;

        public Thread TH_PLC_Read;

        // ############################################################################
        // ############################ PLC READ VARIABLE ############################ //
        public int      IR_StartByte;
        public int      IR_LengthByte = 4;
        public ListBox  IR_LSB_Result = new ListBox();
        byte[]          IBR_Buff;     // Buffer Byte Area
        public bool[]   IR_BOOL;      // Result Bit Area // OKUNACAK VERILER BU ARRAY İÇİNDEDİR

        byte[] AQBR_Buff;   // Buffer Byte Area

        public int      QR_StartByte;
        public int      QR_LengthByte = 4 ;
        public ListBox  QR_LSB_Result = new ListBox();
        byte[]          QBR_Buff;     // Buffer Byte Area
        public bool[]   QR_BOOL;      // Result Bit Area // OKUNACAK VERILER BU ARRAY İÇİNDEDİR

        public int      MR_StartByte;
        public int      MR_LengthByte;
        public ListBox  MR_LSB_Result = new ListBox();
        byte[]          MBR_Buff;     // Buffer Byte Area
        public bool[]   MR_BOOL;      // Result Bit Area // OKUNACAK VERILER BU ARRAY İÇİNDEDİR

        public int      DBR_DBNo;
        public int      DBR_StartByte;
        public int      DBR_LengthByte;
        public ListBox  DBR_LSB_Result = new ListBox();
        byte[]          DBBR_Buff;    // Buffer Byte Area
        public bool[]   DBR_BOOL;     // Result Bit Area // OKUNACAK VERILER BU ARRAY İÇİNDEDİR

        // ############################################################################
        // ############################ PLC WRITE VARIABLE ############################ //
        public int IW_StartByte;
        public int IW_LengthByte = 4;
        public ListBox IW_Result = new ListBox();
        byte[] IBW_Buff;            // Buffer Byte Area
        public bool[] IW_BOOL;      // Result Bit Area // YAZILACAK VERILER BU ARRAY İÇİNDEDİR

        byte[] AIBR_Buff;   // Buffer Byte Area

        public int QW_StartByte;
        public int QW_LengthByte    = 4;
        public ListBox QW_Result    = new ListBox();
        byte[] QBW_Buff;            // Buffer Byte Area
        public bool[] QW_BOOL;      // Result Bit Area // YAZILACAK VERILER BU ARRAY İÇİNDEDİR

        public int MW_StartByte     = 2000;
        public int MW_LengthByte    = 4;
        public ListBox MW_Result    = new ListBox();
        byte[] MBW_Buff;            // Buffer Byte Area
        public bool[] MW_BOOL;      // Result Bit Area // YAZILACAK VERILER BU ARRAY İÇİNDEDİR

        public int DBW_DBNo;
        public int DBW_StartByte;
        public int DBW_LengthByte   = 4;
        public ListBox DBW_Result   = new ListBox();
        byte[] DBBW_Buff;           // Buffer Byte Area
        public bool[] DBW_BOOL;     // Result Bit Area // YAZILACAK VERILER BU ARRAY İÇİNDEDİR

        #endregion

#region PLC_FirstLoad
        public string PLCFirstLoad()
        {
            try
            {
                // Adres uzunluklarını belirleme
                if (IR_LengthByte < 0) { IR_LengthByte = 1; }
                IBR_Buff    = new byte[IR_LengthByte];                  // Buffer Byte Area
                IR_BOOL     = new bool[(IR_LengthByte * 8)];            // Result Bit Area

                if (QR_LengthByte < 0) { QR_LengthByte = 1; }
                QBR_Buff    = new byte[QR_LengthByte];                  // Buffer Byte Area
                QR_BOOL     = new bool[(QR_LengthByte * 8)];            // Result Bit Area

                if (MR_LengthByte < 0) { MR_LengthByte = 1; }
                MBR_Buff    = new byte[MR_LengthByte];                  // Buffer Byte Area
                MR_BOOL     = new bool[(MR_LengthByte * 8)];            // Result Bit Area

                if (DBR_LengthByte < 0) { DBR_LengthByte = 1; }
                DBBR_Buff   = new byte[DBR_LengthByte];                 // Buffer Byte Area
                DBR_BOOL    = new bool[(DBR_LengthByte * 8)];           // Result Bit Area


                if (IW_LengthByte < 0) { IW_LengthByte = 1; }
                IBW_Buff    = new byte[IW_LengthByte];                  // Buffer Byte Area
                IW_BOOL     = new bool[(IW_LengthByte * 8)];            // Result Bit Area

                if (MW_LengthByte < 0) { MW_LengthByte = 1; }
                MBW_Buff = new byte[MW_LengthByte];                     // Buffer Byte Area
                MW_BOOL = new bool[(MW_LengthByte * 8)];                // Result Bit Area


                AIBR_Buff = new byte[16];     // Buffer Byte Area
                AQBR_Buff = new byte[16];     // Buffer Byte Area
                PLCStatus   = "";
                return "PLC Ayarları yüklemesi OK!";
            }
            catch (Exception HATA)
            {
                //Logs.ERR_LOGS(HATA.ToString());
                return "PLC Ayarları yüklemesi sırasında Hata: " + HATA;
            }


        }
#endregion

#region PLC CONNECT / DISCONNECT
        public int PLC_Connect()
        {
            try
            {
                PLCFirstLoad();

                if (ConnectCheck == 0)
                {
                    CLS.Form1.LB_Status.Text = "";
                    MyPLC1.Disconnect();
                    //PLCIP = Form_Main.TB_PLC1_IPAddress.Text;
                    //PLCRACK = Form_Main.CB_PLC1_Rack.Text.ToString();
                    //PLCSLOT = Form_Main.CB_PLC1_Slot.Text.ToString(); CLS.SimConnection.PLCIP
                    ConnectCheck = MyPLC1.ConnectTo(CLS.SimConnection.NETWORKIP, int.Parse(CLS.SimConnection.PLCRACK), int.Parse(CLS.SimConnection.PLCSLOT));     //ConnectTo("127.1.1.1", 0, 1); <= EXAMPLE
                    ConnectSatatus = MyPLC1.ErrorText(ConnectCheck);
                    CLS.Form1.LB_Status.Text = ConnectSatatus;
                }
                else
                {
                    //PLCIP = Form_Main.TB_PLC1_IPAddress.Text;
                    //PLCRACK = Form_Main.CB_PLC1_Rack.Text.ToString();
                    //PLCSLOT = Form_Main.CB_PLC1_Slot.Text.ToString(); CLS.SimConnection.PLCIP
                    ConnectCheck = MyPLC1.ConnectTo(CLS.SimConnection.NETWORKIP, int.Parse(CLS.SimConnection.PLCRACK), int.Parse(CLS.SimConnection.PLCSLOT));     //ConnectTo("127.1.1.1", 0, 1); <= EXAMPLE
                    ConnectSatatus = MyPLC1.ErrorText(ConnectCheck);
                    CLS.Form1.LB_Status.Text = ConnectSatatus;
                }

                MyPLC1.PlcGetStatus(ref status);

                if (ConnectCheck == 0)
                {
                    PLC_READ_ENB    = true;
                    PLC_WRITE_ENB   = true;
                    TH_PLC_Read     = new Thread(PLC_RW);
                    TH_PLC_Read.Start();
                  
                    ConnectError = "false";
                }
                else
                {
                    ConnectError = "true";
                }

                return 1;
            }
            catch (Exception HATA)
            {
                PLCStatus = "---";
                return -1;
            }
        }
        public int PLC_Disconnect()
        {
            try
            {
                PLC_READ_ENB    = false;
                PLC_WRITE_ENB   = false;
                TH_PLC_Read.Abort();
                
                MyPLC1.Disconnect();
                ConnectSatatus = "Disconnect";
                PLCStatus = "---";
                ConnectCheck = -1;
                CLS.Form1.PLC_SignalOutput.BackColor    = SystemColors.Control;
                CLS.Form1.PLC_SignalInput.BackColor = SystemColors.Control;
                return 1;
            }
            catch (Exception HATA)
            {
                return -1;
            }
        }
        public void PLCConTEST()
        {
            CLS.Form1.LB_Status.Text = "";
            ConnectSatatus = "";
            ConnectCheck = MyPLC1.ConnectTo(CLS.SimConnection.NETWORKIP, int.Parse(CLS.SimConnection.PLCRACK), int.Parse(CLS.SimConnection.PLCSLOT));     //ConnectTo("127.1.1.1", 0, 1); <= EXAMPLE
            ConnectSatatus = MyPLC1.ErrorText(ConnectCheck);
            CLS.Form1.LB_Status.Text = ConnectSatatus;
        }
#endregion

#region PLC CYCLE READ / WRITE    
        public void PLC_RW()
        {
           // PLCFirstLoad();
            try
            {
                while (true)
                {
                    // ########################################################################## //
                    // ########################## PLC READ LOOP ################################# //
                    if (ConnectCheck == 0 && PLC_READ_ENB)
                    {

                        // ################# READ - PLC STATUS ################# //
                        MyPLC1.PlcGetStatus(ref status);
                        if (status == 8) { PLCStatus = "RUN"; }
                        if (status == 4) { PLCStatus = "STOP"; }
                        CLS.Form1.Lb_PLCMode.Text = PLCStatus;

                        // READ KOMUTLARI
                        MyPLC1.EBRead(IR_StartByte, IR_LengthByte, IBR_Buff);                   // Input Byte Read
                        MyPLC1.ABRead(QR_StartByte, QR_LengthByte, QBR_Buff);                   // Output Byte Read
                        MyPLC1.MBRead(MR_StartByte, MR_LengthByte, MBR_Buff);                   // Marker Byte Read
                        MyPLC1.DBRead(DBR_DBNo, DBR_StartByte, DBR_LengthByte, DBBR_Buff);      // DB Byte Read
                        MyPLC1.ABRead(200, 16, AQBR_Buff);                                      // Analog Output Read (8 Adet AQ: PQW 200 - PQW214)

                        byte[] DBbytS1 = new byte[24];
                        int b = MyPLC1.DBRead(200, 0, 24, DBbytS1);

                        byte[] DBbytS2 = new byte[24];
                        int c = MyPLC1.DBRead(200, 28, 24, DBbytS2);

                        byte[] DBbytS3 = new byte[24];
                        int d = MyPLC1.DBRead(200, 56, 24, DBbytS3);

                        byte[] DBbytS4 = new byte[24];
                        int e = MyPLC1.DBRead(200, 84, 24, DBbytS4);


                        // Status Word = DWORD 
                        // Example => MD20 => bit 1 => M24.1
                        //Bit 1 = Error
                        //Bit 5 = Homming Done
                        //Bit 6 = Moving Done
                        //Bit 7 = StandStill
                        //Bit17 = HW Limit Sw Min
                        //Bit18 = Hw Limit Sw Max


                        CLS.Form1.LB_Servo1_Poz.Text            = S7.GetLRealAt(DBbytS1, 0).ToString();
                        CLS.Form1.LB_Servo1_Hiz.Text            = S7.GetLRealAt(DBbytS1, 8).ToString();
                        CLS.Form1.CHB_Servo1_ERR.Checked        = bool.Parse(S7.GetBitAt(DBbytS1, 19, 1).ToString()); // Bit - Error 
                        CLS.Form1.CHB_Servo1_Standstill.Checked = bool.Parse(S7.GetBitAt(DBbytS1, 19, 7).ToString()); // Bit - StandStill

                        CLS.Form1.LB_Servo2_Poz.Text            = S7.GetLRealAt(DBbytS2, 0).ToString();
                        CLS.Form1.LB_Servo2_Hiz.Text            = S7.GetLRealAt(DBbytS2, 8).ToString();
                        CLS.Form1.CHB_Servo2_ERR.Checked        = bool.Parse(S7.GetBitAt(DBbytS2, 19, 1).ToString()); // Bit - Error 
                        CLS.Form1.CHB_Servo2_Standstill.Checked = bool.Parse(S7.GetBitAt(DBbytS2, 19, 7).ToString()); // Bit - StandStill

                        CLS.Form1.LB_Servo3_Poz.Text            = S7.GetLRealAt(DBbytS3, 0).ToString();
                        CLS.Form1.LB_Servo3_Hiz.Text            = S7.GetLRealAt(DBbytS3, 8).ToString();
                        CLS.Form1.CHB_Servo3_ERR.Checked        = bool.Parse(S7.GetBitAt(DBbytS3, 19, 1).ToString()); // Bit - Error 
                        CLS.Form1.CHB_Servo3_Standstill.Checked = bool.Parse(S7.GetBitAt(DBbytS3, 19, 7).ToString()); // Bit - StandStill

                        CLS.Form1.LB_Servo4_Poz.Text            = S7.GetLRealAt(DBbytS4, 0).ToString();
                        CLS.Form1.LB_Servo4_Hiz.Text            = S7.GetLRealAt(DBbytS4, 8).ToString();
                        CLS.Form1.CHB_Servo4_ERR.Checked        = bool.Parse(S7.GetBitAt(DBbytS4, 19, 1).ToString()); // Bit - Error 
                        CLS.Form1.CHB_Servo4_Standstill.Checked = bool.Parse(S7.GetBitAt(DBbytS4, 19, 7).ToString()); // Bit - StandStill


                        // ################# READ - DIGITAL / ANALOG OUTPUT ################# //
                        int QR_LengthHesap = 0;
                        int QR_ReadPos = 0;
                        for (int i = 0, j = 0; i < QR_BOOL.Length; i++, j++) // Buffer Byte Alanına yazılanları bit olarak array Bool değerlerine gönderir
                        {
                            if (i == 0) { QR_LengthHesap = (QR_BOOL.Length - 8); }
                            if ((QR_BOOL.Length - i) <= QR_LengthHesap)
                            {
                                QR_ReadPos = QR_ReadPos + 1;
                                if (i > 0) { QR_LengthHesap = (QR_LengthHesap - 8); }
                            }
                            QR_BOOL[i] = S7.GetBitAt(QBR_Buff, QR_ReadPos, j);

                            if (i < 8) { CLS.Var.Output0[j].Checked = QR_BOOL[i]; } // Write Checkbox Byte 0

                            if (i >= 8 && i < 16) { CLS.Var.Output1[j].Checked = QR_BOOL[i]; } // Write Checkbox Byte 1

                            if (i >= 16 && i < 24) { CLS.Var.Output2[j].Checked = QR_BOOL[i]; } // Write Checkbox Byte 2

                            if (i >= 24 && i < 32) { CLS.Var.Output3[j].Checked = QR_BOOL[i]; } // Write Checkbox Byte 3

                            // ------------- READ - ANALOG OUTPUT --------------
                            int AQ = S7.GetIntAt(AQBR_Buff, (j * 2));
                            CLS.Var.LB_AOutput[j].Text = AQ.ToString();
                            //--------------------------------------------------

                            // PLC Read Comm Signal Lamp
                            if (i == 1)
                            {
                                if (CLS.Form1.PLC_SignalOutput.BackColor == Color.Orange) { CLS.Form1.PLC_SignalOutput.BackColor = SystemColors.Control; }
                                else { CLS.Form1.PLC_SignalOutput.BackColor = Color.Orange; }
                            }

                            if (j >= 7) { j = -1; }
                        }

                        // ################# READ - DIGITAL / ANALOG DATA BLOCK ################# //
                        int DBR_LengthHesap = 0;
                        int DBR_ReadPos = 0;
                        for (int i = 0, j = 0; i < DBR_BOOL.Length; i++, j++) // Buffer Byte Alanına yazılanları bit olarak array Bool değerlerine gönderir
                        {
                            if (i == 0) { DBR_LengthHesap = (DBR_BOOL.Length - 8); }
                            if ((DBR_BOOL.Length - i) <= DBR_LengthHesap)
                            {
                                DBR_ReadPos = DBR_ReadPos + 1;
                                if (i > 0) { DBR_LengthHesap = (DBR_LengthHesap - 8); }
                            }

                            DBR_BOOL[i] = S7.GetBitAt(DBBR_Buff, DBR_ReadPos, j);
                            if (j >= 7) { j = -1; }
                        }

                        #region SİMÜLASYONDA KULLANILMAYACAK KOD SATIRLARI
                        /* AÇIKLAMA RW AREA KOMUTU İÇİN
                         * AREA TABLE
                              S7Consts.S7AreaPE 0x81 Process Inputs.
                              S7Consts.S7AreaPA 0x82 Process Outputs.
                              S7Consts.S7AreaMK 0x83 Merkers.
                              S7Consts.S7AreaDB 0x84 DB
                              S7Consts.S7AreaCT 0x1C Counters. 
                              S7Consts.S7AreaTM 0x1D Timers
                         * VAR LENGTH TABLE
                              S7WLBit           0x01 Bit (inside a word)
                              S7WLByte          0x02 Byte (8 bit)
                              S7WLWord          0x04 Word (16 bit)
                              S7WLDWord         0x06 Double Word (32 bit)
                              S7WLReal          0x08 Real (32 bit float)
                              S7WLCounter       0x1C Counter (16 bit)
                              S7WLTimer         0x1D Timer (16 bit)
                          // Test
                          byte[] a = new byte[4];
                          ////Area Tipi, DBNo(kullanılacaksa), Start Add, Okunacak Veri adeti, Okunacak veri tipi, Buff 
                          int chk = MyPLC1.ReadArea(0x82, 0, 200, 1, 0x04, a); 
                        */

                        // ################# READ - DIGITAL / ANALOG INPUT ################# // // SİMÜLASYONDA KULLANILMIYOR!
                        //int IR_LengthHesap = 0;
                        //int IR_ReadPos = 0;
                        //for (int i = 0, j = 0; i < IR_BOOL.Length; i++, j++) // Buffer Byte Alanına yazılanları bit olarak array Bool değerlerine gönderir
                        //{
                        //    if (i == 0) { IR_LengthHesap = (IR_BOOL.Length - 8); }
                        //    if ((IR_BOOL.Length - i) <= IR_LengthHesap)
                        //    {
                        //        IR_ReadPos = IR_ReadPos + 1;
                        //        if (i > 0) { IR_LengthHesap = (IR_LengthHesap - 8); }
                        //    }

                        //    IR_BOOL[i] = S7.GetBitAt(IBR_Buff, IR_ReadPos, j);

                        //    if (j >= 7) { j = -1; }
                        //}

                        // ################# READ - DIGITAL / ANALOG MARKER ################# // // SİMÜLASYONDA KULLANILMIYOR!
                        //int MR_LengthHesap = 0;
                        //int MR_ReadPos = 0;
                        //for (int i = 0, j = 0; i < MR_BOOL.Length; i++, j++) // Buffer Byte Alanına yazılanları bit olarak array Bool değerlerine gönderir
                        //{
                        //    if (i == 0) { MR_LengthHesap = (MR_BOOL.Length - 8); }
                        //    if ((MR_BOOL.Length - i) <= MR_LengthHesap)
                        //    {
                        //        MR_ReadPos = MR_ReadPos + 1;
                        //        if (i > 0) { MR_LengthHesap = (MR_LengthHesap - 8); }
                        //    }

                        //    MR_BOOL[i] = S7.GetBitAt(MBR_Buff, MR_ReadPos, j);

                        //    //MR_LSB_Result.Items.Add("M" + i.ToString() + ": " + j.ToString() + " " + MR_BOOL[i]);
                        //    //Form_Main.LIB_PLC1_MResult.Items.Add("M" + i.ToString() + ": " + j.ToString() + " " + READ_M[i]);

                        //    if (j >= 7) { j = -1; }
                        //}
                        #endregion
                    }

                    // ########################################################################## //
                    // ######################### PLC WRITE LOOP ################################# //
                    if (ConnectCheck == 0 && PLC_WRITE_ENB)
                    {
                        //CLS.Form1.Cb_I00;
                        int MW_LengthHesap = 0;
                        int MW_ReadPos = 0;
                        for (int i = 0, j = 0; i < MW_BOOL.Length; i++, j++) // Buffer Byte Alanına yazılanları bit olarak array Bool değerlerine gönderir
                        {
                            MW_BOOL[j] = CLS.Form1.Cb_I00.Checked;
                            if (i == 0) { MW_LengthHesap = (MW_BOOL.Length - 8); }

                            if ((MW_BOOL.Length - i) <= MW_LengthHesap)
                            {
                                MW_ReadPos = MW_ReadPos + 1;
                                if (i > 0) { MW_LengthHesap = (MW_LengthHesap - 8); }
                            }

                            if (i < 8)              { MW_BOOL[i] = CLS.Var.Input0[j].Checked; }
                            if (i >= 8 && i < 16)   { MW_BOOL[i] = CLS.Var.Input1[j].Checked; }
                            if (i >= 16 && i < 24)  { MW_BOOL[i] = CLS.Var.Input2[j].Checked; }
                            if (i >= 24 && i < 32)  { MW_BOOL[i] = CLS.Var.Input3[j].Checked; }


                            if(j == 1)
                            {

                            }

                            //S7.SetBitAt(ref IBW_Buff, MW_ReadPos, j, MW_BOOL[i]);
                            S7.SetBitAt(ref MBW_Buff, MW_ReadPos, j, MW_BOOL[i]);

                            // PLC Read Comm Signal Lamp
                            if (i == 1)
                            {
                                if (CLS.Form1.PLC_SignalInput.BackColor == Color.Orange) { CLS.Form1.PLC_SignalInput.BackColor = SystemColors.Control; }
                                else { CLS.Form1.PLC_SignalInput.BackColor = Color.Orange; }
                            }


                            if (j >= 7) { j = -1; }
                        }

                       int a =  MyPLC1.MBWrite(MW_StartByte, MW_LengthByte, MBW_Buff);
                       int b =  MyPLC1.MBWrite(200, 16, AIBR_Buff);

                    }


                    Thread.Sleep(Convert.ToInt32("80"));
                }

         
            }
            catch (Exception HATA)
            {

            }



        }

        #endregion








    }
}
