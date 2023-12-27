# SOD.StockMarket

Add's a fully functional stock market to the game through a cruncher app.
Stocks are generated on a NewGame start and persist through save/load games.
Some stocks are directly based on in-game companies, others are completely randomized.
All stocks are updated each in-game minute you can see pass on your in-game clock.
Every hour there is a chance for a stock to start a trend, these trends can persist for a few days.
These trends will raise or lower the stock's value by much larger margins than the usual changes each stock undergoes.

Eventually in a later version these trends can be influenced by in-game actions the player takes.

**Features**
- Stockmarket cruncher app
- Buying / Selling stocks
- Different type of buy/sell orders (market order, limit order)
- Prices are influenced based on game economy, events, news, player actions
- Stock prices calculate each in game minute (you can see the time on your game watch)