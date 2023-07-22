using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OplcE_Sim_Pro
{
  public class FirstStart
    {
        public CLS CLS = new CLS();

        public void Starting()
        {
            int sw = 0;


            switch (sw)
            {
                case 0:



                    goto case 1;
                case 1:

                    XML_RW_Start();

                    goto case 2;
                case 2:

                    Thread thcon = new Thread(CLS.SimConnection.StartConnection);
                    thcon.Start();
                    goto case 3;
                case 3:


                    goto case 4;
                case 4:


                    goto case 5;
                case 5:


                    goto case 50;
                case 50:
                    break;
            }

        }



        //void FirstStart()
        //{
        //    //SerCon.MySqlConnect();
        //    //if (SimTab.Yazad != "") { SimTab.Yazad = "--"; }
        //    //if (SimTab.Yazuno != "") { SimTab.Yazuno = "--"; }
        //    //if (SimTab.Yazunm != "") { SimTab.Yazunm = "--"; }
        //    //if (SimTab.Yazups != "") { SimTab.Yazups = "--"; }
        //    //if (SimTab.Yazurst != "") { SimTab.Yazurst = "false"; }
        //    //if (SimTab.Yazdif != "") { SimTab.Yazdif = "false"; }
        //}


        void XML_RW_Start()
        {
            CLS.XMLFiles_RW.XML_Olustur();

            CLS.Var.Th_XML_Yaz = new Thread(CLS.XMLFiles_RW.XML_YAZ);
            CLS.Var.Th_XML_Yaz.Start();

            CLS.Var.Th_XML_Oku = new Thread(CLS.XMLFiles_RW.XML_OKU);
            CLS.Var.Th_XML_Oku.Start();
        }



    }
}
