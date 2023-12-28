using System;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppFundsInterface : AppContent
    {
        public AppFundsInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => throw new NotImplementedException();

        public override void OnSetup()
        {

        }
    }
}
