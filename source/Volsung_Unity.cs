
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

class Volsung_Unity : MonoBehaviour
{
    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern int VLSNG_create_program();

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern void VLSNG_run(int handle, [MarshalAs(UnmanagedType.LPArray)] float[] data);

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern void VLSNG_interpret_program(int handle, [MarshalAs(UnmanagedType.LPStr)] string program, bool stereo = false);

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern void VLSNG_register_parameters(int handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)] string[] names, int length);

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern void VLSNG_update_parameters(int handle, [MarshalAs(UnmanagedType.LPArray)] float[] values, int length);

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern void VLSNG_initialize();

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern IntPtr VLSNG_read_debug_text();

    [DllImport("Assets/Volsung/libVolsungShared")]
    private static extern void VLSNG_clear_debug_text();

    public TextAsset file;
    public List<string> parameterNames = new List<string>();
    public List<float> parameterValues = new List<float>();
    public int numParameters = 0;

    public bool stereo = false;
    public bool compileOnAwake = false;
    bool programCompiledAsStereo;

    volatile bool audioLock = true;
    volatile bool compileLock = false;

    int handle;
    const int blocksize = 256;
    float[] audioData;
    Queue<float> queue = new Queue<float>(4096);

    public void SetParameter(string name, float value) {
        for (int n = 0; n < parameterNames.Count; n++) {
            if (name == parameterNames[n])
            {
                parameterValues[n] = value;
                return;
            }
        }
    }

    public float GetParameter(string name)
    {
        float value = 0.0f;
        for (int n = 0; n < parameterNames.Count; n++)
        {
            if (name == parameterNames[n]) value = parameterValues[n];
        }
        return value;
    }

    void PrintErrors()
    {
        string error = Marshal.PtrToStringAnsi(VLSNG_read_debug_text());
        // if (error != "Parsed successfully!") print(error);
        VLSNG_clear_debug_text();
    }

    void Awake()
    {
        VLSNG_initialize();
        handle = VLSNG_create_program();
        if (compileOnAwake) Compile();
    }

    public void Compile()
    {
        while (compileLock);
        audioLock = true;

        VLSNG_register_parameters(handle, parameterNames.ToArray(), parameterNames.Count);
        VLSNG_interpret_program(handle, "\n" + file.text + "\n", stereo);
        programCompiledAsStereo = stereo;
        audioData = new float[blocksize * (programCompiledAsStereo ? 2 : 1)];
        PrintErrors();

        audioLock = false;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (audioLock) return;
        compileLock = true;

        int samplesNeeded = programCompiledAsStereo ? (data.Length / channels * 2) : data.Length / channels;
        while (queue.Count < samplesNeeded) {
            VLSNG_update_parameters(handle, parameterValues.ToArray(), numParameters);

            VLSNG_run(handle, audioData);
            if (programCompiledAsStereo) {
                for (int n = 0; n < audioData.Length / 2; n++) {
                    queue.Enqueue(audioData[n]);
                    queue.Enqueue(audioData[n + blocksize]);
                }
            }

            else {
                for (int n = 0; n < audioData.Length; n++) {
                    queue.Enqueue(audioData[n]);
                }
            }
        }

        if (programCompiledAsStereo) {
            for (int n = 0; n < data.Length / channels; n++) {
                for (int c = 0; c < channels; c++) {
                    if (c > 2) data[n * channels + c] = 0.0f;
                    else data[n * channels + c] = queue.Dequeue();
                }
            }
        }

        else {
            for (int n = 0; n < data.Length / channels; n++) {
                float value = queue.Dequeue();
                for (int c = 0; c < channels; c++) {
                    data[n * channels + c] = value;
                }
            }
        }

        compileLock = false;
    }
}
