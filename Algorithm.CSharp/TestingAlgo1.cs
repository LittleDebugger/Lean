using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    class TestingAlgo1 : QCAlgorithm
    {
        decimal low;
        decimal high;

        TradeBar openingBar;
        Indicators.SimpleMovingAverage sma8;
        Indicators.SimpleMovingAverage sma20;

        public override void Initialize()
        {
            SetStartDate(2004, 3, 17);
            SetEndDate(2020, 1, 1);
            SetCash(100000);
            //QuantConnect.Market.AddMarkets("LSEETF", 1);
            AddEquity("SPY", Resolution.Minute, market: "ARCA", extendedMarketHours: true);
            //Consolidate("SPY", TimeSpan.FromMinutes(60), OnDataConsolidated);
            //Schedule.On(DateRules.EveryDay("SPY"), TimeRules.At(13, 30), ClosePositions);
            var market = new QuantConnect.Securities.Equity.EquityExchange();

            //var history = History("SPY", 250).ToArray();
            //SetWarmUp(TimeSpan.FromDays(20));


            //sma8 = SMA("SPY", 8, Resolution.Daily);
            //sma20 = SMA("SPY", 20, Resolution.Daily);
        }
        //1. Create a function named void ClosePositions()
        //2. Set openingBar to null and liquidate SPY

        bool bull = true;

        public override void OnData(Slice data)
        {
            var q= data.QuoteBars["SPY"];
            
            Log($"ask open {q.Ask.Open}  ask close {q.Ask.Close}  ask high {q.Ask.High}  ask low {q.Ask.Low}");
            Log($"open {q.Open}  close {q.Close}  high {q.High}  low {q.Low}");
            Log($"bid open {q.Bid.Open}  bid close {q.Bid.Close}  bid high {q.Bid.High}  bid low {q.Bid.Low}");

            var t = data.Bars["SPY"];
            Log($"trade open {t.Open}  trade close {t.Close}  trade high {t.High}  trade low {t.Low}");

            if (!Portfolio.Invested)
            {
                SetHoldings("SPY", 1);
            }
        }

        private void OnDataConsolidated(TradeBar bar)
        {
            if (bar.EndTime.Hour == 9 && bar.EndTime.Hour == 30)
            {
                low = bar.Low;
                high = bar.High;
            }            
        }

        private void ClosePositions()
        {
            openingBar = null;
            Liquidate("SPY");

        }
    }
}
