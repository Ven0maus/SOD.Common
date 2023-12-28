using System;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppNews : AppContent
    {
        public AppNews(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => throw new NotImplementedException();

        public override void OnSetup()
        {

        }
    }
}
