// Code from: https://stackoverflow.com/a/36892803/12770464
// Credit to Fattie on Stack Overflow https://stackoverflow.com/users/294884/fattie
//
// file Touchable.cs
// Correctly backfills the missing Touchable concept in Unity.UI's OO chain.

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Touchable))]
public class Touchable_Editor : Editor
{ public override void OnInspectorGUI(){} }
#endif
public class Touchable:Text
{ protected override void Awake() { base.Awake();} }