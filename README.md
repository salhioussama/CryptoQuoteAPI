# CryptoQuoteAPI
C# librairie to call crypto currencies exchanges and get quotations data

Currently, only Huobi API is implemented.

## Description
This Nuget project can be added to your C# solutions to call crypto currencies API and get quotations for your favorite crypto.


## Installation
1. Go to NuGet manager and search for CryptoQuoteAPI.
2. Install the last version.

Command line installation:
```PowerShell
Install-Package CryptoQuoteAPI
```

## Example code


```C#
using System;
using CryptoQuote.HuobiAPI;
using CryptoQuote.HuobiAPI.DataObjectsModel;

var RestApi = new Rest();

var btcusdt_ticker_history = RestApi.GetTickerHistory("btcusdt", TickerPeriod.Hour4, 2000);
var ethusdt_ticker_history = RestApi.GetTickerHistory("ethusdt", TickerPeriod.Day1, 2000);

btcusdt_ticker_history.Wait();
ethusdt_ticker_history.Wait();

// or you can wait for all task to be executed in parallel.
List<Task> AllTasks = new List<Task>()
{
    btcusdt_ticker_history,
    ethusdt_ticker_history
};

Task.WaitAll(AllTasks.ToArray());
```


## Contributions
Contribution to this project is open. Any kind of help is welcome. Ideas are also welcome.

