using System.IO;
using UnityEngine;

public class AudioFileMaker : MonoBehaviour
{
    [SerializeField]
    public AudioSource Audio;

    [SerializeField]
    public string ByteFileName = "audio/myaudio.bytes";

    #region Button Listener
    public void LoadSound()
    {
        string soundfile = Application.streamingAssetsPath + "/" + ByteFileName;
        byte[] buffer = File.ReadAllBytes(soundfile);
        Debug.Log("Buffer read:" + buffer.Length);
        float[] samples = ConvertByteToFloat16(buffer);
        AudioClip clip = AudioClip.Create("NewWavClip", buffer.Length / 2, 1, 48000, false);
        clip.SetData(samples, 0);
        Audio.clip = clip;
        Audio.Play();
    }

    public void SaveSound()
    {
        AudioClip clip = Audio.clip;
        float[] samples = new float[clip.samples];
        bool ok = clip.GetData(samples, 0);
        if (ok)
        {
            byte[] buffer = Float2PCM16(samples);
            WriteFileWithBytes(ByteFileName, buffer);
        }
    }

    #endregion

    void WriteFileWithBytes(string filename, byte[] buffer)
    {
        string path = Application.streamingAssetsPath + "/" + filename;
        File.WriteAllBytes(path, buffer);
        Debug.Log("Total of " + buffer.Length + " bytes is written to " + path);
    }

    const int rescaleFactor = 32767;
    /// <summary>
    ///   convert float -> short (int16) -> bytes
    /// </summary>
    /// <param name="data"></param>
    static byte[] Float2PCM16(float[] data)
    {
        byte[] buffer = new byte[data.Length * 2];
        int i = 0;
        foreach (var t in data)
        {
            var sample = t;
            if (sample > 1) sample = 1;
            else if (sample < -1) sample = -1;

            var shortData = (short)(sample * rescaleFactor);
            var byteArr = new byte[2];
            byteArr = System.BitConverter.GetBytes(shortData);
            buffer[i * 2] = byteArr[0];
            buffer[i * 2 + 1] = byteArr[1];
            i++;
        }
        return buffer;
    }

    static float[] ConvertByteToFloat16(byte[] byteArray)
    {
        var floatArray = new float[byteArray.Length / 2];
        for (var i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] = System.BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
        }

        return floatArray;
    }

}