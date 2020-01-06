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

namespace MusicPlayerWithLyrics
{
    class Lyrics
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
        private List<TimeSpan> timeList = null; // 用于二进制导航的列表

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
                    catch (Exception err) { WriteErrLog(err.Message); }

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
            try
            {
                if (lyricsWindow != null && HaveLrc && timeList != null)
                {
                    SwitchtoIndex(
                        timeList.IndexOf(
                        timeList.LongCount(lambda => lambda > lyricsTime) == 0
                        ? timeList.Last()
                       : timeList.Last(lambda => lambda < lyricsTime)
                       ));
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
                        catch (Exception err) { WriteErrLog(err.Message); }
                    }
                }
            }
            catch (Exception err) { WriteErrLog(err.Message); }
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
            catch (Exception err) { WriteErrLog(err.Message); }
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
            if (lyricsWindow.ScrolltoView_able)
            {
                lyricsWindow.lyricsListBox.ScrollIntoView(item);
            }
        }
        #region LogWindow
        public LogOutput logWindow = null;
        //public void CloseLogWindow(object sender, EventArgs e) { CloseWindow(); }
        //public void CloseWindow()
        //{
        //    lyricsWindow.Close();
        //    lyricsWindow = null;
        //}
        public void ShowLogWindow()
        {
            if (logWindow == null || logWindow.IsDisposed)
            {
                logWindow = new LogOutput()
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                logWindow.Show();

            }
            else
            {
                logWindow.Activate();
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
            if (!(logWindow == null || logWindow.IsDisposed))
            {
                logWindow.AddLog(Environment.NewLine + "|" + DateTime.Now.ToString("hh:mm:ss") + "_INFO>" + text);
            }
        }
        public void WriteTestLog(int text) => WriteTestLog(Convert.ToString(text));
        public void WriteTestLog(string text)
        {
            if (!(logWindow == null || logWindow.IsDisposed))
            {
                logWindow.AddLog(Environment.NewLine + "|" + DateTime.Now.TimeOfDay + "_TEST>" + text, System.Drawing.Color.Blue);
            }
        }
        public void WriteErrLog(string text)
        {
            if (!(logWindow == null || logWindow.IsDisposed))
            {
                logWindow.AddLog(Environment.NewLine + "|" + DateTime.Now.ToString("hh:mm:ss") + "_ERROR>" + text, System.Drawing.Color.Red);
            }
        }
        #endregion
    }
}
