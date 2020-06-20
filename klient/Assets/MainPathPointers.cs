using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPathPointers : MonoBehaviour
{

    public PathPointer[] commonPathPoints;
    public PathPointer[] redPoints;
    public PathPointer[] yellowPoints;
    public PathPointer[] bluePoints;
    public PathPointer[] greenPoints;

    [Header("Różnice Skali i pozycji zależnie od ilości pionków")]
    public float[] scalesDifference;
    public float[] positionsDifference;
}
