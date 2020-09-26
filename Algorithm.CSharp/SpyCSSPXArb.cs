using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    public class SpyCSSPXArb : QCAlgorithm
    {
        /*
        SetStartDate(2011, 01, 01);
        SetEndDate(2020, 8, 1);
        SetCash(30000);

        Tickers = "CSSPX", "SPY"


    Max leverage: 1.8x

            NOT CHECKING FOR BULL
    Days invested: 2198 / 4780
    STATISTICS:: Total Trades 241
    STATISTICS:: Average Win 5.47%
    STATISTICS:: Average Loss -2.42%
    STATISTICS:: Compounding Annual Return 20.929%
    STATISTICS:: Drawdown 25.100%
    STATISTICS:: Expectancy 0.954
    STATISTICS:: Net Profit 489.931%
    STATISTICS:: Sharpe Ratio 1.016
    STATISTICS:: Probabilistic Sharpe Ratio 40.421%
    STATISTICS:: Loss Rate 40%
    STATISTICS:: Win Rate 60%
    STATISTICS:: Profit-Loss Ratio 2.26

            CHECKING FOR 5 DAY BULL
    Days invested  1925 / 4788
    STATISTICS:: Total Trades 141
    STATISTICS:: Average Win 8.51%
    STATISTICS:: Average Loss -3.14% -- can this be improved as above
    STATISTICS:: Compounding Annual Return 18.343%
    STATISTICS:: Drawdown 22.400%
    STATISTICS:: Expectancy 1.262
    STATISTICS:: Net Profit 382.068%
    STATISTICS:: Sharpe Ratio 0.982
    STATISTICS:: Probabilistic Sharpe Ratio 37.753%
    STATISTICS:: Loss Rate 39%
    STATISTICS:: Win Rate 61%
    STATISTICS:: Profit-Loss Ratio 2.72

                    CHECKING FOR 6 DAY BULL (TRY MORE DAYS!!) - probably good just because more days - although drawdown is better than 5 day bull
                    2013 / 4788
    STATISTICS:: Total Trades 141
    STATISTICS:: Average Win 7.89%
    STATISTICS:: Average Loss -3.69%
    STATISTICS:: Compounding Annual Return 19.719%
    STATISTICS:: Drawdown 21.400%
    STATISTICS:: Expectancy 1.137
    STATISTICS:: Net Profit 437.063%
    STATISTICS:: Sharpe Ratio 1.045
    STATISTICS:: Probabilistic Sharpe Ratio 44.505%
    STATISTICS:: Loss Rate 32%
    STATISTICS:: Win Rate 68%
    STATISTICS:: Profit-Loss Ratio 2.14


            Now only investing if not currently invested
    BullDays 3
    20200902 21:17:26.885 Trace:: Debug: 2094 / 4870
    STATISTICS:: Total Trades 76
    STATISTICS:: Average Win 9.71%
    STATISTICS:: Average Loss -6.06%
    STATISTICS:: Compounding Annual Return 19.271%
    STATISTICS:: Drawdown 22.400%
    STATISTICS:: Expectancy 0.849
    STATISTICS:: Net Profit 434.282%
    STATISTICS:: Sharpe Ratio 1.019
    STATISTICS:: Probabilistic Sharpe Ratio 41.643%
    STATISTICS:: Loss Rate 29%
    STATISTICS:: Win Rate 71%
    STATISTICS:: Profit-Loss Ratio 1.60

    BullDays 5
    20200902 21:27:04.001 Trace:: Debug: 1943 / 4872
    STATISTICS:: Total Trades 70
    STATISTICS:: Average Win 9.69%
    STATISTICS:: Average Loss -5.63%
    STATISTICS:: Compounding Annual Return 18.387%
    STATISTICS:: Drawdown 22.100%
    STATISTICS:: Expectancy 0.944
    STATISTICS:: Net Profit 397.784%
    STATISTICS:: Sharpe Ratio 0.994
    STATISTICS:: Probabilistic Sharpe Ratio 39.046%
    STATISTICS:: Loss Rate 29%
    STATISTICS:: Win Rate 71%
    STATISTICS:: Profit-Loss Ratio 1.72

    BullDays 9
    20200902 21:28:06.630 Trace:: Debug: 1848 / 4877
    STATISTICS:: Total Trades 70
    STATISTICS:: Average Win 10.75%
    STATISTICS:: Average Loss -5.60%
    STATISTICS:: Compounding Annual Return 13.868%
    STATISTICS:: Drawdown 25.400%
    STATISTICS:: Expectancy 0.753
    STATISTICS:: Net Profit 243.820%
    STATISTICS:: Sharpe Ratio 0.782
    STATISTICS:: Probabilistic Sharpe Ratio 19.224%
    STATISTICS:: Loss Rate 40%
    STATISTICS:: Win Rate 60%
    STATISTICS:: Profit-Loss Ratio 1.92

        */
        decimal low;
        decimal high;

        TradeBar openingBar;
        Indicators.SimpleMovingAverage bullIndicator;

        // Only get in during bull conditions
        decimal previousDay;
        Indicators.BollingerBands bol;
        int bullDays = 3;

        (decimal close, decimal diff) spy;

        public override void Initialize()
        {
            SetStartDate(2011, 10, 01);
            
            SetEndDate(2020, 9, 23);
            SetCash(30000);
            //QuantConnect.Market.AddMarkets("LSEETF", 1);
            AddEquity("CSSPX", Resolution.Daily, market: "LSEETF", extendedMarketHours: false);
            AddEquity("SPY", Resolution.Daily, market: "USA", extendedMarketHours: false);
            AddEquity("IAUP", Resolution.Daily, market: "LSEETF", extendedMarketHours: false);
            
            //Consolidate("SPY", TimeSpan.FromMinutes(60), OnDataConsolidated);
            var market = new QuantConnect.Securities.Equity.EquityExchange();

            //var history = History("SPY", 250).ToArray();
            SetWarmUp(TimeSpan.FromDays(bullDays));

            bullIndicator = SMA("CSSPX", bullDays);

            bol = new Indicators.BollingerBands(20, 2, Indicators.MovingAverageType.Exponential);
            //    bol.Update(DateTime.Now, )
        }
        //1. Create a function named void ClosePositions()
        //2. Set openingBar to null and liquidate SPY

        const decimal x = 1m;
        int investedDays = 0;
        int days = 0;

        public override void OnData(Slice data)
        {
            if (IsWarmingUp)
            {
                return;
            }

            if (data.Keys.Any(k => k == "CSSPX"))
            {
                Console.WriteLine("GOT CSSPX");
            }

            if (data.Keys.Any(k => k == "SPY"))
            {
                Console.WriteLine($"GOT SPY {Time.DayOfWeek} {Time}");
                var close = data.Bars["SPY"].Close;
                if (spy.close != 0)
                {
                    spy.diff = ((close / spy.close) * 100) - 100;
                }

                spy.close = close;

                if (spy.diff < -x * 2)
                {
                    SetHoldings("CSSPX", 0);
                    // not working because the markets are not open at the same time.
                    // Copy this change to another file.
                    if (!Portfolio["IAUP"].Invested)
                    {
                        // why only .5
                        SetHoldings("IAUP", .5m, true);
                    }
                }
                else if (spy.diff > x && Time.DayOfWeek != DayOfWeek.Friday && bullIndicator.Current.Value > previousDay)
                {
                    // Dont get in on Monday morning
                    //  Console.WriteLine($"{previousDay} / {bullIndicator.Current.Value}");

                    if (!Portfolio["CSSPX"].Invested)
                    {
                        SetHoldings("CSSPX", 1.8, true);
                    }

                    if (Portfolio["IAUP"].Invested)
                    {
                        SetHoldings("IAUP", 0);
                    }

                }

                previousDay = bullIndicator.Current.Value;
            }     
        }

        public override void OnEndOfDay()
        {
            foreach(var p in Portfolio)
            {
                Console.WriteLine($"Holding {p.Key} {p.Value.Quantity}");
            }

            days++;

            if (Portfolio.Invested)
            {
                investedDays++;
            }
        }

        public override void OnEndOfAlgorithm()
        {
            Console.WriteLine($"{investedDays} / {days}");
        }
    }
}