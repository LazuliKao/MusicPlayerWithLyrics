using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// LyricsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LyricsWindow : Window
    {
        public LyricsWindow()
        {
            InitializeComponent();
        }
        private void Move_window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lyricsListBox.IsEnabled = false;
            ((Grid)sender).Cursor = Cursors.SizeAll;
            try { DragMove(); }
            catch { }
            lyricsListBox.IsEnabled = true;
            ((Grid)sender).Cursor = Cursors.Arrow;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

            if (((ToggleButton)sender).IsChecked == true)
            {
                ((PackIcon)((ToggleButton)sender).Content).Kind = PackIconKind.ImageSizeSelectSmall;
                WindowState = WindowState.Maximized;
            }
            else if (((ToggleButton)sender).IsChecked == false)
            {
                ((PackIcon)((ToggleButton)sender).Content).Kind = PackIconKind.WindowRestore;
                WindowState = WindowState.Normal;
            }
            else
            {
                ((PackIcon)((ToggleButton)sender).Content).Kind = PackIconKind.Resize;
                WindowState = WindowState.Normal;
            }
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e) => CloseWindow();

        public void CloseWindow()
        {
            Close();
        }
        private int selectedIndex = 0;
        private void LyricsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (selectedIndex >= 0) { ((ListBoxItem)(((ListBox)sender).Items[selectedIndex])).FontSize = 12; }
                selectedIndex = ((ListBox)sender).SelectedIndex;
                ((ListBoxItem)(((ListBox)sender).SelectedItem)).FontSize = 24;
            }
            catch { }
        }
        public bool ScrolltoView_able = true;
        private Timer exitScrollBlock = new Timer();
        private void LyricsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ScrolltoView_able = false;
            exitScrollBlock.Stop();
            exitScrollBlock.Dispose();
            exitScrollBlock = new Timer() { Interval = 2000, AutoReset = false, Enabled = true };
            exitScrollBlock.Elapsed += Exit_Scroll_Block;
        }
        private void Exit_Scroll_Block(object sender, EventArgs e)
        {
            ScrolltoView_able = true;
            ((Timer)sender).Close();
            ((Timer)sender).Dispose();
        }
    }
}
