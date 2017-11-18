using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace AmazonBlock
{
    class Program
    {
        public const string ADDR = "https://ip-ranges.amazonaws.com/ip-ranges.json";

        private static string _CacheFile;

        public static string CacheFile
        {
            get
            {
                if (string.IsNullOrEmpty(_CacheFile))
                {
                    using (var P = Process.GetCurrentProcess())
                    {
                        Path.Combine(Path.GetDirectoryName(P.MainModule.FileName), "cache.json");
                    }
                }
                return _CacheFile;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0 && args.Contains("/?"))
            {
                Help();
            }
            else
            {
                AmazonJson JSON;
                if (!File.Exists(CacheFile) || DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(CacheFile)).TotalDays >= 1.0)
                {
                    Console.Error.WriteLine("Downloading Addresses");
                    using (var WC = new WebClient())
                    {
                        try
                        {
                            JSON = JsonConvert.DeserializeObject<AmazonJson>(WC.DownloadString(ADDR));
                            File.WriteAllText(CacheFile, JsonConvert.SerializeObject(JSON, Formatting.Indented));
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Unable to download JSON: {0}", ex.Message);
                            return;
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine("Taking JSON from Cache");
                    JSON = JsonConvert.DeserializeObject<AmazonJson>(File.ReadAllText(CacheFile));
                }
                Console.Error.WriteLine("Processing Addresses");
                Firewall.BlockAmazon(JSON);
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }

        private static void Help()
        {
            Console.Error.WriteLine(@"BlockAmazon.exe /i /o
Blocks Amazons Network

/i  - Block inbound. Recommended for service providers that want to block
      most automated requests.
/o  - Block outbound. Recommended if users can submit an address to connect
      to and the service provider wants to avoid Amazons network.

To block both directions, /i and /o must be used simultaneously. Any
direction not specified will be unblocked, therefore running this
application without any arguments unblocks the entire Network.
The cache file is kept for 24 hours.

");
        }
    }
}
