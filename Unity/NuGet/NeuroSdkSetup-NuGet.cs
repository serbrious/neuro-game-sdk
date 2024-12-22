﻿using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using NeuroSdk.Resources;
using UnityEngine.LowLevel;

namespace NeuroSdk
{
    partial class NeuroSdkSetup
    {
        static NeuroSdkSetup()
        {
            ResourceManager.InjectAssemblies();
            InitializeUniTask();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeUniTask()
        {
            PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref loop);
        }
    }
}