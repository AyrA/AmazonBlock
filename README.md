# AmazonBlock

Blocks the entire Network of Amazon.

The inbound direction is useful to block a huge number of possibly unwanted and automated requests if you run a service.

This application is a variant of [CountryBlock](https://github.com/AyrA/CountryBlock)
and behaves similar, but has a simpler command line usage.

## Permissions

Because Changes in the System Firewall affect all Users and Services,
you need administrative Rights for this Application to execute.
Keep this in Mind if you use it in your Scripts.

## Usage

    AmazonBlock.exe /i /o
    
    /i   - Blocks all inbound connections
    /o   - Blocks all outbound connections

Without arguments, both directions are unblocked.
Blocking a single Direction will always unblock the other direction

# Rule Names

The Rules in the Firewall are named in this Pattern (without Braces):

    AmazonBlock-{In|Out}-AMAZON

## Firewall Limitations

It's no Mistake if you see the same rule name multiple times.
The Windows Firewall has an upper Limit of 1000 IP Entries for each Rule.
AmazonBlock will create multiple Rules with the same Name in this Case.

# API

AmatzonBlock by Default uses the API from https://ip-ranges.amazonaws.com/ip-ranges.json

# IP Version

This Application together with the default API supports IPv4 and IPv6

# Cache

The Application creates a Cache File called cache.json.
The File is created when it is needed for the first Time.
It's valid for 24 hours.

If the API is unreachable and a cache file exists that is older than 24 hours
it will be used instead.

## Stateless operation

This Application is stateless.
It doesn't keeps track of the directions that are blocked
and you are free to manually modify and delete rules.

# Side effects of blocking outbound

Blocking outbound will also block the website that is used to get the IP list.
