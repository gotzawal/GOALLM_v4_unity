using UnityEngine;
using System;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] data, string clipName = "GeneratedAudioClip")
    {
        // Create a memory stream from the byte array
        using (var memoryStream = new System.IO.MemoryStream(data))
        {
            return ToAudioClip(memoryStream, clipName);
        }
    }

    private static AudioClip ToAudioClip(System.IO.Stream stream, string clipName)
    {
        using (var reader = new System.IO.BinaryReader(stream))
        {
            // Read WAV header
            string chunkID = new string(reader.ReadChars(4));
            if (chunkID != "RIFF")
            {
                throw new FormatException("Invalid WAV file. Expected 'RIFF' header.");
            }

            reader.ReadInt32(); // Chunk Size
            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
            {
                throw new FormatException("Invalid WAV file. Expected 'WAVE' format.");
            }

            // Read chunks until we find the 'data' sub-chunk
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string subChunkID = new string(reader.ReadChars(4));
                int subChunkSize = reader.ReadInt32();

                if (subChunkID == "data")
                {
                    // This is the audio data we need
                    byte[] wavData = reader.ReadBytes(subChunkSize);

                    // Convert WAV data to float samples
                    int bitsPerSample = 16; // Assuming 16-bit PCM as default
                    float[] samples = ConvertWavDataToFloat(wavData, bitsPerSample);

                    // Create an AudioClip
                    AudioClip audioClip = AudioClip.Create(clipName, samples.Length / 2, 2, 44100, false);
                    audioClip.SetData(samples, 0);

                    return audioClip;
                }
                else
                {
                    // Skip this chunk
                    reader.BaseStream.Position += subChunkSize;
                }
            }
            throw new FormatException("Invalid WAV file. 'data' sub-chunk not found.");
        }
    }

    private static float[] ConvertWavDataToFloat(byte[] wavData, int bitsPerSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        int totalSamples = wavData.Length / bytesPerSample;

        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            int sampleIndex = i * bytesPerSample;

            // Read sample and convert to float
            if (bitsPerSample == 16)
            {
                short sample = BitConverter.ToInt16(wavData, sampleIndex);
                samples[i] = sample / 32768.0f; // Convert from short to float (-1 to 1)
            }
            else if (bitsPerSample == 8)
            {
                sbyte sample = (sbyte)wavData[sampleIndex];
                samples[i] = sample / 128.0f; // Convert from byte to float (-1 to 1)
            }
            else
            {
                throw new FormatException("Unsupported WAV bit depth. Only 8-bit and 16-bit are supported.");
            }
        }
        return samples;
    }
}