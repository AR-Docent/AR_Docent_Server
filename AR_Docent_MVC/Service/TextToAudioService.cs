using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using AR_Docent_MVC.Config;
using Azure.Storage.Blobs;
using System.Threading;
using Microsoft.Extensions.Azure;
using System.Collections.Generic;
using System.Linq;

namespace AR_Docent_MVC.Service
{
    public class TextToAudioService
    {
        private SpeechConfig _speechConfig;
        private SpeechSynthesizer _speechSynthesizer;
        private AzureKeyVaultService _azureKey;

        public TextToAudioService(AzureKeyVaultService azurekey)
        {
            _azureKey = azurekey;

            Task.Run(async () =>
            {
                while (_azureKey.blobConnectionString == null || _azureKey.sqlConnectionString == null || _azureKey.speechConnectionString == null)
                {
                    Thread.Sleep(100);
                }
                _speechConfig = SpeechConfig.FromSubscription(_azureKey.speechConnectionString, ServerConfig.region);
                _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);
                _speechConfig.SpeechSynthesisVoiceName = "ko-KR-SunHiNeural";
                _speechSynthesizer = new SpeechSynthesizer(_speechConfig, null);
                /*
                using var result = await _speechSynthesizer.GetVoicesAsync();
                if (result.Reason == ResultReason.VoicesListRetrieved)
                {
                    Debug.WriteLine($"Found {result.Voices.Count} voices");
                    foreach (var voice in result.Voices)
                    {
                        Debug.WriteLine(voice.Name);
                    }
                }
                */
            });
        }
        
        private void OutputSpeechSynthesisResult(SpeechSynthesisResult speechResult, string text)
        {
            switch (speechResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    Debug.WriteLine($"Speech synthesized for text: [{text}]");
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechResult);
                    Debug.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Debug.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Debug.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Debug.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
                default:
                    break;
            }
        }

        private byte[] MakeWaveHeader(int fileSize, int hz, int channel, int bit)
        {
            byte[] header = new byte[44];
            //chunkID
            header[0] = (byte)'R';
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            //ChunkSize
            int size = fileSize - 8;
            header[4] = (byte)(size & 0xFF);
            header[5] = (byte)((size >> 8) & 0xFF);
            header[6] = (byte)((size >> 16) & 0xFF);
            header[7] = (byte)((size >> 24) & 0xFF);
            //format
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            //ChunkID
            header[12] = (byte)'f';
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            //ChunkSize
            int c_size = 16;
            header[16] = (byte)(c_size & 0xFF);
            header[17] = (byte)((c_size >> 8) & 0xFF);
            header[18] = (byte)((c_size >> 16) & 0xFF);
            header[19] = (byte)((c_size >> 24) & 0xFF);
            //AudioFormat
            header[20] = (byte)0x01;
            header[21] = (byte)0x00;
            //channels
            header[22] = (byte)0x01;
            header[23] = (byte)0x00;
            //sample rate
            header[24] = (byte)(hz & 0xFF);
            header[25] = (byte)((hz >> 8) & 0xFF);
            header[26] = (byte)((hz >> 16) & 0xFF);
            header[27] = (byte)((hz >> 24) & 0xFF);
            //average byte rate per sec
            int av = hz * channel;
            header[28] = (byte)(av & 0xFF);
            header[29] = (byte)((av >> 8) & 0xFF);
            header[30] = (byte)((av >> 16) & 0xFF);
            header[31] = (byte)((av >> 24) & 0xFF);
            //block align
            header[32] = (byte)channel;
            header[33] = (byte)0x00;
            //bit per sample
            header[34] = (byte)bit;
            header[35] = (byte)0;
            //Chunk ID
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            //chunk Size
            int d_size = fileSize - 44;
            header[40] = (byte)(d_size & 0xFF);
            header[41] = (byte)((d_size >> 8) & 0xFF);
            header[42] = (byte)((d_size >> 16) & 0xFF);
            header[43] = (byte)((d_size >> 24) & 0xFF);

            return header;
        }

        public async Task<byte[]> TextToSpeech(string txt)
        {
            Debug.WriteLine("run");
            SpeechSynthesisResult speechSynthesisResult = await _speechSynthesizer.SpeakTextAsync(txt);
            AudioDataStream stream = AudioDataStream.FromResult(speechSynthesisResult);
            OutputSpeechSynthesisResult(speechSynthesisResult, txt);

            byte[] data;

            data = new byte[speechSynthesisResult.AudioData.Length - 44];
            stream.ReadData(0, data);

            //Riff24Khz16BitMonoPcm
            //build header
            byte[] header = MakeWaveHeader(speechSynthesisResult.AudioData.Length,
                24000, 1, 16);

            //combine header and data
            List<byte> datalst = new List<byte>();

            datalst.AddRange(header);
            datalst.AddRange(data);

            byte[] buffer = datalst.ToArray();

            stream.Dispose();
            return buffer;
        }
    }
}
