using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Path = System.IO.Path;
using System.Windows.Media;

namespace MusicPlayerWithLyrics
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 初始化
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BackupToolMainWindow_Initialized(object sender, EventArgs e)
        {
            Title = "歌词解析-音乐播放器 by gxh";
            WriteLog("正在初始化...");

            drawerHost.Visibility = Visibility.Visible;

            try
            {

                lyrics.Settings.selectedFontColor = ConverterToBrush(IniRead(VarEls.ini_file_path, "selectedFontColor"));
                lyrics.Settings.defaultFontColor = ConverterToBrush(IniRead(VarEls.ini_file_path, "defaultFontColor"));
                lyrics.Settings.desktopLine1FontColor = ConverterToBrush(IniRead(VarEls.ini_file_path, "desktopLine1FontColor"));
                lyrics.Settings.desktopLine2FontColor = ConverterToBrush(IniRead(VarEls.ini_file_path, "desktopLine2FontColor"));

                lyrics.Settings.selectedFontSize = double.Parse(IniRead(VarEls.ini_file_path, "selectedFontSize"));
                lyrics.Settings.defaultFontSize = double.Parse(IniRead(VarEls.ini_file_path, "defaultFontSize"));
                lyrics.Settings.desktopLine1FontSize = double.Parse(IniRead(VarEls.ini_file_path, "desktopLine1FontSize"));
                lyrics.Settings.desktopLine2FontSize = double.Parse(IniRead(VarEls.ini_file_path, "desktopLine2FontSize"));

                lyrics.Settings.selectedFontWeight = IniRead(VarEls.ini_file_path, "selectedFontWeight") == "Normal" ? FontWeights.Normal : FontWeights.Bold;
                lyrics.Settings.defaultFontWeight = IniRead(VarEls.ini_file_path, "defaultFontWeight") == "Normal" ? FontWeights.Normal : FontWeights.Bold;
                lyrics.Settings.desktopLine1FontWeight = IniRead(VarEls.ini_file_path, "desktopLine1FontWeight") == "Normal" ? FontWeights.Normal : FontWeights.Bold;
                lyrics.Settings.desktopLine2FontWeight = IniRead(VarEls.ini_file_path, "desktopLine2FontWeight") == "Normal" ? FontWeights.Normal : FontWeights.Bold;

                lyrics.Settings.selectedFontStyle = IniRead(VarEls.ini_file_path, "selectedFontStyle") == "Normal" ? FontStyles.Normal : FontStyles.Italic;
                lyrics.Settings.defaultFontStyle = IniRead(VarEls.ini_file_path, "defaultFontStyle") == "Normal" ? FontStyles.Normal : FontStyles.Italic;
                lyrics.Settings.desktopLine1FontStyle = IniRead(VarEls.ini_file_path, "desktopLine1FontStyle") == "Normal" ? FontStyles.Normal : FontStyles.Italic;
                lyrics.Settings.desktopLine2FontStyle = IniRead(VarEls.ini_file_path, "desktopLine2FontStyle") == "Normal" ? FontStyles.Normal : FontStyles.Italic;

                lyrics.Settings.HorizontalContentAlignment = IniRead(VarEls.ini_file_path, "HorizontalContentAlignment") == "Left" ? HorizontalAlignment.Left :
                  IniRead(VarEls.ini_file_path, "HorizontalContentAlignment") == "Center" ? HorizontalAlignment.Center : HorizontalAlignment.Right;
                if (IniRead(VarEls.ini_file_path, "musicDir") != null)
                {
                    VarEls.musicDir.SelectedPath = IniRead(VarEls.ini_file_path, "musicDir");
                    LoadMusicList(null);
                }
            }
            catch (Exception) { }
            ShowTip("初始化完成...");

        }
        private SolidColorBrush ConverterToBrush(string input)
        {
            try
            {
                BrushConverter brushConverter = new BrushConverter();
                return (SolidColorBrush)brushConverter.ConvertFromString(input);
            }
            catch (Exception)
            {
                return new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
        }
        public static void ButtonClick(Button objectName, RoutedEventArgs performEvent)
        {
            objectName.RaiseEvent(performEvent);
        }
        #endregion
        #region 托盘工具
        private Forms.NotifyIcon notifyIcon = null;
        private void InitialTray()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Dispose();
            }
            //设置托盘的各个属性
            Forms.NotifyIcon notifyIcon1 = new Forms.NotifyIcon();
            notifyIcon = notifyIcon1;
            notifyIcon.BalloonTipText = "程序隐藏至托盘...";
            notifyIcon.Text = "歌词音乐工具by gxh";
            System.Drawing.Icon icon = ResourcesResx.icon.Icon1;
            notifyIcon.Icon = icon;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(2000);
            notifyIcon.MouseClick += new Forms.MouseEventHandler(NotifyIcon_MouseClick);
            ////设置菜单项
            //System.Windows.Forms.MenuItem menu1 = new System.Windows.Forms.MenuItem("test");
            Forms.MenuItem active = new Forms.MenuItem("Active");
            active.Click += new EventHandler(ActiveWindow);
            //System.Windows.Forms.MenuItem menu = new Forms.MenuItem("菜单", new System.Windows.Forms.MenuItem[] {/*menu1, menu2*/ });
            //退出菜单项
            Forms.MenuItem exit = new Forms.MenuItem("Exit");
            exit.Click += new EventHandler(Exit_Click);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new Forms.MenuItem[] { active, exit };
            notifyIcon.ContextMenu = new Forms.ContextMenu(childen);
        }
        /// 窗体状态改变时候触发
        private void SysTray_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized || this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.Visibility = Visibility.Hidden;
                InitialTray();
            }
        }
        /// 退出选项
        private void Exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要关闭吗?",
                                               "退出",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question,
                                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                notifyIcon.Dispose();
                Application.Current.Shutdown();
            }
        }
        /// 鼠标单击
        public void ActiveWindow(object sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Normal;
                this.Activate();
                this.Focus();
                notifyIcon.Dispose();
            }
        }
        private void NotifyIcon_MouseClick(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ActiveWindow(null, null);
            }
        }
        #endregion
        #region 信息输出
        public void WriteLog(double text) => WriteLog(Convert.ToString(text));
        public void WriteLog(int text) => WriteLog(Convert.ToString(text));
        public void WriteLog(string text)
        {
            lyrics.WriteLog(text);
            //logOutput.AppendText(Environment.NewLine + "[" + DateTime.Now.ToString("hh:mm:ss") + "]>" + text);
            //logOutput.ScrollToEnd();
            //File.AppendAllText(VarEls.runingDir + "info.log", Environment.NewLine + "[" + DateTime.Now.TimeOfDay.ToString().ToString() + "]>" + text);
            //GC.Collect();
        }
        public void WriteLogText(string text)
        {
            lyrics.WriteLog(text);
            //logOutput.AppendText(Environment.NewLine + text);
            //logOutput.ScrollToEnd();
            //GC.Collect();
        }
        public void ShowTip(string text, string actionText, Action action) => snackbar.MessageQueue.Enqueue(text, actionText, action);
        public void ShowTip(string text, string actionText) => snackbar.MessageQueue.Enqueue(text, actionText, () => WriteLog("Clicked=>\"" + text + "\""));
        public void ShowTip(string text) => snackbar.MessageQueue.Enqueue(text, null, null, null, false, false, TimeSpan.FromSeconds(1));

        #endregion
        #region 读写配置项
        public string IniRead(string path, string title)
        {
            string output = null;
            if (File.Exists(path))
            {
                string[] Alltext = File.ReadAllLines(path);
                foreach (var item in Alltext)
                {
                    int find = item.IndexOf('=');
                    if (find > 0)
                    {
                        if (item.Substring(0, find) == title)
                        {
                            output = item.Substring(find + 1);
                            WriteLog("读取配置:" + title + "=\"" + output + "\"");
                            break;
                        }
                    }
                }
            }
            if (output == null)
            {
                WriteLog("读取配置失败:" + path + "=>" + title);
            }
            return output;
        }
        public string IniWrite(string path, string title, object content)
        {
            string output = null;
            if (File.Exists(path))
            {
                string[] Alltext = File.ReadAllLines(path);
                for (int i = 0; i < Alltext.Length; i++)
                {
                    string item = Alltext[i];
                    int find = item.IndexOf('=');
                    if (find > 0)
                    {
                        if (item.Substring(0, find) == title)
                        {
                            output = item.Substring(find + 1);
                            Alltext[i] = title + "=" + content;
                            WriteLog("写入配置:" + title + "=(" + output + ")<=\"" + content + "\"");
                            break;
                        }
                    }
                }
                File.WriteAllLines(path, Alltext);
            }
            if (output == null)
            {
                WriteLog("写入配置:" + title + "<=\"" + content + "\"");
                File.AppendAllText(path, Environment.NewLine + title + "=" + content);
            }
            return null;
        }
        #endregion
        #region 定义变量
        private class VarEls
        {
            public static string runingDir = Environment.CurrentDirectory + "\\";
            public static string ini_file_path = Environment.CurrentDirectory + "\\config.ini";
            public static Forms.FolderBrowserDialog musicDir = new Forms.FolderBrowserDialog();
            public static DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        }
        #endregion
        #region 保存配置
        private void SaveIntButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lyrics.Settings.selectedFontColor = (SolidColorBrush)lyrics.lyricsWindow.selectedLyricsStyle.Foreground;
                lyrics.Settings.defaultFontColor = (SolidColorBrush)lyrics.lyricsWindow.defaultLyricsStyle.Foreground;
                lyrics.Settings.desktopLine1FontColor = (SolidColorBrush)lyrics.lyricsWindow.desktopLyrics1Style.Foreground;
                lyrics.Settings.desktopLine2FontColor = (SolidColorBrush)lyrics.lyricsWindow.desktopLyrics2Style.Foreground;
                lyrics.Settings.selectedFontSize = lyrics.lyricsWindow.selectedLyricsStyle.FontSize;
                lyrics.Settings.defaultFontSize = lyrics.lyricsWindow.defaultLyricsStyle.FontSize;
                lyrics.Settings.desktopLine1FontSize = lyrics.lyricsWindow.desktopLyrics1Style.FontSize;
                lyrics.Settings.desktopLine2FontSize = lyrics.lyricsWindow.desktopLyrics2Style.FontSize;
                lyrics.Settings.selectedFontWeight = lyrics.lyricsWindow.selectedLyricsStyle.FontWeight;
                lyrics.Settings.defaultFontWeight = lyrics.lyricsWindow.defaultLyricsStyle.FontWeight;
                lyrics.Settings.desktopLine1FontWeight = lyrics.lyricsWindow.desktopLyrics1Style.FontWeight;
                lyrics.Settings.desktopLine2FontWeight = lyrics.lyricsWindow.desktopLyrics2Style.FontWeight;
                lyrics.Settings.selectedFontStyle = lyrics.lyricsWindow.selectedLyricsStyle.FontStyle;
                lyrics.Settings.defaultFontStyle = lyrics.lyricsWindow.defaultLyricsStyle.FontStyle;
                lyrics.Settings.desktopLine1FontStyle = lyrics.lyricsWindow.desktopLyrics1Style.FontStyle;
                lyrics.Settings.desktopLine2FontStyle = lyrics.lyricsWindow.desktopLyrics2Style.FontStyle;
                lyrics.Settings.HorizontalContentAlignment = lyrics.lyricsWindow.lyricsListBox.HorizontalContentAlignment;
            }
            catch (Exception) { }

            IniWrite(VarEls.ini_file_path, "musicDir", VarEls.musicDir.SelectedPath.Trim());
            IniWrite(VarEls.ini_file_path, "selectedFontColor", lyrics.Settings.selectedFontColor);
            IniWrite(VarEls.ini_file_path, "defaultFontColor", lyrics.Settings.defaultFontColor);
            IniWrite(VarEls.ini_file_path, "desktopLine1FontColor", lyrics.Settings.desktopLine1FontColor);
            IniWrite(VarEls.ini_file_path, "desktopLine2FontColor", lyrics.Settings.desktopLine2FontColor);
            IniWrite(VarEls.ini_file_path, "selectedFontSize", lyrics.Settings.selectedFontSize);
            IniWrite(VarEls.ini_file_path, "defaultFontSize", lyrics.Settings.defaultFontSize);
            IniWrite(VarEls.ini_file_path, "desktopLine1FontSize", lyrics.Settings.desktopLine1FontSize);
            IniWrite(VarEls.ini_file_path, "desktopLine2FontSize", lyrics.Settings.desktopLine2FontSize);
            IniWrite(VarEls.ini_file_path, "selectedFontWeight", lyrics.Settings.selectedFontWeight);
            IniWrite(VarEls.ini_file_path, "defaultFontWeight", lyrics.Settings.defaultFontWeight);
            IniWrite(VarEls.ini_file_path, "desktopLine1FontWeight", lyrics.Settings.desktopLine1FontWeight);
            IniWrite(VarEls.ini_file_path, "desktopLine2FontWeight", lyrics.Settings.desktopLine2FontWeight);
            IniWrite(VarEls.ini_file_path, "selectedFontStyle", lyrics.Settings.selectedFontStyle);
            IniWrite(VarEls.ini_file_path, "defaultFontStyle", lyrics.Settings.defaultFontStyle);
            IniWrite(VarEls.ini_file_path, "desktopLine1FontStyle", lyrics.Settings.desktopLine1FontStyle);
            IniWrite(VarEls.ini_file_path, "desktopLine2FontStyle", lyrics.Settings.desktopLine2FontStyle);
            IniWrite(VarEls.ini_file_path, "HorizontalContentAlignment", lyrics.Settings.HorizontalContentAlignment);
        }
        #endregion
        #region 窗体 控件
        private void Function_box_SizeChanged(object sender, SizeChangedEventArgs e) => FilesList.Margin = new Thickness(0, function_box.ActualHeight, 0, 0);
        private void Move_window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            card_run.IsEnabled = false;
            move_window.Cursor = Cursors.SizeAll;
            try { DragMove(); }
            catch { }
            card_run.IsEnabled = true;
            move_window.Cursor = Cursors.Arrow;
        }
        private void Exit_ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e) => dialogHost_exit.IsOpen = true;
        private void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            //you can cancel the dialog close:
            //
            if (!Equals(eventArgs.Parameter, true)) return;
            if (exitSelectedItem.SelectedIndex == -1)
            {
                ShowTip("Please choose a operation to confirm!");
                eventArgs.Cancel();
            }
            else if (exitSelectedItem.SelectedIndex == 0)
            {//Exit
                Close();
            }
            else if (exitSelectedItem.SelectedIndex == 1)
            { //Hide
                InitialTray();
                Visibility = Visibility.Hidden;
            }
            //if (!string.IsNullOrWhiteSpace(FruitTextBox.Text))
            //    FruitListBox.Items.Add(FruitTextBox.Text.Trim());
        }
        #endregion
        #region toolBar
        private void Hide_button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InitialTray();
            Visibility = Visibility.Hidden;
        }
        #endregion
        #region Main
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            var openJSONFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "mp3文件|*.mp3"
            };
            var result = openJSONFileDialog.ShowDialog();
            if (result != true) return;
            string File_path = openJSONFileDialog.FileName;
            VarEls.musicDir.SelectedPath = Path.GetDirectoryName(File_path);
            LoadMusicList(File_path);
            mediaElement.Source = new Uri(File_path, UriKind.Relative);
        }
        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                try { ((ListBoxItem)FilesList.Items[(int)FilesList.Tag]).FontWeight = FontWeights.Normal; }
                catch { }
               ((ListBoxItem)FilesList.SelectedItem).FontWeight = FontWeights.Bold;
                FilesList.Tag = FilesList.SelectedIndex;
                mediaElement.Source = new Uri((string)((ListBoxItem)FilesList.SelectedItem).Tag, UriKind.Relative);
            }
            catch { }
        }
        private void LoadMusicList(string File_path)
        {
            FilesList.Items.Clear();
            foreach (var item in Directory.GetFiles(VarEls.musicDir.SelectedPath, "*.mp3"))
            {
                try
                {
                    WriteLog("File found:" + Path.GetFileNameWithoutExtension(item));
                    FilesList.Items.Add(new ListBoxItem() { Content = Path.GetFileNameWithoutExtension(item), Tag = item });
                    if (item == File_path)
                    {
                        FilesList.SelectedIndex = FilesList.Items.Count - 1;
                    }
                }
                catch (Exception err) { lyrics.WriteErrLog(err.ToString()); }
            }
            FilesList.ScrollIntoView(FilesList.SelectedItem);
        }
        private void PlayerPause()
        {
            if (playBtn.IsChecked == true)
            {
                VarEls.timer.Start();
                mediaElement.Play();
                mediaElement.ToolTip = "Click to Pause";
            }
            else
            {
                VarEls.timer.Stop();
                mediaElement.Pause();
                mediaElement.ToolTip = "Click to Play";
            }
        }
        private void PlayBtn_Click(object sender, RoutedEventArgs e) => PlayerPause();
        private void BackBtn_Click(object sender, RoutedEventArgs e) => mediaElement.Position -= TimeSpan.FromSeconds(10);
        private void ForwardBtn_Click(object sender, RoutedEventArgs e) => mediaElement.Position += TimeSpan.FromSeconds(10);
        private Lyrics lyrics = new Lyrics { };
        public void ShowLyrics()
        {
            if (lyrics.lyricsWindow != null)
            {
                lyrics.lyricsWindow.lyricsListBox.Items.Clear();
                if (lyrics.HaveLrc)
                {
                    foreach (var item in lyrics.LrcLines)
                    {
                        ListBoxItem listBoxItem = new ListBoxItem()
                        {
                            Content = item.Value,
                            Tag = item.Key,
                            FontSize = lyrics.Settings.defaultFontSize,
                            FontWeight = lyrics.Settings.defaultFontWeight,
                            FontStyle = lyrics.Settings.defaultFontStyle,
                            Foreground = lyrics.Settings.defaultFontColor
                        };
                        listBoxItem.MouseUp += Lyrics_ListBoxItem_MouseUp;
                        lyrics.lyricsWindow.lyricsListBox.Items.Add(listBoxItem);
                    }
                }
                lyrics.lyricsWindow.Show();
            }
        }
        private void LoadSetting()
        {
            lyrics.lyricsWindow.defaultLyricsStyle.FontSize = lyrics.Settings.defaultFontSize;
            lyrics.lyricsWindow.defaultLyricsStyle.FontWeight = lyrics.Settings.defaultFontWeight;
            lyrics.lyricsWindow.defaultLyricsStyle.FontStyle = lyrics.Settings.defaultFontStyle;
            lyrics.lyricsWindow.defaultLyricsStyle.Foreground = lyrics.Settings.defaultFontColor;

            lyrics.lyricsWindow.selectedLyricsStyle.FontSize = lyrics.Settings.selectedFontSize;
            lyrics.lyricsWindow.selectedLyricsStyle.FontWeight = lyrics.Settings.selectedFontWeight;
            lyrics.lyricsWindow.selectedLyricsStyle.FontStyle = lyrics.Settings.selectedFontStyle;
            lyrics.lyricsWindow.selectedLyricsStyle.Foreground = lyrics.Settings.selectedFontColor;

            lyrics.lyricsWindow.desktopLyrics1Style.FontSize = lyrics.Settings.desktopLine1FontSize;
            lyrics.lyricsWindow.desktopLyrics1Style.FontWeight = lyrics.Settings.desktopLine1FontWeight;
            lyrics.lyricsWindow.desktopLyrics1Style.FontStyle = lyrics.Settings.desktopLine1FontStyle;
            lyrics.lyricsWindow.desktopLyrics1Style.Foreground = lyrics.Settings.desktopLine1FontColor;

            lyrics.lyricsWindow.desktopLyrics2Style.FontSize = lyrics.Settings.desktopLine2FontSize;
            lyrics.lyricsWindow.desktopLyrics2Style.FontWeight = lyrics.Settings.desktopLine2FontWeight;
            lyrics.lyricsWindow.desktopLyrics2Style.FontStyle = lyrics.Settings.desktopLine2FontStyle;
            lyrics.lyricsWindow.desktopLyrics2Style.Foreground = lyrics.Settings.desktopLine2FontColor;

            lyrics.lyricsWindow.lyricsListBox.HorizontalContentAlignment = lyrics.Settings.HorizontalContentAlignment;
            lyrics.lyricsWindow.LyricsParagraph.SelectedIndex = lyrics.Settings.HorizontalContentAlignment == HorizontalAlignment.Left ? 0 :
                (lyrics.Settings.HorizontalContentAlignment == HorizontalAlignment.Center ? 1 : 2);

        }
        private void Lyrics_ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = (TimeSpan)((ListBoxItem)sender).Tag;
            foreach (var item in lyrics.Timers) { item.Stop(); }
            lyrics.SelectLyrics(mediaElement.Position + TimeSpan.FromTicks(1), VarEls.timer.Interval);
            sliderPosition.Value = mediaElement.Position.TotalSeconds;
        }
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            sliderPosition.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            VarEls.timer.Stop();
            VarEls.timer = new DispatcherTimer { Interval = VarEls.timer.Interval };
            VarEls.timer.Tick += new EventHandler(Timer_tick);
            VarEls.timer.Start();
            if (lyrics.Load(mediaElement.Source))
            { ShowLyrics(); }
            else
            {
                if (lyrics.lyricsWindow != null)
                {
                    lyrics.lyricsWindow.lyricsListBox.Items.Clear();
                }
            }
            lyrics.SelectLyrics(mediaElement.Position, VarEls.timer.Interval);
            WriteLog(lyrics.Source);
            WriteLog(lyrics.LrcSource + "=>" + File.Exists(lyrics.LrcSource));
            foreach (var item in lyrics.LrcLines)
            { WriteLogText(item.Key + item.Value); }
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (playMode.IsChecked == true)
            {
                mediaElement.Position = TimeSpan.FromSeconds(0);
            }
            else
            {
                if (FilesList.SelectedIndex + 1 < FilesList.Items.Count)
                { FilesList.SelectedIndex += 1; }
                else { FilesList.SelectedIndex = 0; }
                sliderPosition.Value = 0;
            }
        }
        private void Timer_tick(object sender, EventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    sliderPosition.Value = mediaElement.Position.TotalSeconds;
            //    lyrics.SelectLyrics(mediaElement.Position, TimeSpan.FromMilliseconds(VarEls.timer.Interval));
            //}), null);
            if (!movingPosition)
            {
                sliderPosition.Value = mediaElement.Position.TotalSeconds;
            }
            lyrics.SelectLyrics(mediaElement.Position, VarEls.timer.Interval);
        }
        private bool movingPosition = false;
        private void SliderPosition_LostMouseCapture(object sender, MouseEventArgs e)
        {
            movingPosition = false;
            WriteLog(" movingPosition = false;");
            mediaElement.Position = TimeSpan.FromSeconds(sliderPosition.Value);
            lyrics.SelectLyrics(mediaElement.Position, VarEls.timer.Interval);
        }
        private void SliderPosition_GotMouseCapture(object sender, MouseEventArgs e)
        {
            movingPosition = true;
            WriteLog(" movingPosition = true;");
        }
        private void SliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            positionTimeEcho.Text = Math.Round(((Slider)sender).Value / 60).ToString("00") + "'" + Math.Round(((Slider)sender).Value % 60).ToString("00") + "\"";

        }
        private void VolumeSlider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            WriteLog(mediaElement.Volume);
        }
        private void ShowLyricsWin_StateChange(object sender, RoutedEventArgs e)
        {
            if (lyrics.lyricsWindow == null)
            {
                lyrics.lyricsWindow = new LyricsWindow() { Owner = this };
                lyrics.lyricsWindow.Closed += new EventHandler(lyrics.CloseLRCWindow);
                lyrics.lyricsWindow.SettingPanel.MouseLeave += new MouseEventHandler(SettingPanel_MouseLeave);
                ShowLyrics();
                LoadSetting();
            }
            else
            {
                lyrics.CloseWindow();
            }
        }
        private void SettingPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                lyrics.Settings.selectedFontColor = (SolidColorBrush)lyrics.lyricsWindow.selectedLyricsStyle.Foreground;
                lyrics.Settings.defaultFontColor = (SolidColorBrush)lyrics.lyricsWindow.defaultLyricsStyle.Foreground;
                lyrics.Settings.desktopLine1FontColor = (SolidColorBrush)lyrics.lyricsWindow.desktopLyrics1Style.Foreground;
                lyrics.Settings.desktopLine2FontColor = (SolidColorBrush)lyrics.lyricsWindow.desktopLyrics2Style.Foreground;

                lyrics.Settings.selectedFontSize = lyrics.lyricsWindow.selectedLyricsStyle.FontSize;
                lyrics.Settings.defaultFontSize = lyrics.lyricsWindow.defaultLyricsStyle.FontSize;
                lyrics.Settings.desktopLine1FontSize = lyrics.lyricsWindow.desktopLyrics1Style.FontSize;
                lyrics.Settings.desktopLine2FontSize = lyrics.lyricsWindow.desktopLyrics2Style.FontSize;

                lyrics.Settings.selectedFontWeight = lyrics.lyricsWindow.selectedLyricsStyle.FontWeight;
                lyrics.Settings.defaultFontWeight = lyrics.lyricsWindow.defaultLyricsStyle.FontWeight;
                lyrics.Settings.desktopLine1FontWeight = lyrics.lyricsWindow.desktopLyrics1Style.FontWeight;
                lyrics.Settings.desktopLine2FontWeight = lyrics.lyricsWindow.desktopLyrics2Style.FontWeight;

                lyrics.Settings.selectedFontStyle = lyrics.lyricsWindow.selectedLyricsStyle.FontStyle;
                lyrics.Settings.defaultFontStyle = lyrics.lyricsWindow.defaultLyricsStyle.FontStyle;
                lyrics.Settings.desktopLine1FontStyle = lyrics.lyricsWindow.desktopLyrics1Style.FontStyle;
                lyrics.Settings.desktopLine2FontStyle = lyrics.lyricsWindow.desktopLyrics2Style.FontStyle;

                lyrics.Settings.HorizontalContentAlignment = lyrics.lyricsWindow.lyricsListBox.HorizontalContentAlignment;
            }
            catch (Exception) { }
        }
        private void Do_backup_Click(object sender, RoutedEventArgs e)
        {
            lyrics.ShowLogWindow();
        }

        private void ShowDesktopLyricsWin_StateChange(object sender, RoutedEventArgs e)
        {
            if (lyrics.desktopLyrics == null)
            {
                lyrics.desktopLyrics = new DesktopLyrics() { Owner = this };
                lyrics.desktopLyrics.Closed += new EventHandler(lyrics.DLRCWindow_Closed);
                ShowDLyrics();
            }
            else
            {
                lyrics.CloseDLRCWindow();
            }
        }
        private void ShowDLyrics()
        {
            if (lyrics.desktopLyrics != null)
            {
                lyrics.desktopLyrics.Show();
            }
        }


        #endregion

        private void OpenDir_Click(object sender, RoutedEventArgs e)
        {
            var result = VarEls.musicDir.ShowDialog();
            if (result != Forms.DialogResult.OK) return;
            LoadMusicList(null);
        }


    }
}