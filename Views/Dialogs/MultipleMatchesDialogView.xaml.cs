using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace RegridMapper.Views
{
    public partial class MultipleMatchesDialogView : UserControl
    {
        public MultipleMatchesDialogView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DoubleAnimation slideDownAnimation = new DoubleAnimation
            {
                From = -100, // Start position (above the control)
                To = 0,      // End position (normal)
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            SlideTransform.BeginAnimation(TranslateTransform.YProperty, slideDownAnimation);
        }
    }
}
