#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NeuroSdk.Actions
{
    public sealed class ActionJData
    {
        public JToken? Data { get; private set; }

        private ActionJData()
        {
        }

        private void DeserializeFromJson(string? stringifiedData)
        {
            if (string.IsNullOrEmpty(stringifiedData))
            {
                Data = null;
                return;
            }

            Data = JToken.Parse(stringifiedData);
        }

        public static bool TryParse(string? stringifiedData, [NotNullWhen(true)] out ActionJData? actionJData)
        {
            try
            {
                actionJData = new ActionJData();
                actionJData.DeserializeFromJson(stringifiedData);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to deserialize ActionJData from string.");
                Debug.LogError(e);
                actionJData = null;
                return false;
            }
        }
    }
}
