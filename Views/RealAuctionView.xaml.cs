using System.Windows.Controls;

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
    }
}
