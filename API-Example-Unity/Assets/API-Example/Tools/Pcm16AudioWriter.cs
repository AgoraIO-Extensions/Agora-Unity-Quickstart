using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Agora_RTC_Plugin.API_Example
{
    public class Pcm16AudioWriter
    {
        private string filePath;
        private int sampleRate;
        private int channels;
        private List<byte> pcmDataBuffer;
        private bool isDisposed;

        /// <summary>
        /// Creates a PCM16 audio writer
        /// </summary>
        /// <param name="fileName">File name (without extension)</param>
        /// <param name="sampleRate">Sample rate (e.g., 44100)</param>
        /// <param name="channels">Number of audio channels (1=mono, 2=stereo)</param>
        public Pcm16AudioWriter(string fileName, int sampleRate, int channels)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty");

            if (sampleRate <= 0)
                throw new ArgumentException("Sample rate must be greater than 0");

            if (channels != 1 && channels != 2)
                throw new ArgumentException("Number of channels must be 1 or 2");

            // Use Unity's sandbox path
            string directory = Application.persistentDataPath;
            this.filePath = Path.Combine(directory, fileName + ".wav");
            this.sampleRate = sampleRate;
            this.channels = channels;
            this.pcmDataBuffer = new List<byte>();
            this.isDisposed = false;

            Debug.Log($"Audio file will be saved to: {this.filePath}");
        }

        /// <summary>
        /// Adds PCM16 data to the buffer
        /// </summary>
        /// <param name="data">Byte array in PCM16 format (2 bytes per sample)</param>
        public void PutData(byte[] data)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Pcm16AudioWriter");

            if (data == null || data.Length == 0)
            {
                Debug.LogWarning("Received data is empty");
                return;
            }

            // Check if data length is a multiple of 2 (PCM16 uses 2 bytes per sample)
            if (data.Length % 2 != 0)
            {
                Debug.LogWarning("PCM16 data length should be a multiple of 2, current data may be truncated");
            }

            // Add data to buffer
            lock (pcmDataBuffer)
            {
                pcmDataBuffer.AddRange(data);
            }

            // Debug.Log($"Received {data.Length} bytes of data, buffer total size: {pcmDataBuffer.Count} bytes");
        }

        /// <summary>
        /// Writes buffer data to disk as a WAV file
        /// </summary>
        public void Flush()
        {
            if (isDisposed)
                throw new ObjectDisposedException("Pcm16AudioWriter");

            byte[] dataToWrite;
            lock (pcmDataBuffer)
            {
                if (pcmDataBuffer.Count == 0)
                {
                    Debug.LogWarning("No data to write, please call PutData to add data first");
                    return;
                }

                dataToWrite = pcmDataBuffer.ToArray();
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // Write WAV file header
                    WriteWavHeader(writer, dataToWrite.Length);

                    // Write PCM data
                    writer.Write(dataToWrite);
                }

                Debug.Log($"Audio file successfully written: {filePath}");
                Debug.Log($"File size: {new FileInfo(filePath).Length} bytes");
                Debug.Log($"Audio duration: {CalculateDuration(dataToWrite.Length)} seconds");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write audio file: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Writes the standard WAV file header
        /// </summary>
        private void WriteWavHeader(BinaryWriter writer, int dataSize)
        {
            // RIFF header
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize); // Total file size - 8
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            // fmt sub-chunk
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16); // PCM format header size
            writer.Write((ushort)1); // Audio format (1 = PCM)
            writer.Write((ushort)channels); // Number of channels
            writer.Write(sampleRate); // Sample rate
            writer.Write(sampleRate * channels * 2); // Byte rate
            writer.Write((ushort)(channels * 2)); // Block align
            writer.Write((ushort)16); // Bit depth (16-bit)

            // data sub-chunk
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize); // Data size
        }

        /// <summary>
        /// Calculates audio duration (seconds)
        /// </summary>
        private float CalculateDuration(int dataSize)
        {
            int sampleCount = dataSize / (channels * 2); // 16-bit = 2 bytes
            return (float)sampleCount / sampleRate;
        }

        /// <summary>
        /// Cleans up resources
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                pcmDataBuffer?.Clear();
                isDisposed = true;
                Debug.Log("Pcm16AudioWriter resources released");
            }
        }

        /// <summary>
        /// Gets the complete file path
        /// </summary>
        public string GetFilePath()
        {
            return filePath;
        }

        /// <summary>
        /// Gets the current buffer size
        /// </summary>
        public int GetBufferSize()
        {
            lock (pcmDataBuffer)
            {
                return pcmDataBuffer.Count;
            }
        }
    }
}