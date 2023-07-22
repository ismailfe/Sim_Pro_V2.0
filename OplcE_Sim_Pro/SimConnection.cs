using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PlcsimS7online;
using System.Net;
using System.ServiceProcess;
using TCPCon;
using System.Net.Sockets;
using System.Threading;
using System.Media;

namespace OplcE_Sim_Pro
{
    public class SimConnection
    {
       //public Form1 Form;// = new Form1();

        public CLS CLS;
        public string PLCIP;          //= "192.168.0.11";
        public string NETWORKIP     = "127.1.1.1";
        public string PLCRACK       = "0";
        public string PLCSLOT       = "2";  // s7300/400= 2   S71200/1500=1
        public string PortStatus;
        public string PortAktivite;
        public string PortAktiviteSonuc;

        #region NetPlcSim VARIABLE

        Config MainAyar = new Config();
        List<IsoToS7online> MainServer = new List<IsoToS7online>();

        string MainAyarAdi = "";
        bool MainS7DOSService_Dur = false;
        string MainIepHServisAdi = String.Empty;

        CmdLineArgs NesneBasla = new CmdLineArgs();
        public StationData Station = new StationData();

        //port102
        public bool Success { get; private set; }
        public bool AutoCloseOnSuccess { get; set; }

        private string m_Servicename;
        private TcpListener m_Listener;

        #endregion



        public void StartConnection()

        {
            int sw = 0;
            switch (sw)
            {
                case 0: // First Load
                    m_Servicename               = Tools.GetS7DOSHelperServiceName();
                    MainIepHServisAdi           = Tools.GetS7DOSHelperServiceName();
                    NesneBasla.parseCmdLineArgs(Environment.GetCommandLineArgs());

                    goto case 1;
                case 1:
                    S7ServisStop();
                    ServerStop();
                    StartGetPortBack();

                    goto case 2;
                case 2:

                    // YENI SERVER EKLEME
                    AddNewStation();

                    goto case 3;
                case 3:

                    ServerStart();
                    S7ServisStart();
                    goto case 4;
                case 4:


                    goto case 5;
                case 5:

                    break;
            }


        }

        public void FirstLoad()
        {
            m_Servicename               = Tools.GetS7DOSHelperServiceName();
            MainIepHServisAdi           = Tools.GetS7DOSHelperServiceName();
            NesneBasla.parseCmdLineArgs(Environment.GetCommandLineArgs());
            // YENI SERVER EKLEME
            AddNewStation();
        }

        public void AddNewStation()
        {
            PLCIP = CLS.Form1.TB_IP.Text;
            InitStationDataGridView();
            Station.Name                = "PLC001";
            Station.NetworkIpAddress    = IPAddress.Parse(NETWORKIP); // 127.1.1.1
            Station.PlcsimIpAddress     = IPAddress.Parse(PLCIP);
            Station.PlcsimRackNumber    = int.Parse(PLCRACK);
            Station.PlcsimSlotNumber    = int.Parse(PLCSLOT);
            Station.TsapCheckEnabled    = false;
           
            StationData stationData = new StationData(Station.Name, Station.NetworkIpAddress, Station.PlcsimIpAddress,
            Station.PlcsimRackNumber, Station.PlcsimSlotNumber, Station.TsapCheckEnabled);

            MainAyar.Stations.Add(stationData);
        }



