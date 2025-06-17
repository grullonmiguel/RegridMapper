using System.Windows;
using System.Windows.Controls;

namespace RegridMapper.Core.Services
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordHelper), new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, OnAttachChanged));
        
        public static void SetAttach(DependencyObject obj, bool value) => obj.SetValue(AttachProperty, value);
        
        public static bool GetAttach(DependencyObject obj) => (bool)obj.GetValue(AttachProperty);

        public static void SetPassword(DependencyObject obj, string value) => obj.SetValue(PasswordProperty, value);

        public static string GetPassword(DependencyObject obj) => (string)obj.GetValue(PasswordProperty);

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
                if (!(bool)passwordBox.GetValue(IsUpdatingProperty))
                {
                    passwordBox.Password = e.NewValue as string;
                }
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordChanged;
                }
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            passwordBox.SetValue(IsUpdatingProperty, true);
            passwordBox.SetValue(PasswordProperty, passwordBox.Password);
            passwordBox.SetValue(IsUpdatingProperty, false);
        }

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordHelper));
    }
}