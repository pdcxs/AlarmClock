using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AlarmClock.Commands;
using System.Speech.Synthesis;
using System.Threading;

namespace AlarmClock
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HideConfigWindowCommand hideCommand = new HideConfigWindowCommand();
        private SpeechSynthesizer synth = new SpeechSynthesizer();
        private Parameter parameter = new Parameter();
        private Thread alarmThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowWindow(object sender, RoutedEventArgs e)
        {
            show_MenuItem.Command.Execute(MyNotifyIcon);
        }

        private void Notifier_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide the config window
            hideCommand.Execute(MyNotifyIcon);

            alarmThread = new Thread(new ThreadStart(AlarmProc));
            alarmThread.Start();

            Reset();
        }

        private void Reset()
        {
            parameter.LoadConfig();
            // Get the speakers information

            int index = -1;
            var voices = synth.GetInstalledVoices();
            for (int i = 0; i < voices.Count; i++)
            {
                VoiceInfo info = voices[i].VoiceInfo;
                speaker_Combo.Items.Add(info.Name);
                if (parameter.speakerName == info.Name)
                {
                    index = i;
                }
            }
            
            if (index == -1)
            {
                speaker_Combo.SelectedIndex = 0;
            }
            else
            {
                speaker_Combo.SelectedIndex = index;
            }
            parameter.speakerName = speaker_Combo.SelectedValue as string;

            // Get the delays
            foreach (int i in parameter.delays)
            {
                delay_Combo.Items.Add(i.ToString());
            }
            delay_Combo.SelectedIndex = parameter.delayIndex;

            // Get the precision
            precision_Slider.Value = parameter.precision;

            // Get Volume
            volume_Slider.Value = parameter.speechVolume;

            // Get Rate
            rate_Slider.Value = parameter.speechRate;

            // Is 12-Hour
            if (parameter.is12Hour)
            {
                twelve_Check.IsChecked = true;
            }
            else
            {
                twelve_Check.IsChecked = null;
            }

            // Is Half-Call
            if (parameter.isHalfCall)
            {
                halfCall_Check.IsChecked = true;
            }
            else
            {
                halfCall_Check.IsChecked = false;
            }

            // Get Time Name
            foreach (string name in parameter.timeName)
            {
                timeName_Combo.Items.Add(name);
            }

            hour_Slider.Value = 12;

            // Get Speech Tail
            hourTail_Slider.Value = 12;
            speechTail_Text.Text = parameter.speechTail[12];

            // Speech Head
            speechHead_Text.Text = parameter.speechHead;
        }

        private void WindowState_Changed(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                hideCommand.Execute(MyNotifyIcon);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            QuitCommand t = close_MenuItem.Command as QuitCommand;
            if (t.IsClosed() == true)
            {
                MyNotifyIcon.Dispose();
                alarmThread.Abort();

                base.OnClosing(e);
            }
            else
            {
                hideCommand.Execute(MyNotifyIcon);
                e.Cancel = true;
            }
        }

        private void Delay_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            parameter.delayIndex = delay_Combo.SelectedIndex;
        }

        private void Speaker_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            parameter.speakerName = speaker_Combo.SelectedValue.ToString();
        }

        private void PrecisionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            parameter.precision = Convert.ToInt32(e.NewValue);
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            parameter.speechVolume = Convert.ToInt32(e.NewValue);
        }

        private void RateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            parameter.speechRate = Convert.ToInt32(e.NewValue);
        }

        private void Twelve_CheckChange(object sender, RoutedEventArgs e)
        {
            parameter.is12Hour = twelve_Check.IsChecked ?? false;
        }

        private void HalfCall_CheckChange(object sender, RoutedEventArgs e)
        {
            parameter.isHalfCall = halfCall_Check.IsChecked ?? false;
        }

        private void TimeNameChanged(object sender, SelectionChangedEventArgs e)
        {
            int hour = Convert.ToInt32(hour_Slider.Value);
            parameter.timeNameIndex[hour] = timeName_Combo.SelectedIndex;
        }

        private void HourSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hour = Convert.ToInt32(e.NewValue);
            timeName_Combo.SelectedIndex = parameter.timeNameIndex[hour];
        }

        private void Speak()
        {
            var time = DateTime.Now;
            int minute = time.Minute;
            int hour = time.Hour;

            string content = parameter.speechHead;
            if (parameter.is12Hour)
            {
                string timeName = parameter.timeName[parameter.timeNameIndex[hour]];
                int twelveHour = hour;
                if (hour == 0)
                {
                    twelveHour = 12;
                } else if (hour > 12)
                {
                    twelveHour -= 12;
                }
                content += timeName + twelveHour.ToString() + "点";
            } else
            {
                content += hour.ToString() + "点";
            }

            if (minute == 0 && parameter.isHalfCall)
            {
                content += "整。";
            } else if (minute == 30 && parameter.isHalfCall)
            {
                content += "半。";
            } else
            {
                content += minute.ToString() + "分。";
            }

            if (minute == 0)
            {
                content += parameter.speechTail[hour] + "。";
            }

            synth.SelectVoice(parameter.speakerName);
            synth.Rate = parameter.speechRate;
            synth.Volume = parameter.speechVolume;
            synth.Speak(content);
        }

        private void TestButtonClick(object sender, RoutedEventArgs e)
        {
            Speak();
        }

        private void SpeechHeadChanged(object sender, TextChangedEventArgs e)
        {
            parameter.speechHead = speechHead_Text.Text;
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void AlarmProc()
        {
            bool hasSpoken = false;
            while (true)
            {
                var time = DateTime.Now;
                int minute = time.Minute;

                int delay = parameter.delays[parameter.delayIndex];

                if (minute % delay == 0 && !hasSpoken)
                {
                    Speak();
                    hasSpoken = true;

                    int min = parameter.delays[0];
                    for (int i = 1; i < parameter.delays.Length; i++)
                    {
                        if (parameter.delays[i] < min)
                        {
                            min = parameter.delays[i];
                        }
                    }
                    Thread.Sleep((min - 1) * 60 * 1000);
                } else if (minute % delay != 0)
                {
                    hasSpoken = false;
                    Thread.Sleep(parameter.precision * 1000);
                }
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            parameter.WriteConfig();
        }

        private void QuitButtonClick(object sender, RoutedEventArgs e)
        {
            close_MenuItem.Command.Execute(MyNotifyIcon);
        }

        private void SpeechTailChanged(object sender, TextChangedEventArgs e)
        {
            int index = Convert.ToInt32(hourTail_Slider.Value);
            parameter.speechTail[index] = speechTail_Text.Text;
        }

        private void HourTailSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int hour = Convert.ToInt32(hourTail_Slider.Value);
            speechTail_Text.Text = parameter.speechTail[hour];
        }
    }
}