        private void InitStationDataGridView()
        {
            if (CLS.Form1.Dg_istasyon.Columns.Count > 0)
            {
                CLS.Form1.Dg_istasyon.Columns.Clear();
            }

            if (CLS.Form1.Dg_istasyon.Rows.Count > 0)
            {
                CLS.Form1.Dg_istasyon.Rows.Clear();
            }

            CLS.Form1.Dg_istasyon.AutoGenerateColumns = false;

            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.DataPropertyName = "Name";
            nameColumn.HeaderText = "PLC Count";
            nameColumn.Width = 110;

            DataGridViewTextBoxColumn netipColumn = new DataGridViewTextBoxColumn();
            netipColumn.DataPropertyName = "NetworkIpAddress";
            netipColumn.HeaderText = "Network Address";
            netipColumn.Width = 120;

            DataGridViewTextBoxColumn plcsimipColumn = new DataGridViewTextBoxColumn();
            plcsimipColumn.DataPropertyName = "PlcsimIpAddress";
            plcsimipColumn.HeaderText = "Plcsim Address";
            plcsimipColumn.Width = 120;

            DataGridViewTextBoxColumn rackSlotColumn = new DataGridViewTextBoxColumn();
            rackSlotColumn.DataPropertyName = "PlcsimRackSlot";
            rackSlotColumn.HeaderText = "PLC Rack-Slot";
            rackSlotColumn.Width = 30;

            DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn();
            statusColumn.DataPropertyName = "Status";
            statusColumn.HeaderText = "Connect Status";
            statusColumn.Width = 100;

            CLS.Form1.Dg_istasyon.Columns.Add(nameColumn);
            CLS.Form1.Dg_istasyon.Columns.Add(netipColumn);
            CLS.Form1.Dg_istasyon.Columns.Add(plcsimipColumn);
            CLS.Form1.Dg_istasyon.Columns.Add(rackSlotColumn);
            CLS.Form1.Dg_istasyon.Columns.Add(statusColumn);

            CLS.Form1.Dg_istasyon.DataSource = MainAyar.Stations;
        }
     
        string ServerStartKont;

