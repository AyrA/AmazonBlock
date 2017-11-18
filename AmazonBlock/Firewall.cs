using NetFwTypeLib;
using System;
using System.Linq;

namespace AmazonBlock
{
    public static class Firewall
    {
        /// <summary>
        /// Firewall Rule Direction
        /// </summary>
        /// <remarks>
        /// For TCP, the direction is only applied for the Connection Initiation.
        /// Blocking Inbound still allows you to connect to any system and receive
        /// a Response but it prevents them from starting a Connection
        /// </remarks>
        [Flags]
        public enum Direction
        {
            /// <summary>
            /// Inbound
            /// </summary>
            In = 1,
            /// <summary>
            /// Outbound
            /// </summary>
            Out = 2,
            /// <summary>
            /// Both Directions
            /// </summary>
            Both = In | Out
        }

        /// <summary>
        /// Rule Name Prefix
        /// </summary>
        public const string BLOCK = "AmazonBlock";
        public const string COUNTRY = "AMAZON";

        /// <summary>
        /// Gets the Firewall Policy Object
        /// </summary>
        /// <returns></returns>
        private static INetFwPolicy2 GetPolicy()
        {
            return (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        }

        /// <summary>
        /// Unblocks A Country
        /// </summary>
        /// <param name="CountryCode">Country</param>
        /// <param name="D">Direction to Unblock</param>
        public static void UnblockAmazon(AmazonJson Addr, Direction D = Direction.Both)
        {
            var Policy = GetPolicy();
            if (D == Direction.Both)
            {
                UnblockAmazon(Addr, Direction.In);
                UnblockAmazon(Addr, Direction.Out);
            }
            else
            {
                var Key = $"{BLOCK}-{D}-{COUNTRY}";
                var Rules = Policy.Rules.Cast<INetFwRule2>().Select(m => m.Name).ToArray();
                for (var i = 0; i < Rules.Count(m => m == Key); i++)
                {
                    Policy.Rules.Remove(Key);
                }
            }
        }

        /// <summary>
        /// Blocks A Country
        /// </summary>
        /// <param name="CountryCode">Country Code</param>
        /// <param name="IPList">IP Address List to Block</param>
        /// <param name="D">Direction to Block</param>
        /// <remarks>This first completely unblocks said Country</remarks>
        public static void BlockAmazon(AmazonJson Addr, Direction D = Direction.Both)
        {
            if (D == Direction.Both)
            {
                BlockAmazon(Addr, Direction.In);
                BlockAmazon(Addr, Direction.Out);
            }
            else
            {
                UnblockAmazon(Addr, D);
                IpBlock(Addr.prefixes.Select(m => m.ip_prefix).ToArray(), D);
                IpBlock(Addr.ipv6_prefixes.Select(m => m.ipv6_prefix).ToArray(), D);
            }
        }

        private static void IpBlock(string[] IPList, Direction D)
        {
            var Policy = GetPolicy();
            for (var I = 0; I < IPList.Length; I += 1000)
            {
                Console.Error.Write('.');
                var R = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

                R.Name = $"{BLOCK}-{D}-{COUNTRY}";
                R.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                R.Description = $"Blocking All IP Addresses of {COUNTRY}";
                R.Direction = D == Direction.In ? NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN : NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                R.Enabled = true;
                R.InterfaceTypes = "All";
                R.LocalAddresses = "*";
                R.Profiles = int.MaxValue;
                R.Protocol = 256;
                R.RemoteAddresses = string.Join(",", IPList.Skip(I).Take(1000).ToArray());
                Policy.Rules.Add(R);
            }
        }
    }
}
