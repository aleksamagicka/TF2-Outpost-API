# TF2 Outpost API
Easily fetch trades from the TF2Outpost.com website.

## Usage
This library supports .NET Core (.NET Standard 1.6.1).

1. Install this library via NuGet.
2. To fetch a trade, initialize the TF2Outpost.API class and call FetchTrade method, like this:
```
var tf2OutpostAPI = new TF2Outpost.API();
Trade tradeInfo = await tf2OutpostAPI.FetchTrade(1);

// All information about trade with id 1 is now in the tradeInfo object.
// Use it however you wish.
```

## TODO
Interacting with trades ((re)opening, closing, commenting).
