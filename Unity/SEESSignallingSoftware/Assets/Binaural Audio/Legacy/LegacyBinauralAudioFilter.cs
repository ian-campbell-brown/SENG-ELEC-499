using UnityEngine;

public class LegacyBinauralAudioFilter : MonoBehaviour
{
    private AudioListener _Listener = null;

    public HRIRAsset _HRIR = null;
    private float[] _LeftHRIR = null;
    private float[] _RightHRIR = null;

    public SourceType SampleSource = SourceType.OneChannel;
    public RolloffType Rolloff = RolloffType.Square;

    public float Amplification = 20;
    public float MaxVolume = 3;

    public bool DisableConvolution = false;

    private float _Azimuth = 0;
    private float _Elevation = 0;
    private float _Radial = 0;
    private int _IAzimuth = 0;
    private int _IElevation = 0;

    private PCMQueue _PCMBuffer = null;
    private bool _FilterEnabled = false;
    private float _FilterGain = 1.0f;

    void Start () {
        _Listener = FindObjectOfType(typeof(AudioListener)) as AudioListener;
        _PCMBuffer = new PCMQueue(1);

        if (_HRIR == null)
            Debug.LogWarning("No HRIR specified. Binaural audio processing disabled.", this);

        if (GetComponent<AudioSource>() == null)
            Debug.LogWarning(string.Format("There is no AudioSource attached to {0}. Binaural audio processing disabled.", this.name), this);

        if (GetComponent<AudioSource>() && GetComponent<AudioSource>().clip != null && GetComponent<AudioSource>().clip.channels != 2)
            Debug.LogWarning("This script can only run on 2 channel audio clips. Binaural audio processing disabled.", this);

        if (_Listener == null)
            Debug.LogWarning("No Listener in scene. Binaural audio processing disabled.", this);

        Update();
	}

    void Update()
    {
        UpdateSphericalCoordinates();
        UpdateFilterGain();
        UpdateHRIR();
    }

    void UpdateSphericalCoordinates()
    {
        if (_Listener == null)
            return;

        Vector3 forward = _Listener.transform.forward;
        Vector3 up = _Listener.transform.up;
        Vector3 right = _Listener.transform.right;
        Vector3 displacement = transform.position - _Listener.transform.position;
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

    void UpdateFilterGain()
    {
        _FilterGain = 1;
        _FilterGain /= (Rolloff == RolloffType.Linear ? _Radial : 1.0f);
        _FilterGain /= (Rolloff == RolloffType.Square ? _Radial * _Radial : 1.0f);
        _FilterGain *= Amplification;
        _FilterGain = Mathf.Min(_FilterGain, MaxVolume);
    }

    void UpdateHRIR()
    {
        if (_HRIR == null)
        {
            _FilterEnabled = false;
            return;
        }

        if (_PCMBuffer.Length != _HRIR.Samples)
        {
            _FilterEnabled = false;
            _PCMBuffer = new PCMQueue(_HRIR.Samples);
        }

        int newIAzimuth = _HRIR.GetNearestAzimuthIndex(_Azimuth);
        int newIElevation = _HRIR.GetNearestElevationIndex(_Elevation);
        if (newIAzimuth != _IAzimuth || newIElevation != _IElevation)
        {
            _IAzimuth = newIAzimuth;
            _IElevation = newIElevation;
            _LeftHRIR = _HRIR.GetLeftHrir(_IAzimuth, _IElevation);
            _RightHRIR = _HRIR.GetRightHrir(_IAzimuth, _IElevation);
        }

        _FilterEnabled = true;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_FilterEnabled || channels != 2)
            return;

        for (int i = 0; i < data.Length / 2; i++)
        {
            float inSample = data[i * 2];
            if (SampleSource == SourceType.TwoChannels)
                inSample = (inSample + data[i * 2 + 1]) / 2;

            float lSample = inSample * _LeftHRIR[0];
            float rSample = inSample * _RightHRIR[0];

            _PCMBuffer.Enqueue(inSample);

            if (!DisableConvolution)
            {
                for (int m = 1; m < _HRIR.Samples; m++)
                {
                    inSample = _PCMBuffer[m];
                    lSample += inSample * _LeftHRIR[m];
                    rSample += inSample * _RightHRIR[m];
                }

                data[i * 2 + 0] = Mathf.Min(lSample * _FilterGain, 1);
                data[i * 2 + 1] = Mathf.Min(rSample * _FilterGain, 1);
            }
        }
    }

    #region enums

    public enum SourceType
    {
        OneChannel,
        TwoChannels
    }

    public enum RolloffType
    {
        None,
        Linear,
        Square
    }

    #endregion
}
