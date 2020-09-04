using QuantConnect.Algorithm.CSharp.Helpers;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    public class Day9SmaOnCspx : QCAlgorithm
    {
        /*
        SetStartDate(2011, 01, 01);
        SetEndDate(2020, 8, 1);
        SetCash(30000);

        Tickers = "CSPX", "GBS", "SPY", "IAUP"

        Days invested: 1930 / 4907
        Max leverage: 1.8x

        STATISTICS:: Total Trades 369
        STATISTICS:: Average Win 2.39%
        STATISTICS:: Average Loss -1.24%
        STATISTICS:: Compounding Annual Return 10.716%
        STATISTICS:: Drawdown 35.400%
        STATISTICS:: Expectancy 0.426
        STATISTICS:: Net Profit 165.417%
        STATISTICS:: Sharpe Ratio 0.727
        STATISTICS:: Probabilistic Sharpe Ratio 15.179%
        STATISTICS:: Loss Rate 51%
        STATISTICS:: Win Rate 49%
        STATISTICS:: Profit-Loss Ratio 1.93
        */

        int longDays = 50;
        int shortDays = 20;

        Dictionary<string, SmaCrossover> _smaCrossovers = new Dictionary<string, SmaCrossover>();
        SmaCrossover _smaCrossoverSpy;

        (decimal close, decimal diff) spy;

        public override void Initialize()
        {
            SetStartDate(2011, 01, 01);
            SetEndDate(2020, 8, 1);
            SetCash(30000);
            //QuantConnect.Market.AddMarkets("LSEETF", 1);
            AddEquity("CSPX", Resolution.Daily, market: "LSEETF", extendedMarketHours: false);
            AddEquity("GBS", Resolution.Daily, market: "LSE", extendedMarketHours: false);
            AddEquity("SPY", Resolution.Daily, market: "USA", extendedMarketHours: false); // TODO cant invest in this!!
            AddEquity("IAUP", Resolution.Daily, market: "LSE", extendedMarketHours: false);

            //Consolidate("SPY", TimeSpan.FromMinutes(60), OnDataConsolidated);
            //var market = new QuantConnect.Securities.Equity.EquityExchange();

            //var history = History("SPY", 250).ToArray();
            SetWarmUp(TimeSpan.FromDays(longDays));

            foreach(var ticker in tickers)
            {
                _smaCrossovers[ticker] = new SmaCrossover(this, ticker, shortDays, longDays);
            }

            //bol = new Indicators.BollingerBands(20, 2, Indicators.MovingAverageType.Exponential);
            //    bol.Update(DateTime.Now, )
        }
        //1. Create a function named void ClosePositions()
        //2. Set openingBar to null and liquidate SPY

        private int days =0;
        private int investedDays = 0;

        private string[] tickers = new[] { "IAUP",  "CSPX",
                                                    "GBS",
                                                    "SPY",
                                                    "IAUP"};


        private int maxInvested = 3;

        public void SetHoldingsLocal(string ticker, double amount)
        {
            if (amount == 0d)
            {
                if (Portfolio[ticker].Invested)
                {
                    Console.WriteLine($"SOLD the {ticker}");
                    SetHoldings(ticker, 0);
                }

                return;
            }

            if (Portfolio.Count(s => s.Value.Invested) < maxInvested)
            {
                Console.WriteLine($"got the {ticker}");
                SetHoldings(ticker, amount);
            }
        }

        public override void OnData(Slice data)
        {
            if (IsWarmingUp)
            {
                return;
            }

            days++;
            // Log(data.Time.ToString() + " " + data.Keys[0]);

            foreach(var ticker in tickers)
            {
                if (data.Keys.Any(k => k.Value == ticker))
                {
                    var price = data.Bars[ticker];
                    var symbol = data.Keys.Single(k => k.Value == ticker);

                    var indicator = _smaCrossovers[ticker].GetIndicator(price);
                    if (indicator == Indication.Long)
                    {
                        SetHoldingsLocal(ticker, 0.6d);
                    }

                    if (indicator == Indication.Sell)
                    {
                         SetHoldingsLocal(ticker, 0);
                    }

                }
            
                // The side doesnot work and probably shorting is not a good idea


                //if (price.Close < smaLong && previousAboveLong && !Portfolio[ticker].Invested)
                //{
                //    side = Side.Short;
                //    SetHoldings(ticker, -1.5);
                //}

                //if (price.Close > smaShort && !previousAboveLong && side == Side.Short)
                //{
                //    side = Side.None;
                //    SetHoldings(ticker, 0);
                //}


                
            }

            
            // Console.WriteLine($"{investedDays} / {days}");
        }

        public override void OnEndOfDay()
        {
            if (Portfolio.Invested)
            {
                investedDays++;
            }
        }
        public override void OnEndOfAlgorithm()
        {
            Console.WriteLine($"{investedDays} / {days}");
        }
        private enum Side
        {
            Long,
            Short,
            None
        }
    }

}