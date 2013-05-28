using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectTest
{
    class KinectManager
    {
        private const string kinectAudioID = "SR_MS_en-US_Kinect_11.0";
        Thread audioThread;
        KinectSensor kinect;
        KinectAudioSource audioSource;
        SpeechRecognitionEngine speechRecognitionEngine;

        Stream audioStream;

        static KinectManager kinectManager = new KinectManager();
        private KinectManager() {

            try { kinect = KinectSensor.KinectSensors.First(); }
            catch { Console.WriteLine("Cannot find Kinect");  }
            
        }
        public static KinectManager getInstance()
        {
            return kinectManager;
        }

        public void run()
        {
            try
            {
                kinect.Start();
                audioThread = new Thread(new ThreadStart(captureAudio));
                audioThread.SetApartmentState(ApartmentState.MTA);
                audioThread.Name = "Audio Thread";
                audioThread.Start();
            }
            catch
            {
                Console.WriteLine("Unable to start the Kinect. Is one connected?");
            }

        }

        private void captureAudio()
        {
            try {
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
            catch
            {
                Console.WriteLine("Unable to initialize audio");
            }

        }

        private void speechRecognitionEngine_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {

        }

        public static int Result = 0; //find a better way to refactor
        private void speechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                if (e.Result.Confidence > 0.1)
                {
                    SwitchLight(e.Result.Text);
                    Result = speechToInt(e.Result.Text);
                }
            }
            catch { Console.WriteLine("Error processing data."); }
        }


        private void SwitchLight(string recognizedText)
        {
            byte result = (byte)speechToInt(recognizedText);
            Console.Out.WriteLine("RESULT: {0} ({1})", recognizedText, result);

           TcpSocketManager.getInstance().sendMessage(result);
           WebSockeManager.getInstance().sendMessage(result);
        }

        private int speechToInt(string speechResult)
        {
            int result;
            if (speechResult == "on")
                result = 1;
            else
                result = 0;

            return result;
        }
    
    }
}
