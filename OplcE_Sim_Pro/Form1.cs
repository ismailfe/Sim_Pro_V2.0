using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using System.Management;
using Microsoft.Win32;
using System.Data.SqlClient;


namespace OplcE_Sim_Pro
{
    public partial class Form1 : Form
    {

        #region VARIABLE
     

        CLS CLS                             = new CLS();
        static FirstStart FirstStart        = new FirstStart();
        static Var Var                      = new Var();
        static XMLFiles_RW XMLFiles_RW      = new XMLFiles_RW();
        static PLC_Simatic PLC_Simatic      = new PLC_Simatic();
        static PLC_TiaPortal PLC_TiaPortal  = new PLC_TiaPortal();
        static SimConnection SimConnection  = new SimConnection();
   
        bool IPAdresiDegisti;
        #endregion

        #region Genel Fonksiyonlar

        void ClassCalling()
        {
            CLS.Form1 = this;
            CLS.Var = Var;
            CLS.XMLFiles_RW = XMLFiles_RW;
            CLS.PLC_Simatic = PLC_Simatic;
            CLS.PLC_TiaPortal = PLC_TiaPortal;
            CLS.FirstStart = FirstStart;
            CLS.SimConnection = SimConnection;

            Var.CLS = CLS;
            FirstStart.CLS = CLS;
            XMLFiles_RW.CLS = CLS;
            PLC_TiaPortal.CLS = CLS;
            PLC_Simatic.CLS = CLS;
            SimConnection.CLS = CLS;
        }
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        public void Form1_Load(object sender, EventArgs e)
        {
            ClassCalling();
            CLS.FirstStart.Starting();
            Timer_1S.Enabled = true;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            Application.Exit();

        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                S7_OplcE_Pro.Visible = true;
                //S7_OplcE_Pro.ShowBalloonTip(1000);
            }
        }

        void Notify()
        {
            //// Backround Work
            S7_OplcE_Pro.BalloonTipText = "Connection Protocol is running!";
            S7_OplcE_Pro.BalloonTipTitle = " PLC Sim Protocol";
            // this.WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            S7_OplcE_Pro.Visible = true;
            S7_OplcE_Pro.ShowBalloonTip(1000);
            ////**********************
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar           = true;
            S7_OplcE_Pro.Visible    = false;
            WindowState             = FormWindowState.Normal;
        }

        #endregion

        #region TIMERS
        // Timerlar
        private void Timer_20ms_Tick(object sender, EventArgs e)
        {
            Lb_PortStatus.Text = CLS.SimConnection.PortAktivite;
        }
        private void Timer_150ms_Tick(object sender, EventArgs e)
        {
        }
        private void Timer_1S_Tick(object sender, EventArgs e)
        {
        }

        #endregion

        private void CHB_ManCheck_CheckedChanged(object sender, EventArgs e)
        {

            if (CHB_ManCheck.Checked)
            {


                RB_S71200_1500.Enabled = true;
                RB_S7300_400.Enabled = true;
                CHB_BaglanPLC.Enabled = true;
            }
            else
            {
                RB_S71200_1500.Enabled = false;
                RB_S7300_400.Enabled = false;
                CHB_BaglanPLC.Enabled = false;
            }
        }
        private void Cb_Show_I_CheckedChanged(object sender, EventArgs e)
        {
            if (Cb_Show_I.Checked == true)
            {
                Grup_I.Visible = true;
            }
            else
            {
                Grup_I.Visible = false;
            }
        }
        private void Cb_Show_Q_CheckedChanged(object sender, EventArgs e)
        {
            if (Cb_Show_Q.Checked == true)
            {
                Grup_Q.Visible = true;
            }
            else
            {
                Grup_Q.Visible = false;
            }
        }
        private void Cb_Show_A_CheckedChanged(object sender, EventArgs e)
        {
            if (Cb_Show_A.Checked == true)
            {
                Grup_A.Visible = true;
                Grup_Motion1.Visible = true;
                Grup_Motion2.Visible = true;
                Grup_Motion3.Visible = true;
                Grup_Motion4.Visible = true;
                LB_Servo_Aciklama.Visible = true;
            }
            else
            {
                Grup_A.Visible = false;
                Grup_Motion1.Visible = false;
                Grup_Motion2.Visible = false;
                Grup_Motion3.Visible = false;
                Grup_Motion4.Visible = false;
                LB_Servo_Aciklama.Visible = false;
            }
        }
        private void B_BitByteWord_Click(object sender, EventArgs e)
        {
            Form_BitByteWord form_BitByteWord = new Form_BitByteWord();
            form_BitByteWord.Show();
        }
        private void CHB_BaglanPLC_CheckedChanged(object sender, EventArgs e)
        {
            if (CHB_BaglanPLC.Checked)
            {
                int sw = 0;
                switch (sw)
                {
                    case 0:
                        //Thread thcon = new Thread(CLS.SimConnection.StartConnection);
                        //thcon.Start();
                        
                        goto case 1;
                    case 1:
                      //  Thread.Sleep(3000);

                        goto case 2;
                    case 2:

                        if (RB_S71200_1500.Checked)
                        {
                            CLS.PLC_TiaPortal.PLC_Connect();
                        }

                        if (RB_S7300_400.Checked)
                        {
                            CLS.PLC_Simatic.PLC_Connect();
                        }
                        goto case 3;
                    case 3:
                        break;
                }
              
               
            }
            else
            {
                LB_Status.Text = "Bağlantı yok!";
                if (RB_S7300_400.Checked)
                {
                    CLS.PLC_Simatic.PLC_Disconnect();
                }
                if (RB_S71200_1500.Checked)
                {
                    CLS.PLC_TiaPortal.PLC_Disconnect();
                }

            }
           
        }
        private void LB_Status_TextChanged(object sender, EventArgs e)
        {
            if(LB_Status.Text == "OK")
            {
                Signal_PLC_Connection.BackColor = Color.Lime;
            }else
            {
                Signal_PLC_Connection.BackColor = Color.Red;
            }
        }
        private void CHB_HerZamanUstte_CheckedChanged(object sender, EventArgs e)
        {
            if(CHB_HerZamanUstte.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }
        private void B_PrePLCCon_Click(object sender, EventArgs e)
        {
            //CLS.SimConnection.FirstLoad();
            //V1215_PLCBaglanti(true, out string PlcSts, out string cnnSts, out string connErr, Lb_PortStatus.Text);
            Thread thcon = new Thread(CLS.SimConnection.StartConnection);
            thcon.Start();

            //Lb_PLCStatus.Text = PlcSts;
            //LB_PLC_Connect.Text = cnnSts;
            //LB_Status.Text = connErr;
        }

        private void TB_IP_TextChanged(object sender, EventArgs e)
        {
            IPAdresiDegisti     = true;
            Thread thcon        = new Thread(CLS.SimConnection.StartConnection);
            thcon.Start();
        }

        private void LB_Status_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Thread thcon = new Thread(CLS.SimConnection.StartConnection);
            thcon.Start();
        }
    }
}
