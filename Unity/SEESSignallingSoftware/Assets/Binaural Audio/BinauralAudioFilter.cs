using UnityEngine;
using System.Runtime.InteropServices;

using FFTWSharp;

public unsafe class BinauralAudioFilter : MonoBehaviour
{
    public SourceType SampleSource = SourceType.OneChannel;
    public AttenuationType Attenuation = AttenuationType.Square;

    public float Amplification = 20;
    public float MaxVolume = 3;

    public bool DisableConvolution = false;

    private HRIRAsset _ActiveHRIR = null;

    private float _Azimuth = 0;
    private float _Elevation = 0;
    private float _Radial = 0;
    private int _IAzimuth = 0;
    private int _IElevation = 0;

    private bool _FilterInitialized = false;
    private float _FilterGain = 1.0f;

    private fftwf_complexarray _InputBuffer = null;
    private fftwf_complexarray _LeftBuffer = null;
    private fftwf_complexarray _RightBuffer = null;
    private fftwf_plan _InputPlan = null;
    private fftwf_plan _LeftPlan = null;
    private fftwf_plan _RightPlan = null;

    private fftwf_complexarray _LeftHRTFBuffer = null;
    private fftwf_complexarray _RightHRTFBuffer = null;
    private fftwf_plan _LeftHRTFPlan = null;
    private fftwf_plan _RightHRTFPlan = null;

    private float[] _LeftOverlap = null;
    private float[] _RightOverlap = null;

    private object _LockObject = new object();

    void Start()
    {
        BinauralAudioListener listener = FindObjectOfType(typeof(BinauralAudioListener)) as BinauralAudioListener;

        if (listener == null)
            Debug.LogWarning("No Binaural Audio Listener in scene. Binaural audio processing disabled.", this);

        if (listener != null && listener.m_HRIR == null)
            Debug.LogWarning("No HRIR specified. Binaural audio processing disabled.", this);

        if (GetComponent<AudioSource>() == null)
            Debug.LogWarning(string.Format("There is no AudioSource attached to {0}. Binaural audio processing disabled.", this.name), this);

        if (GetComponent<AudioSource>() && GetComponent<AudioSource>().clip != null && GetComponent<AudioSource>().clip.channels != 2)
            Debug.LogWarning("This script can only run on 2 channel audio clips. Binaural audio processing disabled.", this);

        Update();
    }

    void Update()
    {
        BinauralAudioListener listener = FindObjectOfType(typeof(BinauralAudioListener)) as BinauralAudioListener;
        UpdateSphericalCoordinates(listener);
        UpdateFilterGain();
        UpdateHRIR(listener);
    }

    private void UpdateSphericalCoordinates(BinauralAudioListener listener)
    {
        if (listener == null)
            return;

        Vector3 forward = listener.transform.forward;
        Vector3 up = listener.transform.up;
        Vector3 right = listener.transform.right;
        Vector3 displacement = transform.position - listener.transform.position;
        Vector3 direction = (displacement.magnitude != 0 ? Vector3.Normalize(displacement) : forward);
        Vector3 sagittalDisplacement = displacement - Vector3.Dot(displacement, right) * right;
        Vector3 sagittalDirection = (sagittalDisplacement.magnitude != 0 ? Vector3.Normalize(sagittalDisplacement) : forward);

        _Elevation = Vector3.Angle(forward, sagittalDirection);
        _Azimuth = Vector3.Angle(direction, sagittalDirection);
        _Radial = displacement.magnitude;

        if (Vector3.Dot(sagittalDirection, up) < 0)
            _Elevation = -_Elevation;

        if (Vector3.Dot(direction, right) < 0)
            _Azimuth = -_Azimuth;

        if (_Elevation < -90)
            _Elevation += 360;
    }

    private void UpdateFilterGain()
    {
        _FilterGain = 1;
        _FilterGain /= (Attenuation == AttenuationType.Linear ? _Radial : 1.0f);
        _FilterGain /= (Attenuation == AttenuationType.Square ? _Radial * _Radial : 1.0f);
        _FilterGain *= Amplification;
        _FilterGain = Mathf.Min(_FilterGain, MaxVolume);
    }

