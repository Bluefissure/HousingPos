using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingPos.Objects
{
    [Serializable]
    public class OffsetDefinition
    {
        public string ExeVersion { get; set; }
        public string PlaceAnywhere { get; set; }
        public string WallPartition { get; set; }
        public string PrePositionBase { get; set; }
        public string PrePositionOffset { get; set; }
        public string MoveItem { get; set; }
        public string LoadHousing { get; set; }
    }
}
