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
            if (ChooseFontSize.Items.Count < 2)
            {
                foreach (var item in new int[] { 8, 10, 12, 14, 16, 18, 20, 22, 26, 32, 38, 48, 60, 72 })
                {
                    ChooseFontSize.Items.Add(new ComboBoxItem() { Content = item });
                }
            }
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

        public void CloseWindow() => Close();
        private int selectedIndex = 0;
        private void LyricsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (selectedIndex >= 0)
                {
                    ((ListBoxItem)(((ListBox)sender).Items[selectedIndex])).FontSize = defaultLyricsStyle.FontSize;
                    ((ListBoxItem)(((ListBox)sender).Items[selectedIndex])).FontStyle = defaultLyricsStyle.FontStyle;
                    ((ListBoxItem)(((ListBox)sender).Items[selectedIndex])).FontWeight = defaultLyricsStyle.FontWeight;
                }
                selectedIndex = ((ListBox)sender).SelectedIndex;
                ((ListBoxItem)(((ListBox)sender).SelectedItem)).FontSize = selectedLyricsStyle.FontSize;
                ((ListBoxItem)(((ListBox)sender).SelectedItem)).FontStyle = selectedLyricsStyle.FontStyle;
                ((ListBoxItem)(((ListBox)sender).SelectedItem)).FontWeight = selectedLyricsStyle.FontWeight;
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
        private void SettingStackPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            if (SettingStackPanel.SelectedIndex >= 0)
            {
                ((ListBoxItem)FontStyle.Items[0]).IsSelected = ((ListBoxItem)SettingStackPanel.SelectedItem).FontWeight == FontWeights.Bold;
                ((ListBoxItem)FontStyle.Items[1]).IsSelected = ((ListBoxItem)SettingStackPanel.SelectedItem).FontStyle == FontStyles.Italic;
                for (int i = 0; i < ChooseFontSize.Items.Count; i++)
                {
                    try
                    {
                        if (Convert.ToInt32(((ComboBoxItem)ChooseFontSize.Items[i]).Content) == ((ListBoxItem)SettingStackPanel.SelectedItem).FontSize)
                        {
                            ChooseFontSize.SelectedIndex = i;
                        }
                    }
                    catch { }
                }
                //                   ((ComboBoxItem)ChooseFontSize.SelectedItem).Content = ((ListBoxItem)SettingStackPanel.SelectedItem).FontSize;
                //foreach (var item in from object _ in ChooseFontSize.Items
                //                     where ((ListBoxItem)SettingStackPanel.SelectedItem).FontSize == 
                //                     select new { })
                //{
                //    ChooseFontSize.SelectedIndex = ChooseFontSize.Items.IndexOf(item);
                //}
            }
            //}
            //catch { }
        }

        private void LyricsParagraph_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (LyricsParagraph.SelectedIndex)
                {
                    case 0: SettingStackPanel.HorizontalContentAlignment = lyricsListBox.HorizontalContentAlignment = HorizontalAlignment.Left; break;
                    case 1: SettingStackPanel.HorizontalContentAlignment = lyricsListBox.HorizontalContentAlignment = HorizontalAlignment.Center; break;
                    case 2: SettingStackPanel.HorizontalContentAlignment = lyricsListBox.HorizontalContentAlignment = HorizontalAlignment.Right; break;
                    case 3: SettingStackPanel.HorizontalContentAlignment = lyricsListBox.HorizontalContentAlignment = HorizontalAlignment.Stretch; break;
                    default: break;
                }
            }
            catch { }
        }
        private void FontStyle_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                ((ListBoxItem)SettingStackPanel.SelectedItem).FontWeight = ((ListBoxItem)FontStyle.Items[0]).IsSelected ? FontWeights.Bold : FontWeights.Normal;
                ((ListBoxItem)SettingStackPanel.SelectedItem).FontStyle = ((ListBoxItem)FontStyle.Items[1]).IsSelected ? FontStyles.Italic : FontStyles.Normal;
            }
            catch { }
        }

        private void ChooseFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ((ListBoxItem)SettingStackPanel.SelectedItem).FontSize = Convert.ToInt32(((ComboBoxItem)ChooseFontSize.SelectedItem).Content);
            }
            catch { }
        }

        private void Color_DialogHost_OnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {

        }

        private void OpenColorDialogButton(object sender, RoutedEventArgs e)
        {
            ColorChooseDialog.IsOpen = true;
        }
    }
}
