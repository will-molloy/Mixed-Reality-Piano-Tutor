using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

[DataContract]
public class PianoCalibrationDTO
{

    public PianoCalibrationDTO() { }

    public PianoCalibrationDTO(Vector3 position, Vector3 scale, Vector3 eulerAngle)
    {
        this.markerPos = position;
        this.markerScale = scale;
        this.markerEulerAngle = eulerAngle;
    }

    [DataMember]
    public Vector3 markerPos { get; set; }

    [DataMember]
    public Vector3 markerScale { get; set; }

    [DataMember]
    public Vector3 markerEulerAngle { get; set; }

}
