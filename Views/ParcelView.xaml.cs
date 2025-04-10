using RegridMapper.ViewModels;
using System.Windows.Controls;

namespace RegridMapper.Views
{
    public partial class ParcelView : UserControl
    {
        public ParcelView()
        {
            InitializeComponent();
            DataContext = new ParcelViewModel(); // Ensures ViewModel is connected
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
            => e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }
}
