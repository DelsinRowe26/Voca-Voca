using System;
using System.Collections.Generic;
using CSCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimpleNeurotuner
{
    class SampleDSPRecord : ISampleSource
    {
        ISampleSource mSource;
        //public float[] freq;
        public SampleDSPRecord(ISampleSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            mSource = source;
            PitchShift = 1;
        }
        public /*async Task<int>*/ int Read(float[] buffer, int offset, int count)
        {
            try
            {
                //double[] buffer1 = new double[count];
                //double closestfreq = 0;
                float gainAmplification = (float)(Math.Pow(10.0, (GainDB) / 20.0));//получить Усиление
                int samples = mSource.Read(buffer, offset, count);//образцы
                float AmpSrR = 0, AmpSr, Kamp, dKamp, stK;                                                  //if (gainAmplification != 1.0f) 
                                                                                                            //{
                for (int i = offset; i < offset + samples; i++)
                {
                    //buffer[i] = Math.Max(Math.Min(buffer[i] * gainAmplification, 1), -1);
                    AmpSrR += Math.Abs(buffer[i]);
                }

                PitchShifter.AmpDSPR += AmpSrR;
                PitchShifter.ItDSPR += samples;

                if (PitchShifter.ItDSP >= 5000)
                {
                    AmpSr = PitchShifter.AmpDSP / PitchShifter.ItDSP;
                    AmpSrR = PitchShifter.AmpDSPR / PitchShifter.ItDSPR;
                    PitchShifter.AmpDSP = 0;
                    PitchShifter.AmpDSPR = 0;
                    PitchShifter.ItDSP = 0;
                    PitchShifter.ItDSPR = 0;
                    PitchShifter.Kampp = PitchShifter.Kamp;
                    if (Math.Abs(AmpSrR) < 0.001f)
                        Kamp = 0;
                    else
                        Kamp = AmpSr / AmpSrR;
                    if (Kamp < 0.05)
                        Kamp = 0;
                    Kamp = (float)((int)((Kamp + 0.05) * 10)) / 10;
                    PitchShifter.Kamp = Kamp;
                }
                if (PitchShifter.Kamp < 0)
                {
                    Kamp = 0;
                    dKamp = 0;
                }
                else
                {
                    dKamp = PitchShifter.Kamp - PitchShifter.Kampp;
                    Kamp = PitchShifter.Kampp;
                }
                for (int i = offset; i < offset + samples; i++)
                {
                    Kamp += dKamp / (float)(samples);
                    buffer[i] = Math.Max(Math.Min(buffer[i] * Kamp, 1), -1);
                    //if (Math.Abs(buffer[i]) < 0.001f)
                    //    buffer[i] = 0;
                }
                PitchShifter.Kampp = PitchShifter.Kamp;
                ///<summary>
                ///int len = buffer.Length;
                ///freq = buffer;
                ///await Task.Run(() => FrequencyUtils.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 31, 16000));
                ///FrequencyUtilsRec.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 30, mSource.WaveFormat.SampleRate / 2);
                ///freq = FrequencyUtils.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 31, 16000);
                ///await Task.Run(() => PitchShifter.FindClosestNote(FrequencyUtils.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 31, 16000), out closestfreq));
                ///PitchShifter.FindClosestNote(FrequencyUtilsRec.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 30, mSource.WaveFormat.SampleRate / 2), out closestfreq);
                ///await Task.Run(() => File.WriteAllText("ClosestFreq.txt", closestfreq.ToString()));
                ///File.WriteAllText("FreqClosestRec.txt", closestfreq.ToString());
                ///await Task.Run(() => File.AppendAllText("Freq.txt", FrequencyUtils.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 31, 16000).ToString("f3") + "\n"));
                ///File.AppendAllText("FreqRecord.txt", FrequencyUtilsRec.FindFundamentalFrequency(buffer, mSource.WaveFormat.SampleRate, 30, mSource.WaveFormat.SampleRate / 2).ToString("f3") + "\n");
                ///}
                ///</summary>

                /*PitchShifter.PitchShift(PitchShift, offset, count, 4096, 4, mSource.WaveFormat.SampleRate, buffer);

                if (PitchShift != 1.0f)
                {
                    //FrequencyUtils.FindFundamentalFrequency(buffer1, mSource.WaveFormat.SampleRate, 60, 22050);
                    PitchShifter.PitchShift(PitchShift, offset, count, 4096, 4, mSource.WaveFormat.SampleRate, buffer);

                }*/

                return samples;
            }
            catch
            {
                return 0; 
            }
        }

        public float GainDB { get; set; }

        public float PitchShift { get; set; }

        public bool CanSeek
        {
            get { return mSource.CanSeek; }
        }

        public WaveFormat WaveFormat
        {
            get { return mSource.WaveFormat; }
        }

        public long Position
        {
            get
            {
                return mSource.Position;
            }
            set
            {
                mSource.Position = value;
            }
        }

        public long Length
        {
            get { return mSource.Length; }
        }

        public void Dispose()
        {
            if (mSource != null) mSource.Dispose();
        }
    }
}
