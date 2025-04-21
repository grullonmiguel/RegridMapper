using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegridMapper.Views
{
    public partial class RealAuctionView : UserControl
    {
        public RealAuctionView()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
            => e.Row.Header = (e.Row.GetIndex() + 1).ToString();

        private void MonthValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string newText = textBox.Text + e.Text;

            // Allow only numbers
            if (!int.TryParse(newText, out int number))
            {
                e.Handled = true;
                return;
            }

            // Prevent "00"
            if (newText == "00")
            {
                e.Handled = true;
                return;
            }

            // Allow "01" to "09" but ensure numbers stay within range (0-12)
            if (number < 0 || number > 12)
            {
                e.Handled = true;
            }

        }
        private void DayValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string newText = textBox.Text + e.Text;

            // Allow only numbers
            if (!int.TryParse(newText, out int number))
            {
                e.Handled = true;
                return;
            }

            // Prevent "00"
            if (newText == "00")
            {
                e.Handled = true;
                return;
            }

            // Allow "01" to "09" but ensure numbers stay within range (0-12)
            if (number < 0 || number > 31)
            {
                e.Handled = true;
            }
        }
        private void YearValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input
            e.Handled = !int.TryParse(e.Text, out _);

        }

        private void YearLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // Validate only after full input
            if (int.TryParse(textBox.Text, out int year))
            {
                int currentYear = DateTime.Now.Year;
                if (year < currentYear || year > currentYear + 1)
                {
                    textBox.Text = ""; // Clear invalid input
                }
            }
            else
            {
                textBox.Text = ""; // Clear non-numeric input
            }
        }

    }
}
