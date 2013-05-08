using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Threading;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;

namespace KinectTest
{
    public partial class Form1 : Form
    {
        private const string kinectAudioID = "SR_MS_en-US_Kinect_11.0";
        Thread audioThread;
        KinectSensor kinect;
        KinectAudioSource audioSource;
        SpeechRecognitionEngine speechRecognitionEngine;

        Stream audioStream;

        public Form1()
        {
            kinect = KinectSensor.KinectSensors.First();
            kinect.Start();

            audioThread = new Thread(new ThreadStart(captureAudio));
            audioThread.SetApartmentState(ApartmentState.MTA);
            audioThread.Start();

            InitializeComponent();
        }

        private void captureAudio()
        {
            this.audioSource = kinect.AudioSource;
            this.audioSource.AutomaticGainControlEnabled = false;
            this.audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            this.audioSource.EchoCancellationMode = EchoCancellationMode.None;

            RecognizerInfo speechInfo = 
                SpeechRecognitionEngine.InstalledRecognizers().
                Where(r => r.Id == kinectAudioID).FirstOrDefault();

            speechRecognitionEngine = new SpeechRecognitionEngine(speechInfo);
            Choices words = new Choices();
            words.Add("on");
            words.Add("off");

            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = speechInfo.Culture;
            grammarBuilder.Append(words);

            Grammar grammar = new Grammar(grammarBuilder);


            speechRecognitionEngine.LoadGrammar(grammar);
            this.speechRecognitionEngine.SpeechRecognized +=
                speechRecognitionEngine_SpeechRecognized;

            this.speechRecognitionEngine.SpeechDetected += 
                speechRecognitionEngine_SpeechDetected;

            audioStream = audioSource.Start();
            speechRecognitionEngine.SetInputToAudioStream(audioStream,
                new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

        }

        private void speechRecognitionEngine_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            
        }

        private void speechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.5)
            {
                MessageBox.Show(e.Result.Text);
                using(StreamWriter sw = new StreamWriter(File.Open("C:\\dev\\www\\index.html", FileMode.Create, FileAccess.Write)))
                {
                    sw.Write(speechToBool(e.Result.Text).ToString());
                }
                
            }
        }

        private int speechToBool(string speechResult)
        {
            if (speechResult == "on")
                return 1;
            else
                return 0;
        }
    }
}
