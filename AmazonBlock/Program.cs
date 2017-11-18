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
        /// <summary>
        /// URL that delivers Amazons IP Address File
        /// </summary>
        public const string ADDR = "https://ip-ranges.amazonaws.com/ip-ranges.json";

        /// <summary>
        /// Maximum amount of Time to keep the cache
        /// </summary>
        public const int CACHE_MAX_HOURS = 24;

        private static string _CacheFile;

        public static string CacheFile
        {
            get
            {
                if (string.IsNullOrEmpty(_CacheFile))
                {
                    using (var P = Process.GetCurrentProcess())
                    {
                        _CacheFile = Path.Combine(Path.GetDirectoryName(P.MainModule.FileName), "cache.json");
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
            else if (args.Any(m => m.ToLower() == "/i" || m.ToLower() == "/o"))
            {
                AmazonJson JSON;
                if (!File.Exists(CacheFile) || DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(CacheFile)).TotalHours >= CACHE_MAX_HOURS)
                {
                    Console.Error.WriteLine("Downloading Addresses");
                    using (var WC = new WebClient())
                    {
                        try
                        {
                            //Don't use WC.DownloadFile. We don't want the file to be empty on errors.
                            File.WriteAllText(CacheFile, WC.DownloadString(ADDR));
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Unable to download JSON: {0}", ex.Message);
                            if (File.Exists(CacheFile))
                            {
                                Console.Error.WriteLine("Attempting to use old Cache File");
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine("Taking JSON from Cache");
                }
                JSON = JsonConvert.DeserializeObject<AmazonJson>(File.ReadAllText(CacheFile));
                Console.Error.WriteLine("Processing Addresses");
                var Dir = args.Any(m => m.ToLower() == "/i") ? Firewall.Direction.In : Firewall.Direction.Out;

                Firewall.BlockAmazon(JSON, Dir);
            }
            else
            {
                Console.Error.WriteLine("Unblocking Amazon");
                //Just unblock all Amazone Addresses
                Firewall.UnblockAmazon();
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

Warning: Using /o also blocks the Server that delivers the IP File.

");
        }
    }
}
