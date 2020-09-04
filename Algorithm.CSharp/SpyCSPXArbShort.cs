using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    class SpyCSPXArbShort : QCAlgorithm
    {
       /* 
        * Generally pretty bad. Maybe look into it a bit more.
        */
        decimal low;
        decimal high;

        TradeBar openingBar;
        Indicators.SimpleMovingAverage bearIndicator;

        // Only get in during bull conditions
        decimal previousDay;
        Indicators.BollingerBands bol;
        int bearDays = 5;

        (decimal close, decimal diff) spy;

        public override void Initialize()
        {
            SetStartDate(2011, 01, 01);
            SetStartDate(2011, 01, 01);
            SetEndDate(2020, 7, 1);
            SetCash(30000);
            //QuantConnect.Market.AddMarkets("LSEETF", 1);
            AddEquity("CSPX", Resolution.Daily, market: "LSEETF", extendedMarketHours: false);
            AddEquity("SPY", Resolution.Daily, market: "USA", extendedMarketHours: false);

            //Consolidate("SPY", TimeSpan.FromMinutes(60), OnDataConsolidated);
            var market = new QuantConnect.Securities.Equity.EquityExchange();

            //var history = History("SPY", 250).ToArray();
            SetWarmUp(TimeSpan.FromDays(bearDays));

            bearIndicator = SMA("CSPX", bearDays);

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
            days++;

            if (data.Keys.Any(k => k == "SPY"))
            {
                var close = data.Bars["SPY"].Close;
                if (spy.close != 0)
                {
                    spy.diff = ((close / spy.close) * 100) - 100;
                }

                spy.close = close;

                if (spy.diff < -x * 2  && Time.DayOfWeek != DayOfWeek.Friday && bearIndicator.Current.Value < previousDay)
                {
                    if (!Portfolio["CSPX"].Invested)
                    {
                        SetHoldings("CSPX", -1m);
                    }
                }
                else if (spy.diff > x * 2)
                {
                    // Dont get in on Monday morning
                    //  Console.WriteLine($"{previousDay} / {bullIndicator.Current.Value}");
                    SetHoldings("CSPX", 0);
                    
                    
                }

                previousDay = bearIndicator.Current.Value;
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