﻿using System.Windows.Controls;

namespace RegridMapper.Views
{
    public partial class ParcelView : UserControl
    {
        public ParcelView()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
            => e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }
}
