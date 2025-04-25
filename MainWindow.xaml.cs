using RegridMapper.ViewModels;
using System.Windows;
using System.Windows.Media.Animation;

namespace RegridMapper
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        private void TriggerAnimation(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var fadeAnimation = (Storyboard)FindResource("FadeAnimation");
                fadeAnimation?.Begin(ViewContainer);
            });
        }
    }
}