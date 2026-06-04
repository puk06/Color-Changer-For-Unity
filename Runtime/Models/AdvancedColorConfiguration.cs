using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace net.puk06.ColorChanger.Models
{
    [Serializable]
    public class AdvancedColorConfiguration : ISerializationCallbackReceiver
    {
        private const int CurrentSerializationVersion = 1;
        [SerializeField] private int SerializationVersion = 0;

        [FormerlySerializedAs("Enabled")]
        public bool IsEnabled = false;
        
        [Range(-180f, 180f)] public float Hue;
        [Range(0f, 2f)] public float Saturation = 1.0f;
        [Range(0f, 2f)] public float Value = 1.0f;
        public float Brightness = 1.0f;
        public float Contrast = 1.0f;
        public float Gamma = 1.0f;
        public float Exposure = 0.0f;
        [Range(0f, 1f)] public float Transparency = 0.0f;

        public void OnBeforeSerialize()
        {
            SerializationVersion = CurrentSerializationVersion;
        }

        public void OnAfterDeserialize()
        {
            if (SerializationVersion >= CurrentSerializationVersion) return;

            if (SerializationVersion == 0)
            {
                Hue = ConvertLegacyDegreesHueToCurrent(Hue);
                Saturation = ConvertLegacyStrengthToCurrent(Saturation);
                Value = ConvertLegacyStrengthToCurrent(Value);
            }

            SerializationVersion = CurrentSerializationVersion;
        }

        public static float ConvertLegacyDegreesHueToCurrent(float legacyHue)
        {
            float wrappedHue = Mathf.Repeat(legacyHue, 360f);
            if (wrappedHue > 180f) wrappedHue -= 360f;

            return wrappedHue;
        }

        public static float ConvertLegacyStrengthToCurrent(float legacyValue)
        {
            return 1f + legacyValue / 100f;
        }
    }
}
