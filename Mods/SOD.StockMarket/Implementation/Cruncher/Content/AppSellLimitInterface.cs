using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppSellLimitInterface : AppContent
    {
        private string _targetPriceInput;
        private string TargetPriceInput
        {
            get { return _targetPriceInput; }
            set
            {
                _targetPriceInput = value;

                // Update text field
                _targetSellPrice.text = "€ " + _targetPriceInput;

                // Update visual if the decimal is valid or not
                var priceValid = IsTargetPriceValid(out var price);
                if (!string.IsNullOrEmpty(_targetPriceInput) && !priceValid)
                {
                    _infoText.text = "Invalid decimal. (Max 2 digits after point)";
                    _targetSellPrice.color = Color.red;
                }
                else if (!string.IsNullOrEmpty(_targetPriceInput) && price < _stock.Price)
                {
                    _infoText.text = "Target price cannot be lower than the current.";
                    _targetSellPrice.color = Color.red;
                }
                else
                {
                    _infoText.text = _defaultInfoText;
                    _targetSellPrice.color = Color.white;
                }
            }
        }

        public AppSellLimitInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("LimitSellStockInterface").gameObject;

        private Stock _stock;
        private TextMeshProUGUI _targetSellPrice, _stockSymbol, _stockPrice, _owned, _payment, _amount, _infoText;
        private string _defaultInfoText;
        private Color _originalColor, _hoverColor;
        private UnityEngine.UI.Image _image;

        private readonly KeyCode[] _numberKeys =
        {
            KeyCode.Alpha0,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Keypad0,
            KeyCode.Keypad1,
            KeyCode.Keypad2,
            KeyCode.Keypad3,
            KeyCode.Keypad4,
            KeyCode.Keypad5,
            KeyCode.Keypad6,
            KeyCode.Keypad7,
            KeyCode.Keypad8,
            KeyCode.Keypad9,
        };

        public override void OnSetup()
        {
            _stockSymbol = Container.transform.Find("Stock").Find("Text").GetComponent<TextMeshProUGUI>();
            _stockPrice = Container.transform.Find("Price").Find("Text").GetComponent<TextMeshProUGUI>();
            _owned = Container.transform.Find("Owned").Find("Text").GetComponent<TextMeshProUGUI>();
            _targetSellPrice = Container.transform.Find("TargetPrice").Find("Text").GetComponent<TextMeshProUGUI>();
            _infoText = Container.transform.Find("TargetPrice").Find("InfoText").GetComponent<TextMeshProUGUI>();
            _defaultInfoText = _infoText.text;
            _payment = Container.transform.Find("Payment").Find("Text").GetComponent<TextMeshProUGUI>();
            _amount = Container.transform.Find("Amount").Find("Text").GetComponent<TextMeshProUGUI>();

            _image = _targetSellPrice.transform.parent.GetComponent<UnityEngine.UI.Image>();
            _originalColor = _image.color;
            _hoverColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0.6078f);

            // Reset
            _targetPriceInput = null;
            _infoText.text = _defaultInfoText;
            _targetSellPrice.text = "€";

            MapButton("Back", Back);
            MapButton("+1", () => IncreaseAmount(1));
            MapButton("+10", () => IncreaseAmount(10));
            MapButton("+100", () => IncreaseAmount(100));
            MapButton("+1000", () => IncreaseAmount(1000));
            MapButton("Max", () => IncreaseAmount(decimal.MaxValue));
            MapButton("Reset", ResetAmount);
            MapButton("Sell", SellLimitOrder);

            // Do initial info update
            UpdateInfo(null);

            Lib.Time.OnMinuteChanged += UpdateInfo;
        }

        internal void Update()
        {
            // Handle target price input
            if (!ContentActive || _targetSellPrice == null) return;
            if (Content.controller.currentHover != null && Content.controller.currentHover.gameObject.name.Equals("TargetPrice"))
            {
                // Disable any other input and highlight the hover field
                InputController.Instance.enableInput = false;
                _image.color = _hoverColor;

                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    if (TargetPriceInput.Length > 0)
                        TargetPriceInput = TargetPriceInput[..^1];
                    PlayKeyboardKeyPressSound();
                }
                else if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.KeypadPeriod) || Input.GetKeyDown(KeyCode.Comma))
                {
                    TargetPriceInput += ".";
                    PlayKeyboardKeyPressSound();
                }
                else
                {
                    for (int i = 0; i < _numberKeys.Length; i++)
                    {
                        if (Input.GetKeyDown(_numberKeys[i]))
                        {
                            var numberPressed = i >= 10 ? i - 10 : i;
                            TargetPriceInput += numberPressed;
                            PlayKeyboardKeyPressSound();
                        }
                    }
                }
            }
            else
            {
                // Enable any other input and remove highlight from the hover field
                InputController.Instance.enableInput = true;
                _image.color = _originalColor;
            }
        }

        private decimal _currentAmount = 0;
        private decimal _currentPayment = 0;
        private void IncreaseAmount(decimal amount)
        {
            BalanceAmountAndCostBasedOnCurrentPrices(amount);

            // Update new info
            UpdateInfo(null);
        }

        private void ResetAmount()
        {
            _currentAmount = 0;
            _currentPayment = 0;

            // Reset
            _targetPriceInput = null;
            _infoText.text = _defaultInfoText;
            _targetSellPrice.text = "€";

            // Update new info
            UpdateInfo(null);
        }

        private void SellLimitOrder()
        {
            if (_currentAmount == 0 || !IsTargetPriceValid(out var price) || price < _stock.Price) return;

            var tradeController = Plugin.Instance.Market.TradeController;
            if (tradeController.SellLimitOrder(_stock, price, _currentAmount))
            {
                Content.AppPortfolio.UpdatePortfolio();
                Content.AppStock.UpdateInfo();
                ResetAmount();
            }
        }

        private void BalanceAmountAndCostBasedOnCurrentPrices(decimal amount)
        {
            if (!IsTargetPriceValid(out var price)) return;

            if (price < _stock.Price)
                price = _stock.Price;

            var tradeController = Plugin.Instance.Market.TradeController;
            var maxAffordableAmount = tradeController.GetInvestedVolume(_stock);

            var newAmount = amount == decimal.MaxValue ? maxAffordableAmount : (_currentAmount + amount);
            if (newAmount > maxAffordableAmount)
                newAmount = maxAffordableAmount;

            var totalPayment = Math.Round(newAmount * price, 2);

            _currentAmount = newAmount;
            _currentPayment = totalPayment;
        }

        private void UpdateInfo(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (_stockSymbol == null || _stockSymbol.gameObject == null)
            {
                Lib.Time.OnMinuteChanged -= UpdateInfo;
                return;
            }
            if (_stock == null || !ContentActive) return;

            UpdateInfo(e);
        }

        private void UpdateInfo(Common.Helpers.TimeChangedArgs e)
        {
            if (_stock == null) return;

            _stockSymbol.text = _stock.Symbol;
            _stockPrice.text = _stock.Price.ToString(CultureInfo.InvariantCulture);

            var tradeController = Plugin.Instance.Market.TradeController;
            _owned.text = tradeController.GetInvestedVolume(_stock).ToString(CultureInfo.InvariantCulture);

            // Make sure to balance the amount / cost on update
            if (e != null)
                BalanceAmountAndCostBasedOnCurrentPrices(0);

            _amount.text = _currentAmount.ToString(CultureInfo.InvariantCulture);
            _payment.text = _currentPayment.ToString(CultureInfo.InvariantCulture);
        }

        private void PlayKeyboardKeyPressSound()
        {
            AudioController.Instance.PlayWorldOneShot(AudioControls.Instance.computerKeyboardKey, Player.Instance, Player.Instance.currentNode, Content.transform.position, null, null, 1f, null, false, null, false);
        }

        internal void SetStock(Stock stock)
        {
            _stock = stock;
            ResetAmount();
        }

        private bool IsTargetPriceValid(out decimal targetPrice)
        {
            targetPrice = default;
            if (string.IsNullOrWhiteSpace(TargetPriceInput) || (TargetPriceInput.Length >= 2 && TargetPriceInput.StartsWith("0") && !TargetPriceInput.StartsWith("0.")))
                return false;
            var valid = decimal.TryParse(TargetPriceInput, NumberStyles.Any, CultureInfo.InvariantCulture, out targetPrice);
            if (valid && CountDecimalPlaces(targetPrice) > 2)
                return false;
            return valid;
        }

        private static int CountDecimalPlaces(decimal value)
        {
            // Convert the decimal to a string
            string stringValue = value.ToString(CultureInfo.InvariantCulture);

            // Find the position of the decimal point
            int decimalPointIndex = stringValue.IndexOf('.');

            // If there is no decimal point, or it's the last character, there are no decimal places
            if (decimalPointIndex == -1 || decimalPointIndex == stringValue.Length - 1)
            {
                return 0;
            }

            return stringValue.Length - decimalPointIndex - 1;
        }
    }
}
