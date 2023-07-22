using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Drawing;

namespace OplcE_Sim_Pro
{
   public class XMLFiles_RW
    {
      public  CLS CLS;
        



        #region XML Variable

        public int BarInput_Count;
        public int BarOutput_Count;


        public string q_xml_yolu;
        public string i_xml_yolu;
        //    public OpenFileDialog QxmlYoluSec = new OpenFileDialog();
        //    public OpenFileDialog IxmlYoluSec = new OpenFileDialog();

        XDocument IGuncelle;
        XElement Inode;

        XDocument Q_Yaz;
        XElement Qnode;

        XDocument ComWGuncelle;
        XElement ComWnode;

        XDocument ComR_Yaz;
        XElement ComRnode;


        public string Yol_XmlQ = "";
        public string Yol_XmlI = "";

        public string Yol_xmlComR = "";
        public string Yol_xmlComW = "";

        public string Okuuno;  // Kullanıcının yazdığı Veri Kullanıcı No
        public string Okuunm;  // Kullanıcının yazdığı Veri Kullanıcı Adı
        public string Okuups;  // Kullanıcının yazdığı Veri Kullanıcı Pass
        public string Okuups2; // Kullanıcının yazdığı Veri Kullanıcı pass2

        public string OkuUset;
        public string PLCTipiSecim; //300-400 / 1200-1500
        public string OkuBag;
        public string Okuip;

        public string Yazuno; //= "--";
        public string Yazunm; //= "--";
        public string Yazups; //= "--";
        public string Yazurst;
        public string Yazad;
        public string Yazdif;

        public string[] OkuInput0 = new string[8];
        public string[] OkuInput1 = new string[8];
        public string[] OkuInput2 = new string[8];
        public string[] OkuInput3 = new string[8];
        public string[] OkuAInput = new string[8];

        public string PlcStatus = "--";
        public string PlcConnectStatus = "--";
        public string PlcConnectError = "--";

        public string ListViewQ;
        public string ListViewI;
        bool xmlListeleriOK;
        #endregion


