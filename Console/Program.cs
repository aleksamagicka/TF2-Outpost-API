using Newtonsoft.Json;
using TF2OutpostAPI.Models;

namespace Console
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            if (ulong.TryParse(System.Console.ReadLine(), out ulong tradeID))
            {
                var api = new TF2Outpost.API();
                Trade trade = api.ParseTrade(tradeID).Result; // Use await in ideal conditions
                System.Console.WriteLine(JsonConvert.SerializeObject(trade, Formatting.Indented));
            }
            else
            {
                System.Console.WriteLine("Incorrect number. Exiting.");
            }

            System.Console.ReadLine();
        }
    }
}