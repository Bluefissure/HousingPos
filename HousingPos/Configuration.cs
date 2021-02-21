using System;
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
        public List<int> HiddenScreenItemHistory = new List<int>();
        public List<int> GroupingList = new List<int>();
        public bool PlaceAnywhere = false;
        public bool Grouping = false;
        public bool UseFloatingWindow;
        public string UILanguage = "en";
        public List<HousingItem> HousingItemList = new List<HousingItem>();
        public string UploadName = "";
        public List<string> Tags = new List<string>();
        public List<bool> TagsSelectList = new List<bool>();
        public string Uploader = "";
        public string Location = "";
        public List<HousingItem> UploadItems = new List<HousingItem>();
        public bool Anonymous = true;
        public List<CloudMap> CloudMap = new List<CloudMap>();
        public string DefaultCloudUri = "https://api.4c43.work/ffxiv";
        // public string API_BASE_URL = "https://OHAlmaVE.api.lncldglobal.com/1.1";
        // public string CLASS_NAME = "/classes/housing";
        // public string SessionToken = "";

        public bool BDTH = false;
        public bool SingleExport = false;
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

        public void ResetRecord()
        {
            PlaceX = 0;
            PlaceY = 0;
            PlaceZ = 0;
            PlaceRotate = 0;
            SelectedItemIndex = -1;
            HiddenScreenItemHistory.Clear();
            GroupingList.Clear();
            Save();
        }

        #endregion
    }
}