        public void XML_Olustur()
        {
           int set = 0;

           Yol_XmlI     = "C:\\i.xml";
           Yol_XmlQ     = "C:\\q.xml";
           Yol_xmlComR  = "C:\\comr.xml";
           Yol_xmlComW  = "C:\\comw.xml";


            switch (set)
            {
                case 0:
                    #region Q.xml Dosyası oluştur (Varsa mevcut dosyası sil yeni oluştur)
                    if (File.Exists(Yol_XmlQ))
                    {
                        File.Delete(Yol_XmlQ);
                        Thread.Sleep(100);
                    }

                    XmlTextWriter Q_xmlolustur = new XmlTextWriter(Yol_XmlQ, UTF8Encoding.UTF8);
                    // Dosyanın Kaydedilceği yer ve Dil Kodlaması
                    Q_xmlolustur.WriteStartDocument();// Element Oluşturma Başlangıcı
                    // xmlolustur.WriteComment("uzmanim.net"); // Açıklama Satırı Ekledik
                    Q_xmlolustur.WriteStartElement("ItemCollection");//item Etiketi ekledik.
                    Q_xmlolustur.WriteEndDocument();//Element Oluşturma işleminı sonlandırdık
                    Q_xmlolustur.Close();//Dosya Bağlantısını Kapatıyoruz..!

                    #endregion

                    goto case 1;
                case 1:
                    #region Q.xml dosyası içine digital ve analog outputları ekle ve kaydet
                    XmlDocument Q_doc = new XmlDocument();
                    Q_doc.Load(Yol_XmlQ);

                    XmlElement Crt_QQ = Q_doc.CreateElement("Q");
                    Crt_QQ.InnerText = "0";
                    Q_doc.DocumentElement.AppendChild(Crt_QQ);


                    XmlElement[] Crt_Q0 = new XmlElement[8];
                    XmlElement[] Crt_Q1 = new XmlElement[8];
                    XmlElement[] Crt_Q2 = new XmlElement[8];
                    XmlElement[] Crt_Q3 = new XmlElement[8];
                    XmlElement[] Crt_AQ = new XmlElement[8];


                    // XML listesi sıralı olması için for döngüleri iç içe yazılmıştır.
                    for (int i = 0; i < 8; i++) // Q0.0 - Q0.7
                    {
                        Crt_Q0[i] = Q_doc.CreateElement("Q0" + i);
                        Crt_Q0[i].InnerText = "false";
                        Q_doc.DocumentElement.AppendChild(Crt_Q0[i]);
                        if (i == 7)
                        {
                            for (int i1 = 0; i1 < 8; i1++) // Q1.0 - Q1.7
                            {
                                Crt_Q1[i1] = Q_doc.CreateElement("Q1" + i1);
                                Crt_Q1[i1].InnerText = "false";
                                Q_doc.DocumentElement.AppendChild(Crt_Q1[i1]);
                                if (i1 == 7)
                                {
                                    for (int i2 = 0; i2 < 8; i2++) // Q2.0 - Q2.7
                                    {
                                        Crt_Q2[i2] = Q_doc.CreateElement("Q2" + i2);
                                        Crt_Q2[i2].InnerText = "false";
                                        Q_doc.DocumentElement.AppendChild(Crt_Q2[i2]);
                                        if (i2 == 7)
                                        {
                                            for (int i3 = 0; i3 < 8; i3++) // Q3.0 - Q3.7
                                            {
                                                Crt_Q3[i3] = Q_doc.CreateElement("Q3" + i3);
                                                Crt_Q3[i3].InnerText = "false";
                                                Q_doc.DocumentElement.AppendChild(Crt_Q3[i3]);
                                                if (i3 == 7)
                                                {
                                                    for (int i4 = 0; i4 < 8; i4++) // AQ0 - AQ7
                                                    {
                                                        Crt_AQ[i4] = Q_doc.CreateElement("AQ" + i4);
                                                        Crt_AQ[i4].InnerText = "000";
                                                        Q_doc.DocumentElement.AppendChild(Crt_AQ[i4]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    XmlTextWriter Q_xmleEkle = new XmlTextWriter(Yol_XmlQ, null); // Fiziksel olarak kayıtları ekliyoruz
                    Q_xmleEkle.Formatting = Formatting.Indented;
                    Q_doc.WriteContentTo(Q_xmleEkle); // Kayıt başarıyla gerçekleştirildi
                    Q_xmleEkle.Close(); // Xml Dosyamızla bağlantımızı kesiyoruz..!

                    #endregion

                    goto case 2;
                case 2:
                    #region I.xml Dosyası oluştur (Varsa mevcut dosyası sil yeni oluştur)
                    if (File.Exists(Yol_XmlI))
                    {
                        File.Delete(Yol_XmlI);
                        Thread.Sleep(100);
                    }

                    XmlTextWriter I_xmlolustur = new XmlTextWriter(Yol_XmlI, UTF8Encoding.UTF8);
                    // Dosyanın Kaydedilceği yer ve Dil Kodlaması
                    I_xmlolustur.WriteStartDocument();// Element Oluşturma Başlangıcı
                    // xmlolustur.WriteComment("uzmanim.net"); // Açıklama Satırı Ekledik
                    I_xmlolustur.WriteStartElement("ItemCollection");//item Etiketi ekledik.
                    I_xmlolustur.WriteEndDocument();//Element Oluşturma işleminı sonlandırdık
                    I_xmlolustur.Close();//Dosya Bağlantısını Kapatıyoruz..!

                    #endregion

                    goto case 3;
                case 3:
                    #region I.xml dosyası içine digital ve analog outputları ekle ve kaydet
                    XmlDocument I_doc = new XmlDocument();
                    I_doc.Load(Yol_XmlI);

                    XmlElement Crt_II = I_doc.CreateElement("I");
                    Crt_II.InnerText = "0";
                    I_doc.DocumentElement.AppendChild(Crt_II);

                    XmlElement[] Crt_I0 = new XmlElement[8];
                    XmlElement[] Crt_I1 = new XmlElement[8];
                    XmlElement[] Crt_I2 = new XmlElement[8];
                    XmlElement[] Crt_I3 = new XmlElement[8];
                    XmlElement[] Crt_AI = new XmlElement[8];

                    // XML listesi sıralı olması için for döngüleri iç içe yazılmıştır.
                    for (int i = 0; i < 8; i++) // I0.0 - I0.7
                    {
                        Crt_I0[i] = I_doc.CreateElement("I0" + i);
                        Crt_I0[i].InnerText = "false";
                        I_doc.DocumentElement.AppendChild(Crt_I0[i]);

                        if (i == 7)
                        {
                            for (int i1 = 0; i1 < 8; i1++) // I1.0 - I1.7
                            {
                                Crt_I1[i1] = I_doc.CreateElement("I1" + i1);
                                Crt_I1[i1].InnerText = "false";
                                I_doc.DocumentElement.AppendChild(Crt_I1[i1]);
                                if (i1 == 7)
                                {
                                    for (int i2 = 0; i2 < 8; i2++) // I2.0 - I2.7
                                    {
                                        Crt_I2[i2] = I_doc.CreateElement("I2" + i2);
                                        Crt_I2[i2].InnerText = "false";
                                        I_doc.DocumentElement.AppendChild(Crt_I2[i2]);
                                        if (i2 == 7)
                                        {
                                            for (int i3 = 0; i3 < 8; i3++) // I3.0 - I3.7
                                            {
                                                Crt_I3[i3] = I_doc.CreateElement("I3" + i3);
                                                Crt_I3[i3].InnerText = "false";
                                                I_doc.DocumentElement.AppendChild(Crt_I3[i3]);
                                                if (i3 == 7)
                                                {
                                                    for (int i4 = 0; i4 < 8; i4++) // AI0 - AI7
                                                    {
                                                        Crt_AI[i4] = I_doc.CreateElement("AI" + i4);
                                                        Crt_AI[i4].InnerText = "000";
                                                        I_doc.DocumentElement.AppendChild(Crt_AI[i4]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    XmlTextWriter I_xmleEkle = new XmlTextWriter(Yol_XmlI, null); // Fiziksel olarak kayıtları ekliyoruz
                    I_xmleEkle.Formatting = Formatting.Indented;
                    I_doc.WriteContentTo(I_xmleEkle); // Kayıt başarıyla gerçekleştirildi
                    I_xmleEkle.Close(); // Xml Dosyamızla bağlantımızı kesiyoruz..!

                    #endregion

                    goto case 4;
                case 4:
                    #region COM_R.xml Dosyası oluştur (Varsa mevcut dosyası sil yeni oluştur)
                    if (File.Exists(Yol_xmlComR))
                    {
                        File.Delete(Yol_xmlComR);
                        Thread.Sleep(100);
                    }

                    XmlTextWriter ComR_xmlolustur = new XmlTextWriter(Yol_xmlComR, UTF8Encoding.UTF8);
                    // Dosyanın Kaydedilceği yer ve Dil Kodlaması
                    ComR_xmlolustur.WriteStartDocument();// Element Oluşturma Başlangıcı
                    // xmlolustur.WriteComment("uzmanim.net"); // Açıklama Satırı Ekledik
                    ComR_xmlolustur.WriteStartElement("ItemCollection");//item Etiketi ekledik.
                    ComR_xmlolustur.WriteEndDocument();//Element Oluşturma işleminı sonlandırdık
                    ComR_xmlolustur.Close();//Dosya Bağlantısını Kapatıyoruz..!
                    #endregion

                    goto case 5;
                case 5:
                    #region ComR.xml dosyası içine digital ve analog outputları ekle ve kaydet
                    XmlDocument ComR_doc = new XmlDocument();
                    ComR_doc.Load(Yol_xmlComR);

                    XmlElement Crt_ID = ComR_doc.CreateElement("ID");
                    Crt_ID.InnerText = "0";
                    ComR_doc.DocumentElement.AppendChild(Crt_ID);

                    XmlElement Crt_UID = ComR_doc.CreateElement("UID");
                    Crt_UID.InnerText = "0";
                    ComR_doc.DocumentElement.AppendChild(Crt_UID);

                    XmlElement Crt_ad = ComR_doc.CreateElement("ad");
                    Crt_ad.InnerText = "--";
                    ComR_doc.DocumentElement.AppendChild(Crt_ad);

                    XmlElement Crt_uno = ComR_doc.CreateElement("uno");
                    Crt_uno.InnerText = "--";
                    ComR_doc.DocumentElement.AppendChild(Crt_uno);

                    XmlElement Crt_unmsts = ComR_doc.CreateElement("unmsts");
                    Crt_unmsts.InnerText = "--";
                    ComR_doc.DocumentElement.AppendChild(Crt_unmsts);

                    XmlElement Crt_upssts = ComR_doc.CreateElement("upssts");
                    Crt_upssts.InnerText = "--";
                    ComR_doc.DocumentElement.AppendChild(Crt_upssts);

                    XmlElement Crt_urst = ComR_doc.CreateElement("urst");
                    Crt_urst.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_urst);

                    XmlElement Crt_dif = ComR_doc.CreateElement("dif");
                    Crt_dif.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_dif);

                    XmlElement Crt_conerr = ComR_doc.CreateElement("conerr");
                    Crt_conerr.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_conerr);

                    XmlElement Crt_con = ComR_doc.CreateElement("con");
                    Crt_con.InnerText = "--";
                    ComR_doc.DocumentElement.AppendChild(Crt_con);

                    XmlElement Crt_plc = ComR_doc.CreateElement("plc");
                    Crt_plc.InnerText = "--";
                    ComR_doc.DocumentElement.AppendChild(Crt_plc);

                    XmlElement Crt_SV1_Poz = ComR_doc.CreateElement("SV1Poz");
                    Crt_SV1_Poz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV1_Poz);

                    XmlElement Crt_SV1_Hiz = ComR_doc.CreateElement("SV1Hiz");
                    Crt_SV1_Hiz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV1_Hiz);

                    XmlElement Crt_SV1_Sstill = ComR_doc.CreateElement("SV1Sstill");
                    Crt_SV1_Sstill.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV1_Sstill);

                    XmlElement Crt_SV1_ERR = ComR_doc.CreateElement("SV1Err");
                    Crt_SV1_ERR.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV1_ERR);


                    XmlElement Crt_SV2_Poz = ComR_doc.CreateElement("SV2Poz");
                    Crt_SV2_Poz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV2_Poz);

                    XmlElement Crt_SV2_Hiz = ComR_doc.CreateElement("SV2Hiz");
                    Crt_SV2_Hiz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV2_Hiz);

                    XmlElement Crt_SV2_Sstill = ComR_doc.CreateElement("SV2Sstill");
                    Crt_SV2_Sstill.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV2_Sstill);

                    XmlElement Crt_SV2_ERR = ComR_doc.CreateElement("SV2Err");
                    Crt_SV2_ERR.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV2_ERR);


                    XmlElement Crt_SV3_Poz = ComR_doc.CreateElement("SV3Poz");
                    Crt_SV3_Poz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV3_Poz);

                    XmlElement Crt_SV3_Hiz = ComR_doc.CreateElement("SV3Hiz");
                    Crt_SV3_Hiz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV3_Hiz);

                    XmlElement Crt_SV3_Sstill = ComR_doc.CreateElement("SV3Sstill");
                    Crt_SV3_Sstill.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV3_Sstill);

                    XmlElement Crt_SV3_ERR = ComR_doc.CreateElement("SV3Err");
                    Crt_SV3_ERR.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV3_ERR);


                    XmlElement Crt_SV4_Poz = ComR_doc.CreateElement("SV4Poz");
                    Crt_SV4_Poz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV4_Poz);

                    XmlElement Crt_SV4_Hiz = ComR_doc.CreateElement("SV4Hiz");
                    Crt_SV4_Hiz.InnerText = "000";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV4_Hiz);

                    XmlElement Crt_SV4_Sstill = ComR_doc.CreateElement("SV4Sstill");
                    Crt_SV4_Sstill.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV4_Sstill);

                    XmlElement Crt_SV4_ERR = ComR_doc.CreateElement("SV4Err");
                    Crt_SV4_ERR.InnerText = "false";
                    ComR_doc.DocumentElement.AppendChild(Crt_SV4_ERR);

                    XmlTextWriter ComR_xmleEkle = new XmlTextWriter(Yol_xmlComR, null); // Fiziksel olarak kayıtları ekliyoruz
                    ComR_xmleEkle.Formatting = Formatting.Indented;
                    ComR_doc.WriteContentTo(ComR_xmleEkle); // Kayıt başarıyla gerçekleştirildi
                    ComR_xmleEkle.Close(); // Xml Dosyamızla bağlantımızı kesiyoruz..!

                    #endregion

                    goto case 6;
                case 6:
                    #region COM_W.xml Dosyası oluştur (Varsa mevcut dosyası sil yeni oluştur)
                    if (File.Exists(Yol_xmlComW))
                    {
                        File.Delete(Yol_xmlComW);
                        Thread.Sleep(100);
                    }

                    XmlTextWriter ComW_xmlolustur = new XmlTextWriter(Yol_xmlComW, UTF8Encoding.UTF8);
                    // Dosyanın Kaydedilceği yer ve Dil Kodlaması
                    ComW_xmlolustur.WriteStartDocument();// Element Oluşturma Başlangıcı
                    // xmlolustur.WriteComment("uzmanim.net"); // Açıklama Satırı Ekledik
                    ComW_xmlolustur.WriteStartElement("ItemCollection");//item Etiketi ekledik.
                    ComW_xmlolustur.WriteEndDocument();//Element Oluşturma işleminı sonlandırdık
                    ComW_xmlolustur.Close();//Dosya Bağlantısını Kapatıyoruz..!
                    #endregion


                    goto case 7;
                case 7:
                    #region ComW.xml dosyası içine digital ve analog outputları ekle ve kaydet
                    XmlDocument ComW_doc = new XmlDocument();
                    ComW_doc.Load(Yol_xmlComW);

                    XmlElement Crt_input_ID0 = ComW_doc.CreateElement("ID");
                    Crt_input_ID0.InnerText = "0";
                    ComW_doc.DocumentElement.AppendChild(Crt_input_ID0);

                    XmlElement Crt_unm = ComW_doc.CreateElement("unm");
                    Crt_unm.InnerText = "--";
                    ComW_doc.DocumentElement.AppendChild(Crt_unm);

                    XmlElement Crt_ups = ComW_doc.CreateElement("ups");
                    Crt_ups.InnerText = "--";
                    ComW_doc.DocumentElement.AppendChild(Crt_ups);

                    XmlElement Crt_uset = ComW_doc.CreateElement("uset");
                    Crt_uset.InnerText = "false";
                    ComW_doc.DocumentElement.AppendChild(Crt_uset);

                    XmlElement Crt_bag = ComW_doc.CreateElement("bag");
                    Crt_bag.InnerText = "false";
                    ComW_doc.DocumentElement.AppendChild(Crt_bag);

                    XmlElement Crt_tip = ComW_doc.CreateElement("tip");
                    Crt_tip.InnerText = "0";
                    ComW_doc.DocumentElement.AppendChild(Crt_tip);

                    XmlElement Crt_ip = ComW_doc.CreateElement("ip");
                    Crt_ip.InnerText = "192.168.0.1";
                    ComW_doc.DocumentElement.AppendChild(Crt_ip);

                    XmlTextWriter ComW_xmleEkle = new XmlTextWriter(Yol_xmlComW, null); // Fiziksel olarak kayıtları ekliyoruz
                    ComW_xmleEkle.Formatting = Formatting.Indented;
                    ComW_doc.WriteContentTo(ComW_xmleEkle); // Kayıt başarıyla gerçekleştirildi
                    ComW_xmleEkle.Close(); // Xml Dosyamızla bağlantımızı kesiyoruz..!

                    #endregion


                    goto case 8;
                case 8:

                 

                    goto case 9;
                case 9:


                    goto case 10;
                case 10:

                    // Thread.Sleep(200);
                    xmlListeleriOK = true;
                    CLS.Var.CheckBox_Seri();
                    goto case 50;
                case 50:

                    break;
            }
        }
        public void XML_YAZ()
        {
            if (xmlListeleriOK)
            {
                try
                {
                    Q_Yaz       = XDocument.Load(Yol_XmlQ);
                    Qnode       = Q_Yaz.Elements("ItemCollection").FirstOrDefault(a => a.Element("Q").Value.Trim() == "0");
                    ComR_Yaz    = XDocument.Load(Yol_xmlComR);
                    ComRnode    = ComR_Yaz.Elements("ItemCollection").FirstOrDefault(a => a.Element("ID").Value.Trim() == "0");

                    for (int i = 0; i < 8; i++)
                    {
                        CLS.Form1.XML_SignalOutput.Text = i.ToString();
                        BarOutput_Count = i;

                        Qnode.SetElementValue("Q0" + i, CLS.Var.Output0[i].Checked);
                        Qnode.SetElementValue("Q1" + i, CLS.Var.Output1[i].Checked);
                        Qnode.SetElementValue("Q2" + i, CLS.Var.Output2[i].Checked);
                        Qnode.SetElementValue("Q3" + i, CLS.Var.Output3[i].Checked);
                        Qnode.SetElementValue("AQ" + i, CLS.Var.LB_AOutput[i].Text);
                        ListViewQ = Q_Yaz.ToString();

                        ComRnode.SetElementValue("con", CLS.Form1.LB_Status.Text);
                        ComRnode.SetElementValue("plc", CLS.Form1.Lb_PLCMode.Text);

                        ComRnode.SetElementValue("SV1Poz", CLS.Form1.LB_Servo1_Poz.Text);
                        ComRnode.SetElementValue("SV1Hiz", CLS.Form1.LB_Servo1_Hiz.Text);
                        ComRnode.SetElementValue("SV1Sstill", CLS.Form1.CHB_Servo1_Standstill.Checked);
                        ComRnode.SetElementValue("SV1Err", CLS.Form1.CHB_Servo1_ERR.Checked);

                        ComRnode.SetElementValue("SV2Poz", CLS.Form1.LB_Servo2_Poz.Text);
                        ComRnode.SetElementValue("SV2Hiz", CLS.Form1.LB_Servo2_Hiz.Text);
                        ComRnode.SetElementValue("SV2Sstill", CLS.Form1.CHB_Servo2_Standstill.Checked);
                        ComRnode.SetElementValue("SV2Err", CLS.Form1.CHB_Servo2_ERR.Checked);

                        ComRnode.SetElementValue("SV3Poz", CLS.Form1.LB_Servo3_Poz.Text);
                        ComRnode.SetElementValue("SV3Hiz", CLS.Form1.LB_Servo3_Hiz.Text);
                        ComRnode.SetElementValue("SV3Sstill", CLS.Form1.CHB_Servo3_Standstill.Checked);
                        ComRnode.SetElementValue("SV3Err", CLS.Form1.CHB_Servo3_ERR.Checked);

                        ComRnode.SetElementValue("SV4Poz", CLS.Form1.LB_Servo4_Poz.Text);
                        ComRnode.SetElementValue("SV4Hiz", CLS.Form1.LB_Servo4_Hiz.Text);
                        ComRnode.SetElementValue("SV4Sstill", CLS.Form1.CHB_Servo4_Standstill.Checked);
                        ComRnode.SetElementValue("SV4Err", CLS.Form1.CHB_Servo4_ERR.Checked);


                        if (i == 0 && i == 3)
                        {
                            ComR_Yaz.Save(Yol_xmlComR);
                        }


                        if (i == 7)
                        {
                            if (CLS.Form1.XML_SignalOutput.BackColor == SystemColors.Control) { CLS.Form1.XML_SignalOutput.BackColor = Color.Green; }
                            else { CLS.Form1.XML_SignalOutput.BackColor = SystemColors.Control; }

                            try
                            {
                                Q_Yaz.Save(Yol_XmlQ);
                                ComR_Yaz.Save(Yol_xmlComR);

                                //CLS.Form1.XML_SignalOutput.BackColor = Color.Lime;
                            }
                            catch (Exception HATA)
                            {
                            }
                            Thread.Sleep(2);
                            i = -1;
                        }


                    }
                }
                catch (Exception HATA)
                {
                }
              
            }

        }


public void XML_OKU()
        {
            if (xmlListeleriOK)
            {

          
                    for (int i = 0; i < 8; i++)
                    {
                    try
                    {

                        IGuncelle = XDocument.Load(Yol_XmlI);
                        Inode = IGuncelle.Elements("ItemCollection").FirstOrDefault(a => a.Element("I").Value.Trim() == "0");

                        CLS.Var.Input0[i].Checked = bool.Parse(Inode.Element("I0" + i).Value.ToString());
                        CLS.Var.Input1[i].Checked = bool.Parse(Inode.Element("I1" + i).Value.ToString());
                        CLS.Var.Input2[i].Checked = bool.Parse(Inode.Element("I2" + i).Value.ToString());
                        CLS.Var.Input3[i].Checked = bool.Parse(Inode.Element("I3" + i).Value.ToString());
                        CLS.Var.LB_AInput[i].Text = Inode.Element("AI" + i).Value.ToString();
                        //OkuInput0[i] = Inode.Element("I0" + i).Value.ToString();
                        //OkuInput1[i] = Inode.Element("I1" + i).Value.ToString();
                        //OkuInput2[i] = Inode.Element("I2" + i).Value.ToString();
                        //OkuInput3[i] = Inode.Element("I3" + i).Value.ToString();
                        //OkuAInput[i] = Inode.Element("AI" + i).Value.ToString();

                        BarInput_Count = i;
                        ListViewI = IGuncelle.ToString();

                        if (i == 7)
                        {
                            //  Okuuno = Inode.Element("uno").Value.ToString();
                            //Okuunm = Inode.Element("unm").Value.ToString();
                            //Okuups = Inode.Element("ups").Value.ToString();
                            //OkuUset = Inode.Element("uset").Value.ToString();
                            //OkuTip = Inode.Element("tip").Value.ToString();
                            //OkuBag = Inode.Element("bag").Value.ToString();
                            //Okuip = Inode.Element("ip").Value.ToString();
                            if (CLS.Form1.XML_SignalInput.BackColor == SystemColors.Control) { CLS.Form1.XML_SignalInput.BackColor = Color.Green; }
                            else { CLS.Form1.XML_SignalInput.BackColor = SystemColors.Control; }

                            if (CLS.Form1.CHB_ManCheck.Checked != true)
                            { 
                            ComWGuncelle = XDocument.Load(Yol_xmlComW);
                            ComWnode = ComWGuncelle.Elements("ItemCollection").FirstOrDefault(a => a.Element("ID").Value.Trim() == "0");

                            CLS.Form1.TB_IP.Text                = ComWnode.Element("ip").Value.ToString();
                                if (ComWnode.Element("tip").Value.ToString() == "0")
                                {
                                    CLS.Form1.RB_S7300_400.Checked = true;
                                }
                                else
                                {
                                    CLS.Form1.RB_S71200_1500.Checked = true;
                                }
                              CLS.Form1.CHB_BaglanPLC.Checked     = bool.Parse(ComWnode.Element("bag").Value.ToString());
                            }



                            Thread.Sleep(5);
                            i = -1;

                        }
                    }
                    catch (Exception HATA)
                    {
                        //    ConnOK = false;
                        i = -1;
                    }

                    }



            }
        }


    }
}
