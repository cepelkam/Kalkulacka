using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1
{
    /// <summary>
    /// Interakce pro MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        // Aktuální text v displeji
        private string _display = "0";

        public MainWindow()
        {
            InitializeComponent();

            // Globální zachytávání kláves pro pohodlné ovládání klávesnicí
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            UpdateDisplay();
        }

        // Aktualizuje displej (TextBlock)
        private void UpdateDisplay()
        {
            DisplayBlock.Text = _display;
        }

        // Přidá číslici
        private void AppendDigit(string digit)
        {
            if (_display == "0")
                _display = digit;
            else
                _display += digit;

            UpdateDisplay();
        }

        // Přidá desetinnou tečku (kontrola, aby nebyly dvě v tomtéž čísle)
        private void AppendDecimal()
        {
            var lastNumber = Regex.Match(_display, @"(\d+\.?\d*)$").Value;
            if (!lastNumber.Contains("."))
            {
                _display += ".";
                UpdateDisplay();
            }
        }

        // Přidá operátor (nahrazuje poslední operátor, pokud tam už je)
        private void AppendOperator(string op)
        {
            if (_display.Length > 0 && Regex.IsMatch(_display[^1].ToString(), @"[\+\-\*/%]"))
            {
                _display = _display.Remove(_display.Length - 1) + op;
            }
            else
            {
                _display += op;
            }

            UpdateDisplay();
        }

        private void DoClear()
        {
            _display = "0";
            UpdateDisplay();
        }

        private void DoBackspace()
        {
            if (_display.Length <= 1)
                _display = "0";
            else
                _display = _display.Substring(0, _display.Length - 1);

            UpdateDisplay();
        }

        private void DoEquals()
        {
            try
            {
                var expr = _display.Replace(" ", string.Empty);

                if (string.IsNullOrWhiteSpace(expr))
                {
                    _display = "0";
                    UpdateDisplay();
                    return;
                }

                if (!Regex.IsMatch(expr, @"^[0-9\+\-\*/%\.\(\)]+$"))
                {
                    _display = "Chyba";
                    UpdateDisplay();
                    return;
                }

                var table = new DataTable();
                var resultObj = table.Compute(expr, string.Empty);

                _display = resultObj is double or decimal
                    ? Convert.ToDouble(resultObj).ToString()
                    : resultObj.ToString();
            }
            catch
            {
                _display = "Chyba";
            }

            UpdateDisplay();
        }

        // Event handlery pro tlačítka
        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            AppendDigit(btn.Content.ToString());
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            AppendDecimal();
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            string op = btn.Content.ToString();

            // Namapovat vizuální symboly na operátory použitelné v eval
            if (op == "×") op = "*";
            if (op == "÷") op = "/";
            if (op == "−") op = "-";
            if (op == "%") op = "%";

            AppendOperator(op);
        }

        private void Clear_Click(object sender, RoutedEventArgs e) => DoClear();
        private void Backspace_Click(object sender, RoutedEventArgs e) => DoBackspace();
        private void Equals_Click(object sender, RoutedEventArgs e) => DoEquals();

        // Klávesové zkratky: čísla, tečka, + - * /, Enter, Backspace, Esc
        private void MainWindow_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key >= Key.D0 && key <= Key.D9)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    AppendDigit(((int)(key - Key.D0)).ToString());
                    e.Handled = true;
                    return;
                }
            }

            if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                AppendDigit(((int)(key - Key.NumPad0)).ToString());
                e.Handled = true;
                return;
            }

            if (key == Key.OemPeriod || key == Key.Decimal)
            {
                AppendDecimal();
                e.Handled = true;
                return;
            }

            if (key == Key.Add || key == Key.OemPlus)
            {
                AppendOperator("+");
                e.Handled = true;
                return;
            }

            if (key == Key.Subtract || key == Key.OemMinus)
            {
                AppendOperator("-");
                e.Handled = true;
                return;
            }

            if (key == Key.Multiply)
            {
                AppendOperator("*");
                e.Handled = true;
                return;
            }

            if (key == Key.Divide)
            {
                AppendOperator("/");
                e.Handled = true;
                return;
            }

            if (key == Key.D5 && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                AppendOperator("%");
                e.Handled = true;
                return;
            }

            if (key == Key.Enter || key == Key.Return)
            {
                DoEquals();
                e.Handled = true;
                return;
            }

            if (key == Key.Escape)
            {
                DoClear();
                e.Handled = true;
                return;
            }

            if (key == Key.Back)
            {
                DoBackspace();
                e.Handled = true;
                return;
            }
        }
    }
}