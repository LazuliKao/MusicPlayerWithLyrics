using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerWithLyrics
{
    public partial class LogOutput : Form
    {
        public LogOutput()
        {
            InitializeComponent();
        }
        public void AddLog(string text)
        {
            int iLoca = logbox.TextLength;
            logbox.AppendText(text);
            logbox.SelectionStart = iLoca;
            logbox.SelectionLength = logbox.TextLength;
            logbox.SelectionColor = Color.Black;
            logbox.SelectionStart = logbox.TextLength;
            logbox.Focus();
        }
        public void AddLog(string text, Color textColor)
        {
            int iLoca = logbox.TextLength;
            logbox.AppendText(text);
            logbox.SelectionStart = iLoca;
            logbox.SelectionLength = logbox.TextLength;
            logbox.SelectionColor = textColor;
            logbox.SelectionStart = logbox.TextLength;
            logbox.Focus();
        }
    }
}
