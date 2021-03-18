using System;
using System.Globalization;

using IniParser;
using IniParser.Model;

namespace BombPeli
{
    public class Config
    {
        private KeyDataCollection data;

        public Config (string filename) {
            FileIniDataParser parser = new FileIniDataParser();
            data = parser.ReadFile (filename) ["client"];
        }

        public bool TryGetBool (string key, out bool val) {
            val = false;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return bool.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetByte (string key, out byte val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return byte.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetSbyte (string key, out sbyte val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return sbyte.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetShort (string key, out short val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return short.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetUshort (string key, out ushort val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return ushort.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetInt (string key, out int val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return int.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetUint (string key, out uint val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return uint.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetLong (string key, out long val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return long.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetUlong (string key, out ulong val) {
            val = 0;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return ulong.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetFloat (string key, out float val) {
            val = 0.0f;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return float.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetDouble (string key, out double val) {
            val = 0.0d;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return double.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetDecimal (string key, out decimal val) {
            val = 0.0m;
            if (data.ContainsKey (key)) {
                KeyData tmp = data.GetKeyData (key);
                return decimal.TryParse (tmp.Value, out val);
            }
            return false;
        }

        public bool TryGetString (string key, out string val) {
            val = "";
            if (data.ContainsKey (key)) {
                val = data.GetKeyData (key).Value;
                return true;
            }
            return false;
        }

        public bool GetBool (string key) {
            return bool.Parse (data[key]);
        }

        public byte GetByte (string key) {
            return byte.Parse (data[key]);
        }

        public sbyte GetSbyte (string key) {
            return sbyte.Parse (data[key]);
        }

        public short GetShort (string key) {
            return short.Parse (data[key]);
        }

        public ushort GetUshort (string key) {
            return ushort.Parse (data[key]);
        }

        public int GetInt (string key) {
            return int.Parse (data[key]);
        }

        public uint GetUint (string key) {
            return uint.Parse (data[key]);
        }

        public long GetLong (string key) {
            return long.Parse (data[key]);
        }

        public ulong GetUlong (string key) {
            return ulong.Parse (data[key], CultureInfo.InvariantCulture);
        }

        public float GetFloat (string key) {
            return float.Parse (data[key]);
        }

        public double GetDouble (string key) {
            return double.Parse (data[key]);
        }

        public decimal GetDecimal (string key) {
            return decimal.Parse (data[key]);
        }

        public string GetString (string key) {
            return data[key];
        }

    }
}