    private void UpdateHRIR(BinauralAudioListener listener)
    {
        if (listener == null || listener.m_HRIR == null)
        {
            _FilterInitialized = false;
            _ActiveHRIR = null;
            return;
        }

        if (listener.m_HRIR != _ActiveHRIR)
        {
            if (_ActiveHRIR == null || listener.m_HRIR.Samples != _ActiveHRIR.Samples)
                _FilterInitialized = false;

            _ActiveHRIR = listener.m_HRIR;
        }

        int oldIAzimuth = _IAzimuth;
        int oldIElevation = _IElevation;
        _IAzimuth = _ActiveHRIR.GetNearestAzimuthIndex(_Azimuth);
        _IElevation = _ActiveHRIR.GetNearestElevationIndex(_Elevation);

        if (!_FilterInitialized)
        {
            InitializeDSP();
            LoadHRIR(_IAzimuth, _IElevation);
            _FilterInitialized = true;
        }
        else if (_IAzimuth != oldIAzimuth || _IElevation != oldIElevation)
        {
            LoadHRIR(_IAzimuth, _IElevation);
        }
    }

    private void InitializeDSP()
    {
        int inputSize = GetDSPBufferSize();
        int hrirSize = _ActiveHRIR.Samples;
        int n = inputSize + hrirSize;

        lock (_LockObject)
        {
            _InputBuffer = new fftwf_complexarray(n);
            _LeftBuffer = new fftwf_complexarray(n);
            _RightBuffer = new fftwf_complexarray(n);
            _InputPlan = fftwf_plan.dft_1d(n, _InputBuffer, _InputBuffer, fftw_direction.Forward, fftw_flags.Patient);
            _LeftPlan = fftwf_plan.dft_1d(n, _LeftBuffer, _LeftBuffer, fftw_direction.Backward, fftw_flags.Patient);
            _RightPlan = fftwf_plan.dft_1d(n, _RightBuffer, _RightBuffer, fftw_direction.Backward, fftw_flags.Patient);

            _LeftHRTFBuffer = new fftwf_complexarray(n);
            _RightHRTFBuffer = new fftwf_complexarray(n);
            _LeftHRTFPlan = fftwf_plan.dft_1d(n, _LeftHRTFBuffer, _LeftHRTFBuffer, fftw_direction.Forward, fftw_flags.Patient);
            _RightHRTFPlan = fftwf_plan.dft_1d(n, _RightHRTFBuffer, _RightHRTFBuffer, fftw_direction.Forward, fftw_flags.Patient);

            _LeftOverlap = new float[hrirSize];
            _RightOverlap = new float[hrirSize];
        }
    }

    private void LoadHRIR(int iAzimuth, int iElevation)
    {
        float[] leftHRIR = _ActiveHRIR.GetLeftHrir(iAzimuth, iElevation);
        float[] rightHRIR = _ActiveHRIR.GetRightHrir(iAzimuth, iElevation);

        lock (_LockObject)
        {
            int bufferSize = _LeftHRTFBuffer.Length;
            CComplex* leftData = (CComplex*)_LeftHRTFBuffer.Handle;
            CComplex* rightData = (CComplex*)_RightHRTFBuffer.Handle;

            for (int i = 0; i < _ActiveHRIR.Samples; i++)
            {
                leftData[i] = new CComplex(leftHRIR[i]);
                rightData[i] = new CComplex(rightHRIR[i]);
            }

            for (int i = _ActiveHRIR.Samples; i < bufferSize; i++)
            {
                leftData[i] = CComplex.Zero;
                rightData[i] = CComplex.Zero;
            }

            _LeftHRTFPlan.Execute();
            _RightHRTFPlan.Execute();

            RecalculateOverlap();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_FilterInitialized || channels != 2 || DisableConvolution)
            return;

        lock (_LockObject)
        {
            CalculateFilteredData(ref data);
        }
    }

