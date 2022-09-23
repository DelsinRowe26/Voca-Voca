// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;

namespace Voca_Voca
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private FileInfo fileInfo1 = new FileInfo("Data_Load.tmp");

        private SimpleMixer mMixer, mMixerRight;
        private int SampleRate;//441
        //private Equalizer equalizer;
        private WasapiOut mSoundOut, mSoundOut1;
        private WasapiCapture mSoundIn, mSoundIn1;
        private SampleDSPPitch mDsp;
        private SampleDSPTurbo mDspTurbo;
        private IWaveSource mSource;
        private ISampleSource mMp3;
        private MMDeviceCollection mOutputDevices;
        private MMDeviceCollection mInputDevices;

        private int click = 0;
        private int ImgBtnStartClick = 0, ImgBtnTurboClick = 0;

        string langindex;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Voca_Voca_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SoftCl.IsSoftwareInstalled("Microsoft Visual C++ 2015-2022 Redistributable (x86) - 14.32.31332") == false)
                {
                    Process.Start("VC_redist.x86.exe");
                }



                //Находит устройства для захвата звука и заполнияет комбобокс
                MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
                mInputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
                MMDevice activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);

                SampleRate = activeDevice.DeviceFormat.SampleRate;

                foreach (MMDevice device in mInputDevices)
                {
                    cmbInput.Items.Add(device.FriendlyName);
                    if (device.DeviceID == activeDevice.DeviceID) cmbInput.SelectedIndex = cmbInput.Items.Count - 1;
                }


                //Находит устройства для вывода звука и заполняет комбобокс
                activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                mOutputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

                foreach (MMDevice device in mOutputDevices)
                {
                    cmbOutput.Items.Add(device.FriendlyName);
                    if (device.DeviceID == activeDevice.DeviceID) cmbOutput.SelectedIndex = cmbOutput.Items.Count - 1;
                }

                string[] filename = File.ReadAllLines(fileInfo1.FullName);
                if (filename.Length == 1)
                {
                    Languages();
                }
                if (!File.Exists("log.tmp"))
                {
                    File.Create("log.tmp").Close();
                }
                else
                {
                    if (File.ReadAllLines("log.tmp").Length > 1000)
                    {
                        File.WriteAllText("log.tmp", " ");
                    }
                }

                if (click == 0)
                {
                    if (langindex == "0")
                    {
                        string msg = "Подключите проводную аудио-гарнитуру к компьютеру.\nЕсли на данный момент гарнитура не подключена,\nто подключите проводную гарнитуру, и перезапустите программу для того, чтобы звук подавался в наушники.";
                        MessageBox.Show(msg);
                    }
                    else
                    {
                        string msg = "Connect a wired audio headset to your computer.\nIf a headset is not currently connected,\nthen connect a wired headset and restart the program so that the sound is played through the headphones.";
                        MessageBox.Show(msg);
                    }
                    click++;
                }

                TembroClass tembro = new TembroClass();
                tembro.Tembro(SampleRate, "Wide_voice_effect.tmp");
                
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Loaded: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Loaded: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void Mixer()
        {
            try
            {

                mMixer = new SimpleMixer(2, SampleRate) //стерео, 44,1 КГц
                {
                    //Right = true,
                    //Left = true,
                    FillWithZeros = true,
                    DivideResult = true, //Для этого установлено значение true, чтобы избежать звуков тиков из-за превышения -1 и 1.
                };
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Mixer: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Mixer: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void Stop()
        {
            try
            {
                if (mMixer != null)
                {
                    mMixer.Dispose();
                    mMp3.ToWaveSource(32).Loop().ToSampleSource().Dispose();
                    mMixer = null;
                }
                if (mSoundOut != null)
                {
                    mSoundOut.Stop();
                    mSoundOut.Dispose();
                    mSoundOut = null;
                }
                if (mSoundIn != null)
                {
                    mSoundIn.Stop();
                    mSoundIn.Dispose();
                    mSoundIn = null;
                }
                if (mSource != null)
                {
                    mSource.Dispose();
                    mSource = null;
                }
                if (mMp3 != null)
                {
                    mMp3.Dispose();
                    mMp3 = null;
                }
            }
            catch (Exception ex)
            {
                /*if (langindex == "0")
                {
                    string msg = "Ошибка в Stop: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Stop: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }*/
            }
        }

        private void SoundIn()
        {
            mSoundIn = new WasapiCapture(/*false, AudioClientShareMode.Exclusive, 1*/);
            Dispatcher.Invoke(() => mSoundIn.Device = mInputDevices[cmbInput.SelectedIndex]);
            mSoundIn.Initialize();
            mSoundIn.Start();
        }

        private void Voca_Voca_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
            Environment.Exit(0);
        }

        private void SoundOut()
        {
            try
            {

                mSoundOut = new WasapiOut(/*false, AudioClientShareMode.Exclusive, 1*/);
                Dispatcher.Invoke(() => mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex]);
                //mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex];



                mSoundOut.Initialize(mMixer.ToWaveSource(32).ToStereo());


                mSoundOut.Play();
                mSoundOut.Volume = 10;
                //return ChannelMask.SpeakerFrontLeft;
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в SoundOut: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in SoundOut: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private async void StartFullDuplex()
        {
            try
            {
                //Запускает устройство захвата звука с задержкой 1 мс.
                //await Task.Run(() => SoundIn());
                SoundIn();
                
                var source = new SoundInSource(mSoundIn) { FillWithZeros = true };

                //Init DSP для смещения высоты тона
                mDsp = new SampleDSPPitch(source.ToSampleSource().ToStereo());

                //SetPitchShiftValue();

                //Инициальный микшер
                //Mixer();

                //Добавляем наш источник звука в микшер
                mMixer.AddSource(mDsp.ChangeSampleRate(mDsp.WaveFormat.SampleRate));

                
                //Запускает устройство воспроизведения звука с задержкой 1 мс.
                await Task.Run(() => SoundOut());

            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в StartFullDuplex: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in StartFullDuplex: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStartTurbo.IsEnabled = false;
            btnStart.IsEnabled = false;
            cmbInput.IsEnabled = false;
            cmbOutput.IsEnabled = false;
            ImgBtnStartClick = 1;
            await Task.Run(() => Sound());
            StartFullDuplex();
        }

        private async void btnStartTurbo_Click(object sender, RoutedEventArgs e)
        {
            btnStartTurbo.IsEnabled = false;
            btnStart.IsEnabled = false;
            cmbInput.IsEnabled = false;
            cmbOutput.IsEnabled = false;
            ImgBtnTurboClick = 1;
            Sound();
            StartFullDuplexTurbo();
        }

        private void btnStart_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"Voca-Voca\button\button-play-hover.png";
            ImgBtnStart.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnStartTurbo_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"Voca-Voca\button\button-turbo-hover.png";
            ImgBtnTurboStart.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnStart_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImgBtnStartClick == 1)
            {
                string uri = @"Voca-Voca\button\button-play-active.png";
                ImgBtnStart.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
            else
            {
                string uri = @"Voca-Voca\button\button-play-inactive.png";
                ImgBtnStart.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
        }

        private void btnStartTurbo_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImgBtnTurboClick == 1)
            {
                string uri = @"Voca-Voca\button\button-turbo-active.png";
                ImgBtnTurboStart.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
            else
            {
                string uri = @"Voca-Voca\button\button-turbo-inactive.png";
                ImgBtnTurboStart.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
        }

        private async void StartFullDuplexTurbo()//запуск пича и громкости
        {
            try
            {
                SoundIn();
                
                var source = new SoundInSource(mSoundIn) { FillWithZeros = true };

                //Init DSP для смещения высоты тона
                mDspTurbo = new SampleDSPTurbo(source.ToSampleSource().ToStereo());

                //Добавляем наш источник звука в микшер
                mMixer.AddSource(mDspTurbo.ChangeSampleRate(mMixer.WaveFormat.SampleRate));

                //Запускает устройство воспроизведения звука с задержкой 1 мс.
                await Task.Run(() => SoundOut());

            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в StartFullDuplexTurbo: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in StartFullDuplexTurbo: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
            //return false;
        }

        private async void Sound()
        {
            try
            {
                    Mixer();
                    mMp3 = CodecFactory.Instance.GetCodec(@"Voca-Voca\record\Muisc.wav").ToStereo().ToSampleSource();
                    //mDspRec = new SampleDSPPitch(mMp3.ToWaveSource(32).ToSampleSource());
                    //SampleRate = mDspRec.WaveFormat.SampleRate;
                    mMixer.AddSource(mMp3.ChangeSampleRate(mMp3.WaveFormat.SampleRate));
                    //await Task.Run(() => SoundOut());
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void Languages()
        {
            try
            {
                StreamReader FileLanguage = new StreamReader("Data_Language.tmp");
                File.WriteAllText("Data_Load.tmp", "1");
                File.WriteAllText("DataTemp.tmp", "0");
                langindex = FileLanguage.ReadToEnd();
                if (langindex == "0")
                {
                    cmbInput.ToolTip = "Микрофон";
                    cmbOutput.ToolTip = "Динамики";
                    btnStart.ToolTip = "Старт";
                    btnStartTurbo.ToolTip = "Старт Турбо";
                    lbSetMicr.Content = "Выбор микрофона";
                    lbSetSpeak.Content = "Выбор динамиков";
                }
                else if (langindex != "0")
                {
                    lbSetMicr.Content = "Microphone selection";
                    lbSetSpeak.Content = "Speaker selection";
                    btnStart.ToolTip = "Start";
                    btnStartTurbo.ToolTip = "Start Turbo";
                }
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Languages: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Languages: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }
    }
}
