using QuantConnect.Data.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp.Helpers
{
    public class SmaCrossover
    {
        private Indicators.SimpleMovingAverage smaShort;
        private Indicators.SimpleMovingAverage smaLong;
        private string _ticker;
        private QCAlgorithm _algo;

        private bool previousAboveLong = false;
        private bool previousAboveShort = false;


        public SmaCrossover(QCAlgorithm algo, string ticker, int shortDays, int longDays)
        {
            _ticker = ticker;
            _algo = algo;
            smaLong = algo.SMA(ticker, longDays);
            smaShort = algo.SMA(ticker, shortDays);
        }

        public Indication GetIndicator(TradeBar price)
        {
            if (price.Close > smaLong && !previousAboveLong && !_algo.Portfolio[_ticker].Invested)
            {
                return Indication.Long;
            }

            if (price.Close < smaShort && previousAboveLong && _algo.Portfolio[_ticker].Invested)
            {
                return Indication.Sell;            
            }

            previousAboveLong = price.Close > smaLong;
            previousAboveShort = price.Close > smaShort;

            return Indication.None;
        }
    }

    public enum Indication
    {
        None,
        Short,
        Long,
        Sell,
        Hold
    }
}
