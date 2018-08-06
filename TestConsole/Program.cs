using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TF2Outpost;

namespace TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Enter a trade ID:");

            if (ulong.TryParse(Console.ReadLine(), out ulong tradeID))
            {
                var api = new TF2OutpostApi();
                var trade = await api.ParseTrade(tradeID);
                Console.WriteLine(JsonConvert.SerializeObject(trade, Formatting.Indented));
            }
            else
            {
                Console.WriteLine("Incorrect number. Exiting");
            }

            Console.ReadLine();
        }
    }
}
