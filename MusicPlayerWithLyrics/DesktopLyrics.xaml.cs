using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayerWithLyrics
{
    /// <summary>
    /// DesktopLyrics.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopLyrics : Window
    {
        public DesktopLyrics()
        {
            InitializeComponent();
        }

        private void Move_window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Toolbar.IsEnabled = false;
            ((Rectangle)sender).Cursor = Cursors.SizeAll;
            try { DragMove(); }
            catch { }
            Toolbar.IsEnabled = true;
            ((Rectangle)sender).Cursor = Cursors.Arrow;
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == true)
            {
                ExitButton.Visibility = Visibility.Hidden;
                backGround.Visibility = Visibility.Hidden;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;
            }
            else
            {
                ExitButton.Visibility = Visibility.Visible;
                backGround.Visibility = Visibility.Visible;
                ResizeMode = ResizeMode.CanResizeWithGrip;
                Topmost = false;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