        public void ServerStart()
        {

            if (ServerStartKont == "RUNNING")
            {
                ServerStop();
            }


            foreach (StationData station in MainAyar.Stations)
            {
                List<byte[]> tsaps = new List<byte[]>();
                byte tsap2 = (byte)(((station.PlcsimRackNumber << 4) | station.PlcsimSlotNumber));
                tsaps.Add(new byte[] { 0x01, tsap2 });
                tsaps.Add(new byte[] { 0x02, tsap2 });
                tsaps.Add(new byte[] { 0x03, tsap2 });

                IsoToS7online srv = new IsoToS7online(station.TsapCheckEnabled);
                MainServer.Add(srv);
                try
                {
                    srv.start(station.Name, station.NetworkIpAddress, tsaps, station.PlcsimIpAddress, station.PlcsimRackNumber, station.PlcsimSlotNumber);
                }
                catch
                {                   //"Error starting server on connection "
                    MessageBox.Show("Server başlarken bir hata oluştu! " + station.Name, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    station.Connected = false;
                    station.Status = StationStatus.ERROR.ToString();
                }
                station.Connected = true;
                station.Status = StationStatus.RUNNING.ToString();
                ServerStartKont = StationStatus.RUNNING.ToString();
            }

        }

        public void ServerStop()
        {
            foreach (IsoToS7online srv in MainServer)
            {
                srv.stop();
            }
            MainServer.Clear();

            foreach (StationData station in MainAyar.Stations)
            {
                station.Status = StationStatus.STOPPED.ToString();
            }

        }

        public void S7ServisStop()
        {
            if (!(String.IsNullOrEmpty(MainIepHServisAdi)))
            {
                if (Tools.StopService(MainIepHServisAdi, 17000, true) == true)
                {
                    MainS7DOSService_Dur = true;
                }
            }

        }

        public void S7ServisStart()
        {
            if (!(String.IsNullOrEmpty(MainIepHServisAdi)))
            {
                if (Tools.StartService(MainIepHServisAdi, 17000, true, false) == true)
                {
                    MainS7DOSService_Dur = false;
                }
            }



        }

        private bool Step1()
        {
            return Tools.StopService(m_Servicename, 170000, true);
        }

        private bool Step2()
        {
            try
            {
                m_Listener = new TcpListener(IPAddress.Any, 102);
                m_Listener.Start();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool Step3()
        {
            return Tools.StartService(m_Servicename, 17000, true, false);
        }

        private bool Step4()
        {
            try
            {
                m_Listener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool Step5()
        {
            return Tools.IsTcpPortAvailable(102);
        }

        public void StartGetPortBack()
        {

            m_Servicename = Tools.GetS7DOSHelperServiceName();

            CLS.Form1.progressBar1.Maximum = 10;
            CLS.Form1.progressBar1.Step = 1;
            this.Success = false;
            PortAktivite = "";
            PortAktivite = "Port 102 Geri alımı başlıyor...";//"Starting to get Port 102 back...";
            CLS.Form1.Lb_PortStatus.Text = PortAktivite;
            CLS.Form1.progressBar1.PerformStep();
            Thread.Sleep(20);


            PortAktivite = "Mevcut Servis durduruluyor :" + m_Servicename + " ...";//"Step 1) Stopping service '" + m_Servicename + "'...";
            CLS.Form1.Lb_PortStatus.Text = PortAktivite;
            CLS.Form1.progressBar1.PerformStep();
            Thread.Sleep(20);

            if (Step1())
            {

                PortAktivite = "Port 102 ile bağlantı için Server başlatılıyor.";// "Step 2) Starting our own Server on TCP Port 102...";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                if (Step2() == false)
                {
                    SystemSounds.Exclamation.Play();

                    PortAktivite += "HATA";// "FAILED!";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    CLS.Form1.progressBar1.Value = 0;
                    return;
                }
                PortAktivite += "OK.";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                PortAktivite = "Servis Başlıyor..." + m_Servicename;// "Step 3) Starting service '" + m_Servicename + "'...";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                if (Step3())
                {
                    PortAktivite += "OK.";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    CLS.Form1.progressBar1.PerformStep();
                    Thread.Sleep(20);
                }
                else
                {
                    Step4(); // stops the previous started TCP server
                    SystemSounds.Exclamation.Play();

                    PortAktivite += "HATA! Servis başlarken bir problem oluştu.";// "FAILED to start the service!";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    PortAktiviteSonuc = "Servisi Durdururken ''Simülasyon, " + System.Environment.NewLine + "'' uygulamasını YÖNETİCİ OLARAK çalıştırmayı unutmayın.";  //"Remember that you need to start NetToPLCsim" + System.Environment.NewLine + "with administrative rights to stop the service!";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    CLS.Form1.progressBar1.Value = 0;
                    return;
                }
                PortAktivite += "OK.";

                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                PortAktivite = "Simülasyon Connection Server durduruluyor!";////"Step 4) Stopping our own Server...";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                if (Step4() == false)
                {
                    SystemSounds.Exclamation.Play();

                    PortAktivite += "Hata";//"FAILED!";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    CLS.Form1.progressBar1.Value = 0;
                    return;
                }
                PortAktivite += "OK.";

                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                PortAktivite = "TCP Port 102 Kontrol ediliyor...";// "Step 5) Checking TCP Port 102...";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                CLS.Form1.progressBar1.PerformStep();
                Thread.Sleep(20);
                if (Step5())
                {
                    PortAktivite += "Port 102 uygun durumda!"; // "OK. Port 102 is available.";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    PortAktiviteSonuc = "İşlem tamamlandı. PLC simülasyonu ile bağlantı kurulabilir. "; // "Success! You are ready to use NetToPLCsim :)";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    this.Success = true;
                }
                else
                {
                    SystemSounds.Exclamation.Play();

                    PortAktivite += "HATA! Port 102 kullanıma uygun değil...";// "FAILED. Port 102 is still not available :(";
                    CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                    CLS.Form1.progressBar1.Value = 0;
                }
            }
            else
            {
                SystemSounds.Exclamation.Play();

                PortAktivite += "HATA! Servis durdurulurken bir problem oluştu."; // "FAILED to stop the service!";
                PortAktiviteSonuc = "Servisi Durdururken ''Simülasyon, " + System.Environment.NewLine + "'' uygulamasını YÖNETİCİ OLARAK çalıştırmayı unutmayın."; //"Remember that you need to start NetToPLCsim" "with administrative rights to stop the service!";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
                CLS.Form1.progressBar1.Value = 0;
            }


            if (Tools.IsTcpPortAvailable(102) == true)
            {
                PortStatus = "Port 102 OK";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
            }
            else
            {
                PortStatus = "Port 102 not available!";
                CLS.Form1.Lb_PortStatus.Text = PortAktivite;
            }

        }







    }
}
