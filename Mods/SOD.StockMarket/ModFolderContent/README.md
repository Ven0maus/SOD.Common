# SOD.StockMarket

Add's a fully functional stock market to the game through a cruncher app.
Some stocks are directly based on in-game companies, others are completely randomized.
All stocks are updated each in-game minute you can see pass on your in-game clock.
Every hour there is a chance for a stock to start a trend, these trends can persist for a few days.
These trends will raise or lower the stock's value by much larger margins than the usual changes each stock undergoes.

**Features**
- Functional Stockmarket cruncher app
- Stocks are procedural, generated once on new game
- Different type of buy/sell orders (instant order, limit order)
- Stock prices calculate each in game minute (you can see the time on your game watch)
- Occassional trends may arise and push big percentage changes in a stock
- Supports saving and loading stockmarket data through savegames.

Notes:
- At the moment, a new game start is required to have stocks available in the stockmarket.
(This will be addressed in an upcoming update to support existing savegames.)

Planned v2.0.0:
- Existing savegames support (before mod install)
- Player actions in game will influence stock prices
- News about stocks becomes available in the stockmarket app
- Trade history becomes available in the stockmarket app
- Introduction becomes available in the stockmarket app
- Balance / Economy overhaul
- Allow buying decimal amount of stocks instead of integer amount (eg buying 0.02 amount of starch kola at price of €100 when starch kola stock price is €5000)
- Add in-game notification alerts for limit orders that have been completed (configurable in its own menu option in stockmarket app)