using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoAwkwardSilence
{
    public partial class MainForm : Form
    {
        private Audio audio_ = new Audio();
        private List<AudioSession> sessionList_;
        private AudioSession defaultSession_;
        private int awkwardMeter_ = 0;

        public MainForm()
        {
            InitializeComponent();
            delayTrackBar.Value = Properties.Settings.Default.Delay;
            toleranceTrackBar.Value = Properties.Settings.Default.Tolerance;
            
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            sourceListBox.Items.Clear();
            sessionList_ = audio_.GetAudioSessionList();
            foreach (var session in sessionList_)
            {
                sourceListBox.Items.Add(session.name);
            }
        }


        private void startBtn_Click(object sender, EventArgs e)
        {
           if (sourceListBox.CheckedItems.Count > 0)
           {
               string sessionName = sourceListBox.CheckedItems[0].ToString();
               foreach (var session in sessionList_)
               {
                   if (session.name.Equals(sessionName))
                   {
                       defaultSession_ = session;
                       timer1.Start();
                       splitContainer.Panel1.Enabled = false;
                       startBtn.Enabled = false;
                       stopBtn.Enabled = true;
                       break;
                   }
               }
           }
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            splitContainer.Panel1.Enabled = true;
            startBtn.Enabled = true;
            stopBtn.Enabled = false;
        }

        private void sourceListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked && sourceListBox.CheckedItems.Count > 0)
            {
                sourceListBox.ItemCheck -= sourceListBox_ItemCheck;
                sourceListBox.SetItemChecked(sourceListBox.CheckedIndices[0], false);
                sourceListBox.ItemCheck += sourceListBox_ItemCheck;
            }

            if(e.NewValue == CheckState.Unchecked && sourceListBox.CheckedItems.Count <= 1)
            {
                splitContainer.Panel2.Enabled = false;
            }
            else
            {
                splitContainer.Panel2.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            logTextBox.Clear();
            if (audio_.IsAwkward(defaultSession_, toleranceTrackBar.Value))
            {
                logTextBox.AppendText("Sound source: Queued\n");
                if (awkwardMeter_ > delayTrackBar.Value*10)
                {
                    logTextBox.Clear();
                    logTextBox.AppendText("Sound source: ON\n");
                    audio_.UnmuteSession(defaultSession_);
                }
                awkwardMeter_++;
            }
            else
            {
                logTextBox.AppendText("Sound source: OFF\n");
                awkwardMeter_ = 0;
                audio_.MuteSession(defaultSession_);
            }
        }

        private void muteRadio_CheckedChanged(object sender, EventArgs e)
        {
            if(muteRadio.Checked)
            {
                splitContainer.Show();
            }
            else
            {
                splitContainer.Hide();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            audio_.UnmuteSession(defaultSession_);
            Properties.Settings.Default.Delay = delayTrackBar.Value;
            Properties.Settings.Default.Tolerance = toleranceTrackBar.Value;
            Properties.Settings.Default.Save();
        }

    }

}
