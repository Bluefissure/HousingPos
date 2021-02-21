using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingPos.Objects
{
    public class CloudMap
    {
        public static CloudMap Empty => new CloudMap(0, "", "", "", "");

        public int LocationId;
        public string Name;
        public string Hash;
        public string ObjectId;
        public string Tags;
        public CloudMap(int locationId, string uploadName, string hash, string tags, string objectId)
        {
            LocationId = locationId;
            Name = uploadName;
            Hash = hash;
            Tags = tags;
            ObjectId = objectId;
        }
    }
}
