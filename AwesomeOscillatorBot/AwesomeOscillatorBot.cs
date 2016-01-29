using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class AwesomeOscillatorBot : Robot
    {
        private AwesomeOscillatorPlus awoscref;
        private AwesomeOscillator awosc;
        private Laguerre_RSI laguerre;
        private DirectionalMovementSystem adx;
        private StochasticOscillator stoch;
        private QualitativeQuantitativeE qqe;
        private List<double> AwOscGreenValues;
        private List<double> AwOscRedValues;
        private List<double> StochDValues;
        private List<double> StochKValues;
        private int position_counter = 0;
        double[] input = 
        {
            (0.0),
            (0.0),
            (0.0),
            (0.0),
            (0.0)
        };
        private enum AOValues
        {
            Red,
            Green,
            None
        }
        private AOValues ao_index = AOValues.None;
        private AOValues previous_value = AOValues.None;
        [Parameter("QQE Value", DefaultValue = 14)]
        public int QQEValue { get; set; }
        [Parameter("ADX Value", DefaultValue = 14)]
        public int ADXValue { get; set; }
        [Parameter("Position volume", DefaultValue = 100000)]
        public int PositionVolume { get; set; }
        [Parameter("Initial Stop Loss Value", DefaultValue = 20)]
        public int init_StopLoss { get; set; }
        [Parameter("Trailing Stop", DefaultValue = 15)]
        public int StopLoss { get; set; }
        [Parameter("Stochastic First parameter", DefaultValue = 21)]
        public int FirstStochParam { get; set; }
        [Parameter("Stochastic Second parameter", DefaultValue = 21)]
        public int SecondStochParam { get; set; }
        protected override void OnStart()
        {
            // Put your initialization logic here
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;
            awoscref = Indicators.GetIndicator<AwesomeOscillatorPlus>(5, 34);
            awosc = Indicators.AwesomeOscillator();
            qqe = Indicators.GetIndicator<QualitativeQuantitativeE>(QQEValue);
            laguerre = Indicators.GetIndicator<Laguerre_RSI>(0.5);
            adx = Indicators.DirectionalMovementSystem(ADXValue);
            stoch = Indicators.StochasticOscillator(FirstStochParam, SecondStochParam, 9, MovingAverageType.Simple);
            AwOscGreenValues = new List<double>(input);
            AwOscRedValues = new List<double>(input);
            StochDValues = new List<double>(input);
            StochKValues = new List<double>(input);
        }
        protected override void OnBar()
        {
            base.OnBar();
            ListHandler(StochDValues, qqe.ResultS);
            ListHandler(StochKValues, qqe.Result);
            ListHandler(AwOscGreenValues, awoscref.AwesomeGreen);
            ListHandler(AwOscRedValues, awoscref.AwesomeRed);
            //StochKValues[0] < 80 && StochKValues[1] > 80
            //awosc.Result.LastValue == AwOscRedValues[0] && previous_value == AOValues.Green && 
            // && awosc.Result.LastValue == AwOscRedValues[0] && previous_value == AOValues.Green)
            if (StochKValues[0] < StochDValues[0] && StochKValues[1] > StochDValues[1] && adx.DIMinus.LastValue > adx.DIPlus.LastValue && awosc.Result.LastValue == AwOscRedValues[0] && previous_value == AOValues.Green)
            {
                //Print("AO is GREEN!");
                Print("We SELL!");
                Print("Stoch K last values " + StochKValues[0] + " " + StochKValues[1]);
                Print("Stoch D last values " + StochDValues[0] + " " + StochDValues[1]);
                Print("AO is green equals " + (awosc.Result.LastValue == AwOscGreenValues[0]));
                Print("AO is red equals " + (awosc.Result.LastValue == AwOscRedValues[0]));
                Print("Previous value " + previous_value);
                OpenPosition(TradeType.Sell);
            }
            //StochKValues[0] > 20 && StochKValues[1] < 20
            //awosc.Result.LastValue == AwOscGreenValues[0] && previous_value == AOValues.Red && 
            // && awosc.Result.LastValue == AwOscGreenValues[0] && previous_value == AOValues.Red)
            else if (StochKValues[0] > StochDValues[0] && StochKValues[1] < StochDValues[1] && adx.DIMinus.LastValue < adx.DIPlus.LastValue && awosc.Result.LastValue == AwOscGreenValues[0] && previous_value == AOValues.Red)
            {
                //Print("AO is RED!");
                Print("We BUY!");
                Print("Stoch K last values " + StochKValues[0] + " " + StochKValues[1]);
                Print("Stoch D last values " + StochDValues[0] + " " + StochDValues[1]);
                Print("AO is green equals " + (awosc.Result.LastValue == AwOscGreenValues[0]));
                Print("AO is red equals " + (awosc.Result.LastValue == AwOscRedValues[0]));
                Print("Previous value " + previous_value);
                OpenPosition(TradeType.Buy);
            }
            else
            {
                //Print("None!");
            }

            if (awosc.Result.LastValue == AwOscGreenValues[0])
            {
                previous_value = AOValues.Green;
            }
            else if (awosc.Result.LastValue == AwOscRedValues[0])
            {
                previous_value = AOValues.Red;
            }
            else
            {
                previous_value = AOValues.None;
            }
            if (Positions.Count > 0)
            {
                foreach (Position position in Positions)
                {
                    //TrailingStop(position);
                }
            }
            //ListHandler(AwOscGreenValues, awosc.AwesomeGreen);
            //ListHandler(AwOscRedValues, awosc.AwesomeRed);
            //if (AwOscGreenValues[0] == AwOscGreenValues[1])
            //{
            //    Print("Is RED!");
            //}
            //else
            //{
            //    Print("Is GREEN!");
            //    Print("Green and Red list values are  " + AwOscGreenValues[0] + "  " + AwOscGreenValues[1] + "  " + AwOscRedValues[0] + "  " + AwOscRedValues[1]);
            //    Print("AwesomeOscillator last value Creen and Red " + awosc.AwesomeGreen.LastValue + " " + awosc.AwesomeRed.LastValue);
            //    Print("Two last GREEN and RED values " + awosc.AwesomeGreen.Last(0) + " " + awosc.AwesomeGreen.Last(1) + " and Red " + awosc.AwesomeRed.Last(0) + " " + awosc.AwesomeRed.Last(1));
            //}
        }
        protected override void OnTick()
        {
            // Put your core logic here
            base.OnTick();
            //ListHandler(AwOscGreenValues, awoscref.AwesomeGreen);
            //ListHandler(AwOscRedValues, awoscref.AwesomeRed);
            //if (AwOscGreenValues[0] == AwOscGreenValues[1] && AwOscGreenValues[1] == AwOscGreenValues[2] && AwOscGreenValues[2] == AwOscGreenValues[3])
            //{
            //    ao_index = AOValues.Red;
            //    // Print("AO turned RED!");
            //}
            //else if (AwOscRedValues[0] == AwOscRedValues[1] && AwOscRedValues[1] == AwOscRedValues[2] && AwOscRedValues[2] == AwOscRedValues[3])
            //{
            //    ao_index = AOValues.Green;
            //    //Print("AO turned GREEN!");
            //}
            //else
            //{
            //    ao_index = AOValues.None;
            //}

            foreach (Position position in Positions)
            {
                //Print("Position " + position.Label + " gross profit " + position.GrossProfit);
                TrailingStop(position);
            }

            //Print("Last GREEN value " + awosc.AwesomeGreen.LastValue);
            //Print(AwOscGreenValues[0] == AwOscGreenValues[1]);
            //Print("Last RED value " + awosc.AwesomeRed.LastValue);
            //Print("")
        }
        protected void OnPositionOpened(PositionOpenedEventArgs args)
        {
            base.OnPositionOpened(args.Position);
            Print("Position " + args.Position.Label + " opened!");
            //List<double> list;
            //list = new List<double>(input);
            //Print("Preparing position list");
            //positions_values.Add("" + position_counter, list);
            //Print("Created position list " + positions_values.Count + "with label " + position_counter);
            position_counter++;
        }
        protected void OnPositionClosed(PositionClosedEventArgs args)
        {
            base.OnPositionClosed(args.Position);
            Print("Position " + args.Position.Label + "closed!");
            Print("Current positions count = " + Positions.Count);
            //positions_values.Remove(args.Position.Label);
            //Print("Current lists count = " + positions_values.Count);
        }
        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
        private void TrailingStop(Position position)
        {
            if (Symbol.Code == position.SymbolCode)
            {

                //set initial sotploss
                //if (position.TradeType == TradeType.Sell && position.StopLoss == null)
                //    ModifyPosition(position, position.EntryPrice + init_StopLoss * Symbol.PipSize, null);
                //if (position.TradeType == TradeType.Buy && position.StopLoss == null)
                //    ModifyPosition(position, position.EntryPrice - init_StopLoss * Symbol.PipSize, null);
                //trailing stop
                if (position.GrossProfit > 0)
                {

                    if (position.TradeType == TradeType.Sell)
                    {
                        double distance = position.EntryPrice - Symbol.Ask;

                        if (distance >= 5 * StopLoss * Symbol.PipSize)
                        {
                            Print(" Position " + position.Label + " " + position.TradeType + " entered trailing stop!");
                            ClosePosition(position);
                            //double newStopLossPrice = Symbol.Ask + init_StopLoss * Symbol.PipSize;
                            //if (position.StopLoss == null || newStopLossPrice < position.StopLoss)
                            //{
                            //    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                            //    Print("Current stop loss " + position.StopLoss + " and current Ask price " + Symbol.Ask + " and Bid " + Symbol.Bid);
                            //    Print("Position Gross Profit " + position.GrossProfit);
                            //}
                        }

                    }
                    else
                    {
                        double distance = Symbol.Bid - position.EntryPrice;

                        if (distance >= 5 * StopLoss * Symbol.PipSize)
                        {
                            Print(" Position " + position.Label + " " + position.TradeType + " entered trailing stop!");
                            ClosePosition(position);
                            //double newStopLossPrice = Symbol.Bid - init_StopLoss * Symbol.PipSize;
                            //if (position.StopLoss == null || newStopLossPrice > position.StopLoss)
                            //{
                            //    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                            //    Print("Current stop loss " + position.StopLoss + " and current Ask price " + Symbol.Ask + " and Bid " + Symbol.Bid + " position entry price " + position.EntryPrice);
                            //    Print("Position Gross Profit " + position.GrossProfit);
                            //}
                        }
                    }
                }

            }
        }
        private void OpenPosition(TradeType trade)
        {
            ExecuteMarketOrder(trade, Symbol, PositionVolume, "" + position_counter);
        }
        private void ListHandler(List<double> inputlist, DataSeries inputseries)
        {
            inputlist.Insert(0, inputseries.LastValue);
            inputlist.RemoveAt(inputlist.Count - 1);
        }
    }
}
