using RegridMapper.Core.Configuration;
using RegridMapper.ViewModels;

namespace RegridMapper.Models
{
    public class US_State : Observable
    {

        public StateCode StateID { get; set; }
        public string? Name { get; set; }
        public List<US_County>? Counties { get; set; }
        public SaleTypeCode SalesType { get; set; }
        public string? InterestRate { get; set; }
        public string? InterestRateComments { get; set; }
        public string? RedemptionPeriod { get; set; }
        public string? RedemptionPeriodComments { get; set; }
        public string? Frequency { get; set; }
        public bool CanShowCountyMap { get; set; }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetProperty(ref isSelected, value);
            }
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