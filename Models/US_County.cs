﻿using RegridMapper.Core.Configuration;
using RegridMapper.ViewModels;

namespace RegridMapper.Models
{
    public class US_County : Observable
    {
        public string? Name { get; set; }
        public StateCode StateID { get; set; }
        public string? AuctionUrl { get; set; }
        public string? AppraiserUrl { get; set; }
        public string? DetailsURL { get; set; }
        public string? RealAuctionURL { get; set; }
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }
        private bool isSelected;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        private bool _isEnabled = true;
    }
}
