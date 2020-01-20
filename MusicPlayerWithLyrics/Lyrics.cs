using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Forms;
using Application = System.Windows.Application;
using System.Drawing;
using FontStyle = System.Windows.FontStyle;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace MusicPlayerWithLyrics
{
    class Lyrics : IDisposable
    {
        #region 信息输出
        #endregion
        public string Source { get; set; } = null; //Path
        public string LrcSource { get; set; } = null; //Path
        public bool HaveLrc { get; set; } = false; //Path
        public SortedList<TimeSpan, string> LrcLines = new SortedList<TimeSpan, string>();//歌词数据
        //public SortedDictionary<TimeSpan, string> LrcLines = new SortedDictionary<TimeSpan, string>();//歌词数据
        public string SongName { get; set; } = "无信息"; //标题
        public string Artist { get; set; } = "无信息"; //歌手
        public string Album { get; set; } = "无信息"; //专辑
        public string Author { get; set; } = "无信息"; //作词家
        public int Length { get; set; } = 0; //音乐的长度
        public string By { get; set; } = "无信息"; //LRC 作者
        public LogOutput LogWindow { get; set; } = null;

        private List<TimeSpan> timeList = null; // 用于二进制导航的列表
        public struct VarEls
        {
            public double selectedFontSize, defaultFontSize, desktopLine1FontSize, desktopLine2FontSize;
            public FontStyle selectedFontStyle, defaultFontStyle, desktopLine1FontStyle, desktopLine2FontStyle;
            public FontWeight selectedFontWeight, defaultFontWeight, desktopLine1FontWeight, desktopLine2FontWeight;
            public SolidColorBrush selectedFontColor, defaultFontColor, desktopLine1FontColor, desktopLine2FontColor;

            public HorizontalAlignment HorizontalContentAlignment;
        }
        public VarEls Settings = new VarEls
        {
            selectedFontSize = 18,
            selectedFontStyle = FontStyles.Normal,
            selectedFontWeight = FontWeights.Normal,
            selectedFontColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            defaultFontSize = 18,
            defaultFontStyle = FontStyles.Normal,
            defaultFontWeight = FontWeights.Normal,
            defaultFontColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            desktopLine1FontSize = 18,
            desktopLine1FontStyle = FontStyles.Normal,
            desktopLine1FontWeight = FontWeights.Normal,
            desktopLine1FontColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            desktopLine2FontSize = 18,
            desktopLine2FontStyle = FontStyles.Normal,
            desktopLine2FontWeight = FontWeights.Normal,
            desktopLine2FontColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        public bool Load(Uri uri)
        {
            Source = uri.ToString();
            LrcSource = Path.GetDirectoryName(Source) + '\\' + Path.GetFileNameWithoutExtension(Source) + ".lrc";
            return ReadAllLyricsLines(LrcSource);
        }
        public bool ReadAllLyricsLines(string lrcPath)
        {
            //WriteLog("获取歌词中");
            LrcLines.Clear();
            timeList = null;
            if (File.Exists(lrcPath))
            {
                foreach (var item in File.ReadAllLines(lrcPath))
                {
                    //WriteLog(item);
                    try
                    {
                        int start = item.IndexOf(']');
                        string time_text = item.Substring(1, start - 1);
                        TimeSpan time = TimeSpan.FromSeconds(double.Parse(time_text.Substring(time_text.IndexOf(':') + 1))) +
                        TimeSpan.FromMinutes(double.Parse(time_text.Remove(time_text.IndexOf(':'))));
                        if (LrcLines.ContainsKey(time))
                        {
                            LrcLines[time] += Environment.NewLine + item.Substring(start + 1);
                        }
                        else
                        {
                            LrcLines.Add(time, item.Substring(start + 1));
                        }
                    }
                    catch (Exception err) { WriteErrLog(err.ToString()); }

                }
                timeList = LrcLines.Keys.ToList();
            }
            HaveLrc = false;
            if (LrcLines.Count > 0)
            {
                HaveLrc = File.Exists(lrcPath);
            }
            return HaveLrc;
        }
        public LyricsWindow lyricsWindow = null;
        public void CloseLRCWindow(object sender, EventArgs e) { CloseWindow(); }
        public void CloseWindow()
        {
            lyricsWindow.Close();
            lyricsWindow = null;
        }
        public List<DispatcherTimer> Timers = new List<DispatcherTimer>();
        public void SelectLyrics(TimeSpan lyricsTime, TimeSpan cycle)
        {
            ChooseDesktopLyrics(lyricsTime, cycle);
            try
            {
                if (lyricsWindow != null && HaveLrc && timeList != null)
                {
                    try
                    {
                        SwitchtoIndex(
                                                timeList.IndexOf(
                                                timeList.Any(lambda => lambda > lyricsTime)
                                                ? timeList.Last(lambda => lambda < lyricsTime)
                                               : timeList.Last()
                       ));
                    }
                    catch { }
                    Timers.Clear();
                    foreach (var item in timeList.FindAll(lambda => lambda > lyricsTime && lambda - lyricsTime < cycle))
                    {
                        try
                        {
                            DispatcherTimer Timer = new DispatcherTimer { Interval = item - lyricsTime, Tag = timeList.IndexOf(item) };
                            Timer.Tick += new EventHandler(Timer_tick);
                            Timers.Add(Timer);
                            Timers.First(lambda => (int)lambda.Tag == timeList.IndexOf(item)).Start();
                        }
                        catch (Exception err) { WriteErrLog(err.ToString()); }
                    }
                }
            }
            catch (Exception err) { WriteErrLog(err.ToString()); }
        }
        private void Timer_tick(object sender, EventArgs e)
        {
            try
            {
                ((DispatcherTimer)sender).IsEnabled = false;
                SwitchtoIndex((int)((DispatcherTimer)sender).Tag);
                Timers.Remove(((DispatcherTimer)sender));
                WriteTestLog(LrcLines.Keys[(int)((DispatcherTimer)sender).Tag] + "=>" + LrcLines.Values[(int)((DispatcherTimer)sender).Tag]);
            }
            catch (Exception err) { WriteErrLog(err.ToString()); }
        }
        private void SwitchtoIndex(int index)
        {
            index = Math.Min(Math.Max(index, 0), lyricsWindow.lyricsListBox.Items.Count - 1);//排除超出界限的index
            if (index != lyricsWindow.lyricsListBox.SelectedIndex)
            {
                try
                {
                    ScrollIntoView(lyricsWindow.lyricsListBox.Items[index + (int)Math.Round((lyricsWindow.lyricsListBox.ActualHeight - ((ListBoxItem)lyricsWindow.lyricsListBox.SelectedItem).ActualHeight) / ((ListBoxItem)lyricsWindow.lyricsListBox.Items[index]).ActualHeight / 2)]);
                }
                catch { ScrollIntoView(lyricsWindow.lyricsListBox.SelectedItem); }
                lyricsWindow.lyricsListBox.SelectedIndex = index;
            }
            else
            {
                ScrollIntoView(lyricsWindow.lyricsListBox.SelectedItem);
            }
        }
        private void ScrollIntoView(object item)
        {
            if (lyricsWindow.ScrolltoViewAble)
            {
                lyricsWindow.lyricsListBox.ScrollIntoView(item);
            }
        }
        #region DesktopLyrics
        public DesktopLyrics desktopLyrics = null;
        public List<DispatcherTimer> DLTimers = new List<DispatcherTimer>();
        public void DLRCWindow_Closed(object sender, EventArgs e) => CloseDLRCWindow();
        public void ChooseDesktopLyrics(TimeSpan lyricsTime, TimeSpan cycle)
        {
            try
            {
                if (desktopLyrics != null && HaveLrc && timeList != null)
                {
                    try
                    {
                        SettoIndex(
                                            timeList.IndexOf
                                            (timeList.Any(lambda => lambda > lyricsTime)
                                            ? timeList.Last(lambda => lambda < lyricsTime)
                                            : timeList.Last()
                                         ));
                    }
                    catch { }
                    DLTimers.Clear();
                    foreach (var item in timeList.FindAll(lambda => lambda > lyricsTime && lambda - lyricsTime < cycle))
                    {
                        try
                        {
                            DispatcherTimer Timer = new DispatcherTimer { Interval = item - lyricsTime, Tag = timeList.IndexOf(item) };
                            Timer.Tick += new EventHandler(DLTimer_tick);
                            DLTimers.Add(Timer);
                            DLTimers.First(lambda => (int)lambda.Tag == timeList.IndexOf(item)).Start();
                        }
                        catch (Exception err) { WriteErrLog(err.ToString()); }
                    }
                }
            }
            catch (Exception err) { WriteErrLog(err.ToString()); }
        }
        private int selectedindex = 0;
        private void SettoIndex(int index)
        {
            if (selectedindex != index)
            {
                desktopLyrics.Line1.Text = index < timeList.Count && index >= 0
                    ? LrcLines[timeList[index]]
                    : null;
                desktopLyrics.Line2.Text = index + 1 < timeList.Count && index >= 0
                    ? LrcLines[timeList[index + 1]]
                    : null;
                desktopLyrics.Line1.Foreground = Settings.desktopLine1FontColor;
                desktopLyrics.Line1.FontStyle = Settings.desktopLine1FontStyle;
                desktopLyrics.Line1.FontWeight = Settings.desktopLine1FontWeight;
                desktopLyrics.Line1.FontSize = Settings.desktopLine1FontSize;
                desktopLyrics.Line2.Foreground = Settings.desktopLine2FontColor;
                desktopLyrics.Line2.FontStyle = Settings.desktopLine2FontStyle;
                desktopLyrics.Line2.FontWeight = Settings.desktopLine2FontWeight;
                desktopLyrics.Line2.FontSize = Settings.desktopLine2FontSize;
               
                desktopLyrics.Line1.HorizontalAlignment = Settings.HorizontalContentAlignment;
                desktopLyrics.Line2.HorizontalAlignment = Settings.HorizontalContentAlignment;
              

                selectedindex = index;
            }
        }

        private void DLTimer_tick(object sender, EventArgs e)
        {
            try
            {
                ((DispatcherTimer)sender).IsEnabled = false;
                SettoIndex((int)((DispatcherTimer)sender).Tag);
                DLTimers.Remove((DispatcherTimer)sender);
               WriteTestLog(LrcLines.Keys[(int)((DispatcherTimer)sender).Tag] + "=>" + LrcLines.Values[(int)((DispatcherTimer)sender).Tag]);
            }
            catch (Exception err) { WriteErrLog(err.ToString()); }
        }

        public void CloseDLRCWindow()
        {
            desktopLyrics.Close();
            desktopLyrics = null;
        }

        #endregion
        #region LogWindow
        //public void CloseLogWindow(object sender, EventArgs e) { CloseWindow(); }
        //public void CloseWindow()
        //{
        //    lyricsWindow.Close();
        //    lyricsWindow = null;
        //}
        public void ShowLogWindow()
        {
            if (LogWindow == null || LogWindow.IsDisposed)
            {
                LogWindow = new LogOutput()
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                LogWindow.Show();

            }
            else
            {
                LogWindow.Close();
                LogWindow.Dispose();
            }

            //logWindow.IsDisposed
            //logWindow = new LogOutput();
            //logWindow.Show();
            //logWindow.Close();
            //foreach (var item in Application.Current.Windows)
            //{
            //    if (item is LogOutput)
            //    {
            //        logWindow.Close();
            //        return;
            //    }
            //}
            //logWindow = new LogOutput();
            //logWindow.Show();
        }
        public void WriteLog(double text) => WriteLog(Convert.ToString(text));
        public void WriteLog(int text) => WriteLog(Convert.ToString(text));
        public void WriteLog(string text)
        {
            if (!(LogWindow == null || LogWindow.IsDisposed))
            {
                LogWindow.AddLog(Environment.NewLine + "|" + DateTime.Now.ToString("hh:mm:ss") + "_INFO>" + text);
            }
        }
        public void WriteTestLog(int text) => WriteTestLog(Convert.ToString(text));
        public void WriteTestLog(string text)
        {
            if (!(LogWindow == null || LogWindow.IsDisposed))
            {
                LogWindow.AddLog(Environment.NewLine + "|" + DateTime.Now.TimeOfDay + "_TEST>" + text, System.Drawing.Color.Blue);
            }
        }
        public void WriteErrLog(string text)
        {
            if (!(LogWindow == null || LogWindow.IsDisposed))
            {
                LogWindow.AddLog(Environment.NewLine + "|" + DateTime.Now.ToString("hh:mm:ss") + "_ERROR>" + text, System.Drawing.Color.Red);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