    void CalculateFilteredData(ref float[] data)
    {
        int inputSize = data.Length / 2;
        int bufferSize = _InputBuffer.Length;
        int overlapSize = _LeftOverlap.Length;
        CComplex* inPtr = (CComplex*)_InputBuffer.Handle;
        CComplex* leftPtr = (CComplex*)_LeftBuffer.Handle;
        CComplex* rightPtr = (CComplex*)_RightBuffer.Handle;
        CComplex* leftHRTFPtr = (CComplex*)_LeftHRTFBuffer.Handle;
        CComplex* rightHRTFPtr = (CComplex*)_RightHRTFBuffer.Handle;

        // Copy input samples into the input buffer.
        for (int i = 0; i < inputSize; i++)
        {
            float inSample = data[i * 2];

            if (SampleSource == SourceType.TwoChannels)
                inSample = (inSample + data[i * 2 + 1]) / 2;

            inPtr[i] = new CComplex(inSample);
        }

        // Pad the rest of the input with zeroes.
        for (int i = inputSize; i < bufferSize; i++)
        {
            inPtr[i] = CComplex.Zero;
        }

        // Take the forward FFT of input data.
        _InputPlan.Execute();

        // Calculate left and right audio data by applying
        // the corresponding HRTFs to the input data.
        for (int i = 0; i < bufferSize; i++)
        {
            leftPtr[i] = inPtr[i] * leftHRTFPtr[i];
            rightPtr[i] = inPtr[i] * rightHRTFPtr[i];
        }

        // Take the backward FFT of resulting data.
        _LeftPlan.Execute();
        _RightPlan.Execute();

        // FFTW computes unnormalized FFTs, so the output signal needs to be scaled by 1/n.
        float scale = 1.0f / bufferSize;

        // Place left and right audio signals back into the audio buffer with overlap.
        // Copy the new overlap into the overlap buffers
        for (int i = 0; i < overlapSize; i++)
        {
            float leftSample = leftPtr[i]._r * scale;
            float rightSample = rightPtr[i]._r * scale;

            data[i * 2] = leftSample + _LeftOverlap[i];
            data[i * 2 + 1] = rightSample + _RightOverlap[i];

            int j = i + inputSize;
            _LeftOverlap[i] = leftPtr[j]._r * scale;
            _RightOverlap[i] = rightPtr[j]._r * scale;
        }

        // Place remaining data back into the audio buffer without overlap.
        for (int i = overlapSize; i < inputSize; i++)
        {
            data[i * 2] = leftPtr[i]._r * scale;
            data[i * 2 + 1] = rightPtr[i]._r * scale;
        }

        for (int i = 0; i < inputSize; i++)
        {
            float leftSample = data[i * 2];
            float rightSample = data[i * 2 + 1];

            data[i * 2] = Mathf.Min(leftSample * _FilterGain, 1);
            data[i * 2 + 1] = Mathf.Min(rightSample * _FilterGain, 1);
        }
    }

    private void RecalculateOverlap()
    {
        if (!_FilterInitialized)
            return;

        //int inputSize = _BackupBuffer.Length;
        int inputSize = GetDSPBufferSize();
        int bufferSize = _InputBuffer.Length;
        int overlapSize = _LeftOverlap.Length;
        CComplex* inPtr = (CComplex*)_InputBuffer.Handle;
        CComplex* leftPtr = (CComplex*)_LeftBuffer.Handle;
        CComplex* rightPtr = (CComplex*)_RightBuffer.Handle;
        CComplex* leftHRTFPtr = (CComplex*)_LeftHRTFBuffer.Handle;
        CComplex* rightHRTFPtr = (CComplex*)_RightHRTFBuffer.Handle;

        // Calculate left and right audio data by applying
        // the corresponding HRTFs to the input data.
        for (int i = 0; i < bufferSize; i++)
        {
            leftPtr[i] = inPtr[i] * leftHRTFPtr[i];
            rightPtr[i] = inPtr[i] * rightHRTFPtr[i];
        }

        // Take the backward FFT of resulting data.
        _LeftPlan.Execute();
        _RightPlan.Execute();

        // FFTW computes unnormalized FFTs, so the output signal needs to be scaled by 1/n.
        float scale = 1.0f / bufferSize;

        // Copy the overlap into the overlap buffers.
        for (int i = 0; i < overlapSize; i++)
        {
            int j = i + inputSize;
            _LeftOverlap[i] = leftPtr[j]._r * scale;
            _RightOverlap[i] = rightPtr[j]._r * scale;
        }
    }

    private int GetDSPBufferSize()
    {
        int bufferSize = 0;
        int numBuffers = 0;
        AudioSettings.GetDSPBufferSize(out bufferSize, out numBuffers);

        return bufferSize;
    }

    #region enums

    public enum SourceType
    {
        OneChannel,
        TwoChannels
    }

    public enum AttenuationType
    {
        None,
        Linear,
        Square,
        Logarithmic
    }

    #endregion
}
