using System;
using System.Windows.Forms;
using System.IO;

namespace OplcE_Sim_Pro
{
    public enum eAutoStopService { YES, NO, ASK };
    public enum eAutoStart {YES, NO};

    class CmdLineArgs
    {
        private bool m_ArgsGiven;
        private string m_StartIni;
        private eAutoStopService m_AutoStopService;
        private eAutoStart m_AutoStart;

        public CmdLineArgs()
        {
            setDefaults();
        }

        private void setDefaults()
        {
            m_ArgsGiven = false;
            m_StartIni = String.Empty;
            m_AutoStopService = eAutoStopService.ASK;
            m_AutoStart = eAutoStart.NO;
        }

        public bool ArgsGiven
        {
            get { return m_ArgsGiven; }
        }

        public eAutoStopService AutoStopService
        {
            get { return m_AutoStopService; }
        }

        public eAutoStart AutoStart
        {
            get { return m_AutoStart; }
        }

        public string StartIni
        {
            get { return m_StartIni; }
        }

        public void parseCmdLineArgs(string[] args)
        {
            int i = 0;
            string opt;

            foreach (string arg in args)
            {
                i++;
                if (i == 1) continue;

                if (arg == "-help" || arg == "--help" || arg == "-?")
                {
                    MessageBox.Show(getHelpText(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (arg.StartsWith("-s="))
                {
                    opt = arg.Substring(3, arg.Length - 3).ToUpper();
                    if (opt == "YES")
                    {
                        m_AutoStopService = eAutoStopService.YES;
                    }
                    else if (opt == "NO")
                    {
                        m_AutoStopService = eAutoStopService.NO;
                    }
                }
                else if (arg == "-autostart")
                {
                    m_AutoStart = eAutoStart.YES;
                }
                else if ((arg.StartsWith("-f=")) || (i == 2))
                {
                    if (arg.StartsWith("-f="))
                    {
                        opt = arg.Substring(3, arg.Length - 3);
                        m_StartIni = opt;
                    }
                    else if (i == 2) 
                    {
                        opt = arg.ToUpper();
                        if (opt.EndsWith(".INI"))
                        {
                            m_StartIni = arg;
                        }
                    }

                    if (m_StartIni.StartsWith(System.Environment.CurrentDirectory) == false)
                    {
                        m_StartIni = System.Environment.CurrentDirectory + "\\" + m_StartIni;
                    }
                    if (File.Exists(m_StartIni) == false)
                    {
                        m_StartIni = String.Empty;
                    }
                }
            }

            if (m_StartIni == String.Empty)              {
                m_AutoStart = eAutoStart.NO;
            }
        }

        public string getHelpText()
        {
            string text =
                "NetToPLCSim - A network interface to Plcsim." + Environment.NewLine +
                Environment.NewLine +
                "Command line options:" + Environment.NewLine +
                Environment.NewLine +
                "NetToPLCSim.exe [configuration.ini] [-f=configuration.ini] [-s=Option] [-autostart]" +
                Environment.NewLine +
                "Options:" + Environment.NewLine +
                "-f=configuration.ini\tStart with this station configuration" + Environment.NewLine +
                "-s=Option\t\tAutostop IEPG-Helper service" + Environment.NewLine +
                "\t\tOptions: NO, YES, ASK" + Environment.NewLine +
                "-autostart\t\tAutostart with configuration file";

            return text;
        }
    }
}
