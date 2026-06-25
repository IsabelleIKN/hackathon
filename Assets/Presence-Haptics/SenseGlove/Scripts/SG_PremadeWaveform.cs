using UnityEngine;

namespace Presence
{
    /// <summary> All available glove vibration locations. </summary>
    public enum VibrationLocation
    {
        Unknown,
        Thumb_Tip,
        Index_Tip,
        Middle_Tip,
        Ring_Tip,
        Pinky_Tip,
        Palm_IndexSide,
        Palm_PinkySide,
        /// <summary> Play this Haptic Effect on the whole hand. For General Hand Feedback. </summary>
        Handpalm
    }


    /// <summary> SenseGlove version of a Premade Waveform. Created either directly, or from a baseEffect -autmatically- </summary>
    [CreateAssetMenu(fileName = "SenseGloveVibration", menuName = "SenseGlove/Vibration", order = 1)]
    public class SG_PremadeWaveform : ScriptableObject
    {
        ///// <summary> If this is an original file, then it's ourses. </summary>
        //private bool isOriginalFile = true;

        /// <summary> The actuator for which this waveform is meant. Useful for conversions, backups and testing. </summary>
        public VibrationLocation intendedMotor = VibrationLocation.Handpalm;

        [Header("Waveform Parameters")]
        [Range(0.0f, 1.0f)] public float amplitude = 1.0f;
        public SGCore.WaveformType waveformType = SGCore.WaveformType.Sine;

        [Header("Timing Parameters")]
        [Range(SGCore.CustomWaveform.minAttackTime, SGCore.CustomWaveform.maxAttackTime)] public float attackTime = 0.0f;
        [Range(SGCore.CustomWaveform.minSustainTime, SGCore.CustomWaveform.maxSustainTime)] public float sustainTime = 1.0f;
        [Range(SGCore.CustomWaveform.minDecayTime, SGCore.CustomWaveform.maxDecayTime)] public float decayTime = 0.0f;
        [Range(SGCore.CustomWaveform.minPauseTime, SGCore.CustomWaveform.maxPauseTime)] public float pauseTime = 0.0f;

        [Range(1, SGCore.CustomWaveform.maxRepeatAmount)] public int RepeatAmount = 1;
        //public bool RepeatInfinite = false;

        [Header("Frequency Parameters")]
        [Range(SGCore.CustomWaveform.freqRangeMin, SGCore.CustomWaveform.freqRangeMax)] public int startFrequency = 180;
        [Range(SGCore.CustomWaveform.freqRangeMin, SGCore.CustomWaveform.freqRangeMax)] public int endFrequency = 180;

        //[Range(0.0f, 1.0f)] public float frequencySwitchTime = 0.0f;
        //[Range(SGCore.CustomWaveform.minFreqFactor, SGCore.CustomWaveform.maxFreqFactor)] public float frequencySwitchMultiplier = 1.0f;


        public SGCore.CustomWaveform InternalWaveform
        {
            get
            {
                SGCore.CustomWaveform wf = new SGCore.CustomWaveform(this.amplitude, this.sustainTime, this.startFrequency);
                wf.AttackTime = this.attackTime;
                wf.DecayTime = this.decayTime;
                wf.FrequencyEnd = this.endFrequency;
                wf.Infinite = false;
                wf.PauseTime = this.pauseTime;
                wf.RepeatAmount = this.RepeatAmount;
                wf.WaveType = this.waveformType;
                return wf;
            }
        }


        public SGCore.HapticLocation InternalLocation
        {
            get
            {
                switch (intendedMotor)
                {
                    case VibrationLocation.Handpalm:
                        return SGCore.HapticLocation.WholeHand;

                    case VibrationLocation.Thumb_Tip:
                        return SGCore.HapticLocation.Thumb_Tip;
                    case VibrationLocation.Index_Tip:
                        return SGCore.HapticLocation.Index_Tip;
                    case VibrationLocation.Middle_Tip:
                        return SGCore.HapticLocation.Middle_Tip;
                    case VibrationLocation.Ring_Tip:
                        return SGCore.HapticLocation.Ring_Tip;
                    case VibrationLocation.Pinky_Tip:
                        return SGCore.HapticLocation.Pinky_Tip;

                    case VibrationLocation.Palm_IndexSide:
                        return SGCore.HapticLocation.Palm_IndexSide;
                    case VibrationLocation.Palm_PinkySide:
                        return SGCore.HapticLocation.Palm_PinkySide;

                    default:
                        return SGCore.HapticLocation.Unknown;
                }
            }
        }


        public void TestBaseEffect()
        {
            SGCore.HapticLocation loc = InternalLocation;
            if (loc != SGCore.HapticLocation.Unknown)
            {
                SGCore.CustomWaveform wf = InternalWaveform;
                SGCore.HandLayer.SendCustomWaveform(true, wf, loc);
                SGCore.HandLayer.SendCustomWaveform(false, wf, loc);
            }
        }



    }


}