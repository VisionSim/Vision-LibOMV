using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using LitJson;

namespace OpenMetaverse.StructuredData
{
    public static partial class OSDParser
    {
        public static OSD DeserializeJson(Stream json)
        {
            using (StreamReader streamReader = new StreamReader(json))
            {
                var reader = new JsonReader(streamReader);
                return DeserializeJson(JsonMapper.ToObject(reader));
            }
        }

        public static OSD DeserializeJson(string json)
        {
            return DeserializeJson(JsonMapper.ToObject(json));
        }

        public static OSD DeserializeJson(JsonData json)
        {
            if (json == null) return new OSD();

            switch (json.GetJsonType ()) {
            case JsonType.Boolean:
                return OSD.FromBoolean ((bool)json);
            case JsonType.Int:
                return OSD.FromInteger ((int)json);
            case JsonType.Long:
                return OSD.FromLong ((long)json);
            case JsonType.Double:
                return OSD.FromReal ((double)json);
            case JsonType.String:
                var str = (string)json;
                if (string.IsNullOrEmpty (str))
                    return new OSD ();
                return OSD.FromString (str);
            case JsonType.Array:
                var array = new OSDArray (json.Count);
                for (int i = 0; i < json.Count; i++)
                    array.Add (DeserializeJson (json [i]));
                return array;
            case JsonType.Object:
                var map = new OSDMap (json.Count);
                IDictionaryEnumerator e = ((IOrderedDictionary)json).GetEnumerator ();
                while (e.MoveNext ())
                    map.Add ((string)e.Key, DeserializeJson ((JsonData)e.Value));
                return map;
            //case JsonType.None:
            default:
                return new OSD ();
            }
        }

        public static string SerializeJsonString(OSD osd)
        {
            return SerializeJson(osd, false).ToJson();
        }

        public static string SerializeJsonString(OSD osd, bool preserveDefaults)
        {
            return SerializeJson(osd, preserveDefaults).ToJson();
        }

        public static void SerializeJsonString(OSD osd, bool preserveDefaults, ref JsonWriter writer)
        {
            SerializeJson(osd, preserveDefaults).ToJson(writer);
        }

        public static JsonData SerializeJson(OSD osd, bool preserveDefaults)
        {
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    return new JsonData(osd.AsBoolean());
                case OSDType.Integer:
                    return new JsonData(osd.AsInteger());
                case OSDType.Real:
                    return new JsonData(osd.AsReal());
                case OSDType.String:
                case OSDType.Date:
                case OSDType.URI:
                case OSDType.UUID:
                    return new JsonData(osd.AsString());
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    var jsonbinarray = new JsonData();
                    jsonbinarray.SetJsonType(JsonType.Array);
                    for (int i = 0; i < binary.Length; i++)
                        jsonbinarray.Add(new JsonData(binary[i]));
                    return jsonbinarray;
                case OSDType.Array:
                    var jsonarray = new JsonData();
                    jsonarray.SetJsonType(JsonType.Array);
                    var array = (OSDArray)osd;
                    for (int i = 0; i < array.Count; i++)
                        jsonarray.Add(SerializeJson(array[i], preserveDefaults));
                    return jsonarray;
                case OSDType.Map:
                    var jsonmap = new JsonData();
                    jsonmap.SetJsonType(JsonType.Object);
                    var map = (OSDMap)osd;
                    foreach (KeyValuePair<string, OSD> kvp in map)
                    {
                        JsonData data;

                        if (preserveDefaults)
                            data = SerializeJson(kvp.Value, preserveDefaults);
                        else
                            data = SerializeJsonNoDefaults(kvp.Value);

                        if (data != null)
                            jsonmap[kvp.Key] = data;
                    }
                    return jsonmap;
                //case OSDType.Unknown:
                default:
                    return new JsonData(null);
            }
        }

        static JsonData SerializeJsonNoDefaults(OSD osd)
        {
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    var b = osd.AsBoolean();
                    if (!b)
                        return null;

                    return new JsonData(b);
                case OSDType.Integer:
                    var v = osd.AsInteger();
                    if (v == 0)
                        return null;

                    return new JsonData(v);
                case OSDType.Real:
                    var d = osd.AsReal();
                    if (Math.Abs (d) < 0.00000001f)     // floating point errors comparing to zero
                        return null;

                    return new JsonData(d);
                case OSDType.String:
                case OSDType.Date:
                case OSDType.URI:
                    var str = osd.AsString();
                    if (string.IsNullOrEmpty(str))
                        return null;

                    return new JsonData(str);
                case OSDType.UUID:
                    UUID uuid = osd.AsUUID();
                    if (uuid == UUID.Zero)return null;

                    return new JsonData(uuid.ToString());
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    if (binary == Utils.EmptyBytes)
                        return null;

                    var jsonbinarray = new JsonData();
                    jsonbinarray.SetJsonType(JsonType.Array);
                    for (int i = 0; i < binary.Length; i++)
                        jsonbinarray.Add(new JsonData(binary[i]));
                    return jsonbinarray;
                case OSDType.Array:
                    var jsonarray = new JsonData();
                    jsonarray.SetJsonType(JsonType.Array);
                    var array = (OSDArray)osd;
                    for (int i = 0; i < array.Count; i++)
                        jsonarray.Add(SerializeJson(array[i], false));
                    return jsonarray;
                case OSDType.Map:
                    var jsonmap = new JsonData();
                    jsonmap.SetJsonType(JsonType.Object);
                    var map = (OSDMap)osd;
                    foreach (KeyValuePair<string, OSD> kvp in map)
                    {
                        var data = SerializeJsonNoDefaults(kvp.Value);
                        if (data != null)
                            jsonmap[kvp.Key] = data;
                    }
                    return jsonmap;
                //case OSDType.Unknown:
                default:
                    return null;
            }
        }
    }
}
