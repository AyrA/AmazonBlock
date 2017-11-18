namespace AmazonBlock
{
    public struct AmazonJson
    {
        public long syncToken;
        public string createDate;
        public IPv4Prefix[] prefixes;
        public IPv6Prefix[] ipv6_prefixes;
    }

    public struct IPv6Prefix
    {
        public string ipv6_prefix;
        public string region;
        public string service;
    }

    public struct IPv4Prefix
    {
        public string ip_prefix;
        public string region;
        public string service;
    }
}
