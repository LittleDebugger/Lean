using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    class TestingAlgo2 : QCAlgorithm
    {
        decimal low;
        decimal high;

        TradeBar openingBar;
        Indicators.SimpleMovingAverage sma8;
        Indicators.SimpleMovingAverage sma20;
        Indicators.BollingerBands bol;

        public override void Initialize()
        {
            SetStartDate(2000, 1, 1);
            SetEndDate(2020, 1, 1);
            SetCash(100000);
            //QuantConnect.Market.AddMarkets("LSEETF", 1);
            AddEquity("SPY", Resolution.Minute, market: "ARCA", extendedMarketHours: true);
            Consolidate("SPY", TimeSpan.FromMinutes(60), OnDataConsolidated);
            //Schedule.On(DateRules.EveryDay("SPY"), TimeRules.At(13, 30), ClosePositions);
            var market = new QuantConnect.Securities.Equity.EquityExchange();

            //var history = History("SPY", 250).ToArray();
            SetWarmUp(TimeSpan.FromDays(20));


            sma8 = SMA("SPY", 8, Resolution.Daily);
            sma20 = SMA("SPY", 20, Resolution.Daily);
            bol = new Indicators.BollingerBands(20, 2, Indicators.MovingAverageType.Exponential);
        //    bol.Update(DateTime.Now, )
        }
        //1. Create a function named void ClosePositions()
        //2. Set openingBar to null and liquidate SPY

        bool bull = true;

        public override void OnData(Slice data)
        {
            if (data.Time.Hour == 7 && data.Time.Minute == 0)
            {
                bull = sma8 > sma20;
            }
            if ((data.Time.Hour == 9 && data.Time.Minute < 30) || (data.Time.Hour < 9))
            {
                return;
            }

            if (!bull)
            {
                return;
            }

            var bar = data.Bars["SPY"];

            if (Portfolio.Invested)
            {
                    if (bar.Close < low)
                    {
                        Liquidate("SPY");
                    }

                    if (bar.Close > high * 1.2m)
                    {
                        Liquidate("SPY");
                    }

                return;
            }

            if (bar.Close > high)
            {
                SetHoldings("SPY", 1);
            }

            //Log(data["SPY"].Close);
            //var a = data["SPY"].Close;
            
            //if (!Portfolio.Invested)
            //{
            //    SetHoldings("SPY", 1);

            //}

            //if (Portfolio.Invested || openingBar == null)
            //{
            //    return;
            //}

            //if (data["SPY"].Close > openingBar.High)
            //{
                
            //    SetHoldings("SPY", 1);
            //}

            //if (data["SPY"].Close < openingBar.Low)
            //{
            //    SetHoldings("SPY", -1);
            //}
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
