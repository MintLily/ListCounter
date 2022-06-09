﻿#nullable enable
using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace ListCounter;

[RegisterTypeInIl2Cpp]
public class EnableDisableListener : MonoBehaviour {
    [method:HideFromIl2Cpp]
    public event Action? OnEnabled;
        
    [method:HideFromIl2Cpp]
    public event Action? OnDisabled;

    public EnableDisableListener(IntPtr obj0) : base(obj0) { }

    private void OnEnable() => OnEnabled?.Invoke();

    private void OnDisable() => OnDisabled?.Invoke();
}