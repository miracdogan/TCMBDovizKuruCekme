using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRate.Model
{


    public class DovizKurlari
    {
        public Tarih_Date Tarih_Date { get; set; }
    }

    public class Tarih_Date
    {
        public List<Currency> Currency { get; set; }
        public DateTime? Tarih { get; set; }
        public string Date { get; set; }
        public string Bulten_No { get; set; }
    }

    public class Currency
    {
        public decimal? Unit { get; set; }
        public string Isim { get; set; }
        public string CurrencyName { get; set; }
        public decimal? ForexBuying { get; set; }
        public decimal? ForexSelling { get; set; }
        public decimal? BanknoteBuying { get; set; }
        public decimal? BanknoteSelling { get; set; }
        public decimal? CrossRateUSD { get; set; }
        public decimal? CrossRateOther { get; set; }
        public decimal? CrossOrder { get; set; }
        public string Kod { get; set; }
        public string CurrencyCode { get; set; }
    }


}
