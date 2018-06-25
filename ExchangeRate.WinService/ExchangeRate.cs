using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using ExchangeRate.Model;
using System.Xml;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Net;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Globalization;

namespace ExchangeRate.WinService
{
    public partial class ExchangeRate : ServiceBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("databaseLogger");
        public ExchangeRate()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");        
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");     
            Timer _timer = new Timer();     
            logger.Info("Servis çalıştı."); 

            _timer.Interval = Convert.ToDouble(ConfigurationSettings.AppSettings["TimeInterval"]);  
            _timer.Elapsed += new ElapsedEventHandler(timeElapsed);
            _timer.Start();  


        }
        private void timeElapsed(object sender, ElapsedEventArgs args)

        {
            string localTime = String.Format("{0:00}", DateTime.Now.Hour) + ":" + String.Format("{0:00}", DateTime.Now.Minute); 
            string updateDate = ConfigurationSettings.AppSettings["StartingHour"]; 
            try
            {
                if (localTime == updateDate) 
                {
                    logger.Info("Yetki zamanı...");  
                    GetExchangeRate();     

                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Hata!", ex); 
            }
        }
        public static void GetExchangeRate()
        {
            try
            {
                Uri uri = new Uri("http://www.tcmb.gov.tr/kurlar/today.xml");


                DataSet ds = GetTCMBKur(uri);
                if (ds != null)
                {
                    DovizKurlari kur = new DovizKurlari();
                    kur.Tarih_Date = new Tarih_Date();
                    kur.Tarih_Date.Tarih = Convert.ToDateTime(ds.Tables[0].Rows[0]["Tarih"]);
                    kur.Tarih_Date.Date = ds.Tables[0].Rows[0]["Date"].ToString();
                    kur.Tarih_Date.Bulten_No = ds.Tables[0].Rows[0]["Bulten_No"].ToString();
                    kur.Tarih_Date.Currency = new List<Currency>();
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        Currency data = new Currency();
                        data.BanknoteBuying = (dr["BanknoteBuying"].ToString() == "" ? default(decimal?) : Convert.ToDecimal(dr["BanknoteBuying"].ToString().Replace(".", ",")));
                        data.BanknoteSelling = (dr["BanknoteSelling"].ToString() == "" ? default(decimal?) : Convert.ToDecimal(dr["BanknoteSelling"].ToString().Replace(".", ",")));
                        data.CrossOrder = Convert.ToDecimal(dr["CrossOrder"].ToString().Replace(".", ","));
                        data.CrossRateOther = (dr["CrossRateOther"].ToString() == "" ? default(decimal?) : Convert.ToDecimal(dr["CrossRateOther"].ToString().Replace(".", ",")));
                        data.CrossRateUSD = (dr["CrossRateUSD"].ToString() == "" ? default(decimal?) : Convert.ToDecimal(dr["CrossRateUSD"].ToString().Replace(".", ",")));
                        data.CurrencyCode = dr["CurrencyCode"].ToString();
                        data.CurrencyName = dr["CurrencyName"].ToString();
                        data.ForexBuying = (dr["ForexBuying"].ToString() == "" ? default(decimal?) : Convert.ToDecimal(dr["ForexBuying"].ToString().Replace(".", ",")));
                        data.ForexSelling = (dr["ForexSelling"].ToString() == "" ? default(decimal?) : Convert.ToDecimal(dr["ForexSelling"].ToString().Replace(".", ",")));
                        data.Kod = dr["Kod"].ToString();
                        data.Unit = Convert.ToDecimal(dr["Unit"].ToString());
                        kur.Tarih_Date.Currency.Add(data);
                    }
                    SaveCurrency(kur);
                }
                else
                {
                    logger.Error("Merkez bankasından kur alınamadı.");
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Hata", ex);
            }

        }
        private static void SaveCurrency(DovizKurlari kur)
        {
            foreach (Currency item in kur.Tarih_Date.Currency)
            {
                string sConn = System.Configuration.ConfigurationManager.ConnectionStrings["NLog"].ConnectionString;
                SqlConnection oConnection = new SqlConnection(sConn);
                oConnection.Open();
                SqlCommand oCommand = new SqlCommand("SP_InsertCurrency", oConnection);
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add(new SqlParameter("@BanknoteBuying", SqlDbType.Decimal));
                oCommand.Parameters["@BanknoteBuying"].Value = (item.BanknoteBuying == null ? DBNull.Value : (object)item.BanknoteBuying);
                oCommand.Parameters.Add(new SqlParameter("@BanknoteSelling", SqlDbType.Decimal));
                oCommand.Parameters["@BanknoteSelling"].Value = (item.BanknoteSelling == null ? DBNull.Value : (object)item.BanknoteSelling);
                oCommand.Parameters.Add(new SqlParameter("@CrossOrder", SqlDbType.Decimal));
                oCommand.Parameters["@CrossOrder"].Value = (item.CrossOrder == null ? DBNull.Value : (object)item.CrossOrder);
                oCommand.Parameters.Add(new SqlParameter("@CrossRateOther", SqlDbType.Decimal));
                oCommand.Parameters["@CrossRateOther"].Value = (item.CrossRateOther == null ? DBNull.Value : (object)item.CrossRateOther);
                oCommand.Parameters.Add(new SqlParameter("@CrossRateUSD", SqlDbType.Decimal));
                oCommand.Parameters["@CrossRateUSD"].Value = (item.CrossRateUSD == null ? DBNull.Value : (object)item.CrossRateUSD);
                oCommand.Parameters.Add(new SqlParameter("@CurrencyCode", SqlDbType.NVarChar, 10));
                oCommand.Parameters["@CurrencyCode"].Value = (item.CurrencyCode == null ? DBNull.Value : (object)item.CurrencyCode);
                oCommand.Parameters.Add(new SqlParameter("@CurrencyName", SqlDbType.NVarChar, 100));
                oCommand.Parameters["@CurrencyName"].Value = (item.CurrencyName == null ? DBNull.Value : (object)item.CurrencyName);
                oCommand.Parameters.Add(new SqlParameter("@ForexBuying", SqlDbType.Decimal));
                oCommand.Parameters["@ForexBuying"].Value = (item.ForexBuying == null ? DBNull.Value : (object)item.ForexBuying);
                oCommand.Parameters.Add(new SqlParameter("@ForexSelling", SqlDbType.Decimal));
                oCommand.Parameters["@ForexSelling"].Value = (item.ForexSelling == null ? DBNull.Value : (object)item.ForexSelling);
                oCommand.Parameters.Add(new SqlParameter("@Kod", SqlDbType.NVarChar, 10));
                oCommand.Parameters["@Kod"].Value = (item.Kod == null ? DBNull.Value : (object)item.Kod);
                oCommand.Parameters.Add(new SqlParameter("@Unit", SqlDbType.Int));
                oCommand.Parameters["@Unit"].Value = (item.Unit == null ? DBNull.Value : (object)item.Unit);


                SqlDataAdapter oDataAdapter = new SqlDataAdapter(oCommand);
                DataSet ds = new DataSet();
                oDataAdapter.Fill(ds);
            }
        }
        private static DataSet GetTCMBKur(Uri uri)
        {
            DataSet ds = new DataSet();
            try
            {
                string xmlStr;
                using (var wc = new WebClient())
                {
                    UTF8Encoding utf8 = new UTF8Encoding();
                    wc.Encoding = utf8;
                    xmlStr = wc.DownloadString(uri);
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlStr);
                ds.ReadXml(new XmlNodeReader(xmlDoc));
                logger.Info("Veriler çekildi.");
                return ds;
            }
            catch (Exception ex)
            {
                logger.ErrorException("Hata oluştu.", ex);
                return null;
            }
        }

        private static Dictionary<string, object> GetXmlData(XElement xml)
        {
            var attr = xml.Attributes().ToDictionary(d => d.Name.LocalName, d => (object)d.Value);
            if (xml.HasElements) attr.Add("_value", xml.Elements().Select(e => GetXmlData(e)));
            else if (!xml.IsEmpty) attr.Add("_value", xml.Value);

            return new Dictionary<string, object> { { xml.Name.LocalName, attr } };
        }
        //private static void SendMail(string subject, string content)
        //{
        //    try

        //    {
        //        MailMessage mail = new MailMessage();
        //        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
        //        mail.From = new MailAddress("miracdogn@gmail.com");
        //        mail.To.Add("miracdgn@outlook.com");
        //        mail.Subject = subject;
        //        mail.Body = content;
        //        SmtpServer.Port = 25;
        //        SmtpServer.Credentials = new System.Net.NetworkCredential("mail_adresiniz.", "mail_sifreniz");
        //        SmtpServer.EnableSsl = true;
        //        SmtpServer.Send(mail);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}


        protected override void OnStop()
        {
            logger.Warn("Sistem durdu..");
        }
    }
}
