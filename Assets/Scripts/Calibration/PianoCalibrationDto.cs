using System.Runtime.Serialization;
using UnityEngine;

namespace Calibration
{
    /// <summary>
    ///     - PianoBuilder calibration DTO
    /// </summary>
    [DataContract]
    public class PianoCalibrationDto
    {
        public PianoCalibrationDto() // For JSON library
        {
        }

        public PianoCalibrationDto(Vector3 position, Vector3 scale, Vector3 eulerAngle)
        {
            markerPos = position;
            markerScale = scale;
            markerEulerAngle = eulerAngle;
        }

        [DataMember] public Vector3 markerPos { get; set; }

        [DataMember] public Vector3 markerScale { get; set; }

        [DataMember] public Vector3 markerEulerAngle { get; set; }
    }
}