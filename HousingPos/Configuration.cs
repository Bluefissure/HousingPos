﻿using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using HousingPos.Objects;

namespace HousingPos
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public HousingPosLanguage HousingPosLanguage = HousingPosLanguage.Client;
        public bool ShowTooltips = true;
        public bool DrawScreen = false;
        public float DrawDistance = 0;
        public List<int> HiddenScreenItemHistory;
        public bool PlaceAnywhere = false;
        public bool UseFloatingWindow;
        public string UILanguage = "en";
        public List<HousingItem> HousingItemList = new List<HousingItem>();
        public bool Recording = true;

        // public bool ForceMove = false;
        public bool BDTH = false;
        public bool SyncPos = false;
        public int SelectedItemIndex = -1;
        public float PlaceX = 0;
        public float PlaceY = 0;
        public float PlaceZ = 0;
        public float PlaceRotate = 0;
        public DateTime lastPosPackageTime = DateTime.Now;
        #region Init and Save

        [NonSerialized] private DalamudPluginInterface _pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
        }

        public void Save()
        {
            _pluginInterface.SavePluginConfig(this);
        }

        #endregion
    }
}