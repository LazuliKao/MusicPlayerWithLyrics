using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using Forms = System.Windows.Forms;
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
            ShowTip("初始化完成...");
            ShowTip("请设置好相关选项...");
            drawerHost.Visibility = Visibility.Visible;
        }
        public void Button_click(Button object_name, RoutedEventArgs perform_event)
        {
            object_name.RaiseEvent(perform_event);
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
            notifyIcon.Text = "桌面备份工具";
            System.Drawing.Icon icon = ResourcesResx.icon.Icon1;
            notifyIcon.Icon = icon;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(2000);
            notifyIcon.MouseClick += new Forms.MouseEventHandler(NotifyIcon_MouseClick);
            ////设置菜单项
            //System.Windows.Forms.MenuItem menu1 = new System.Windows.Forms.MenuItem("test");
            Forms.MenuItem active = new Forms.MenuItem("Active");
            active.Click += new EventHandler(Active_Window);
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
        public void Active_Window(object sender, EventArgs e)
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
                Active_Window(null, null);
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
        public void ShowTip(string text, string action_text, Action action) => snackbar.MessageQueue.Enqueue(text, action_text, action);
        public void ShowTip(string text, string action_text) => snackbar.MessageQueue.Enqueue(text, action_text, () => WriteLog("Clicked=>\"" + text + "\""));
        public void ShowTip(string text) => snackbar.MessageQueue.Enqueue(text, null, null, null, false, false, TimeSpan.FromSeconds(1));
        private void Clear_log(object sender, RoutedEventArgs e)
        {
            logOutput.Document.Blocks.Clear();
            ShowTip("清理成功");
        }
        #endregion
        #region 读写配置项
        public string Ini_read(string path, string title)
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
        public string Ini_write(string path, string title, string content)
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
        private struct DirString
        {
            public string runingDir;
            public Forms.FolderBrowserDialog desktop;
            public Forms.FolderBrowserDialog backup;
            public string ini_file_path;
            public DispatcherTimer timer;
            //public bool timerStarted;
        }
        private DirString VarEls = new DirString
        {
            runingDir = Environment.CurrentDirectory + "\\",
            ini_file_path = Environment.CurrentDirectory + "\\config.ini",
            desktop = new Forms.FolderBrowserDialog(),
            backup = new Forms.FolderBrowserDialog(),
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) },
            //timerStarted = false
        };
        #endregion
        #region 保存配置
        private void SaveIntButton_Click(object sender, RoutedEventArgs e)
        {
            Ini_write(VarEls.ini_file_path, "desktopDir", VarEls.desktop.SelectedPath.Trim());
        }
        #endregion
        #region 窗体 控件
        private void Function_box_SizeChanged(object sender, SizeChangedEventArgs e) => logOutput.Margin = new Thickness(0, function_box.ActualHeight, 0, 0);
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
            mediaElement.Source = new Uri(File_path, UriKind.Relative);
            playBtn.IsEnabled = true;
        }
        private void PlayerPause()
        {
            //SetPlayer(true);
            if (playBtn.Content.ToString() == "Play")
            {
                VarEls.timer.Start();
                mediaElement.Play();
                playBtn.Content = "Pause";
                mediaElement.ToolTip = "Click to Pause";
            }
            else
            {
                VarEls.timer.Stop();
                mediaElement.Pause();
                playBtn.Content = "Play";
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
                        var listBoxItem = new ListBoxItem() { Content = item.Value, Tag = item.Key };
                        listBoxItem.MouseUp += Lyrics_ListBoxItem_MouseUp;
                        lyrics.lyricsWindow.lyricsListBox.Items.Add(listBoxItem);
                    }
                }
                lyrics.lyricsWindow.Show();
            }
        }
        private void Lyrics_ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = (TimeSpan)((ListBoxItem)sender).Tag;
            foreach (var item in lyrics.Timers) { item.Stop(); }
            lyrics.SelectLyrics(mediaElement.Position+TimeSpan.FromTicks(1), VarEls.timer.Interval);
            sliderPosition.Value = mediaElement.Position.TotalSeconds;
        }
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            sliderPosition.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            //if (!VarEls.timerStarted)
            //{
            //    VarEls.timer.Tick += new EventHandler(Timer_tick);
            //    VarEls.timerStarted = true;
            //}
            VarEls.timer.Stop();
            VarEls.timer = new DispatcherTimer { Interval = VarEls.timer.Interval };
            VarEls.timer.Tick += new EventHandler(Timer_tick);
            VarEls.timer.Start();
            if (lyrics.Load(mediaElement.Source))
            {
                ShowLyrics();
            }
            lyrics.SelectLyrics(mediaElement.Position, VarEls.timer.Interval);
            WriteLog(lyrics.Source);
            WriteLog(lyrics.LrcSource + "=>" + File.Exists(lyrics.LrcSource));
            foreach (var item in lyrics.LrcLines)
            {
                WriteLogText(item.Key + item.Value);
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
        #endregion
        private bool movingPosition = false;
        private void SliderPosition_LostMouseCapture(object sender, MouseEventArgs e)
        {
            movingPosition = false;
            WriteLog(" movingPosition = false;");
            mediaElement.Position = TimeSpan.FromSeconds(sliderPosition.Value);
        }
        private void SliderPosition_GotMouseCapture(object sender, MouseEventArgs e)
        {
            movingPosition = true;
            WriteLog(" movingPosition = true;");
        }
        private void SliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => positionTimeEcho.Text = Math.Round(((Slider)sender).Value / 60).ToString("00") + "'" + Math.Round(((Slider)sender).Value % 60).ToString("00") + "\"";
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
                ShowLyrics();
            }
            else
            {
                lyrics.CloseWindow();
            }
        }

        private void Do_backup_Click(object sender, RoutedEventArgs e)
        {
            lyrics.ShowLogWindow();
        }
    }
}