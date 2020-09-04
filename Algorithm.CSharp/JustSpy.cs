using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;

namespace QuantConnect.Algorithm.CSharp
{
    /*
    SetStartDate(2011, 01, 01);
    SetEndDate(2020, 7, 1);
    SetCash(30000);

    Tickers =  "SPY"


Max leverage: 1.8x
STATISTICS:: Total Trades 1
STATISTICS:: Average Win 0%
STATISTICS:: Average Loss 0%
STATISTICS:: Compounding Annual Return 15.097%
STATISTICS:: Drawdown 41.000%
STATISTICS:: Expectancy 0
STATISTICS:: Net Profit 285.055%
STATISTICS:: Sharpe Ratio 0.689
STATISTICS:: Probabilistic Sharpe Ratio 11.796%
STATISTICS:: Loss Rate 0%
STATISTICS:: Win Rate 0%
      
    */
    class JustSpy : QCAlgorithm
    {
        decimal low;
        decimal high;

        TradeBar openingBar;
        Indicators.SimpleMovingAverage bullIndicator;

        // Only get in during bull conditions
        decimal previousDay;
        Indicators.BollingerBands bol;
        int bullDays = 9;

        (decimal close, decimal diff) spy;

        public override void Initialize()
        {
            SetStartDate(2011, 01, 01);
            SetEndDate(2020, 8, 1);
            SetCash(30000);
            //QuantConnect.Market.AddMarkets("LSEETF", 1);
            AddEquity("CSPX", Resolution.Daily, market: "LSEETF", extendedMarketHours: false);
            AddEquity("SPY", Resolution.Daily, market: "USA", extendedMarketHours: false);

            //Consolidate("SPY", TimeSpan.FromMinutes(60), OnDataConsolidated);
            var market = new QuantConnect.Securities.Equity.EquityExchange();

            //var history = History("SPY", 250).ToArray();
            //SetWarmUp(TimeSpan.FromDays(bullDays));

            bullIndicator = SMA("CSPX", bullDays);

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
            if (!Portfolio.Invested)
            {
                SetHoldings("SPY", 1.8m);
            }   
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
    }
}