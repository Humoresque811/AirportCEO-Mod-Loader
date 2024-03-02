using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.WatermarkUtils
{
    public readonly struct WatermarkInfo
    {
        private readonly string _modText;
        private readonly string _version;
        public readonly bool _isAbbreviated;

        public WatermarkInfo(string modName, string version, bool isAbbreviated)
        {
            _modText = modName;
            _version = version;
            _isAbbreviated = isAbbreviated;
        }

        public string GetWatermark()
        {
            if (_isAbbreviated)
            {
                return string.Concat(_modText, _version);
            }

            return string.Concat(_modText, " v", _version, "\n");
        }
    }
}
