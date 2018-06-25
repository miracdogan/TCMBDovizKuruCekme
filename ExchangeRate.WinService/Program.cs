using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRate.WinService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ExchangeRate()
            };
            ServiceBase.Run(ServicesToRun);

            //ExchangeRate.GetExchangeRate();
        }
    }
}
