using System;
using System.IO;
using System.Xml;

namespace OpenMetaverse.Assets
{
    public class RegionSettings
    {
        public bool AllowDamage;
        public bool AllowLandResell;
        public bool AllowLandJoinDivide;
        public bool BlockFly;
        public bool BlockLandShowInSearch;
        public bool BlockTerraform;
        public bool DisableCollisions;
        public bool DisablePhysics;
        public bool DisableScripts;
        public int MaturityRating;
        public bool RestrictPushing;
        public int AgentLimit;
        public float ObjectBonus;

        public UUID TerrainDetail0;
        public UUID TerrainDetail1;
        public UUID TerrainDetail2;
        public UUID TerrainDetail3;
        public float TerrainHeightRange00;
        public float TerrainHeightRange01;
        public float TerrainHeightRange10;
        public float TerrainHeightRange11;
        public float TerrainStartHeight00;
        public float TerrainStartHeight01;
        public float TerrainStartHeight10;
        public float TerrainStartHeight11;

        public float WaterHeight;
        public float TerrainRaiseLimit;
        public float TerrainLowerLimit;
        public bool UseEstateSun;
        public bool FixedSun;

        public static RegionSettings FromStream(Stream stream)
        {
            RegionSettings settings = new RegionSettings();
            System.Globalization.NumberFormatInfo nfi = Utils.EnUsCulture.NumberFormat;

            using (XmlTextReader xtr = new XmlTextReader(stream))
            {
                xtr.ReadStartElement("RegionSettings");
                xtr.ReadStartElement("General");

                while (xtr.Read() && xtr.NodeType != XmlNodeType.EndElement)
                {
                    switch (xtr.Name)
                    {
                        case "AllowDamage":
                            settings.AllowDamage = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "AllowLandResell":
                            settings.AllowLandResell = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "AllowLandJoinDivide":
                            settings.AllowLandJoinDivide = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "BlockFly":
                            settings.BlockFly = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "BlockLandShowInSearch":
                            settings.BlockLandShowInSearch = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "BlockTerraform":
                            settings.BlockTerraform = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "DisableCollisions":
                            settings.DisableCollisions = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "DisablePhysics":
                            settings.DisablePhysics = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "DisableScripts":
                            settings.DisableScripts = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "MaturityRating":
                            settings.MaturityRating = int.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "RestrictPushing":
                            settings.RestrictPushing = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "AgentLimit":
                            settings.AgentLimit = int.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "ObjectBonus":
                            settings.ObjectBonus = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                    }
                }

                xtr.ReadEndElement();
                xtr.ReadStartElement("GroundTextures");

                while (xtr.Read() && xtr.NodeType != XmlNodeType.EndElement)
                {
                    switch (xtr.Name)
                    {
                        case "Texture1":
                            settings.TerrainDetail0 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "Texture2":
                            settings.TerrainDetail1 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "Texture3":
                            settings.TerrainDetail2 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "Texture4":
                            settings.TerrainDetail3 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "ElevationLowSW":
                            settings.TerrainStartHeight00 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationLowNW":
                            settings.TerrainStartHeight01 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationLowSE":
                            settings.TerrainStartHeight10 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationLowNE":
                            settings.TerrainStartHeight11 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighSW":
                            settings.TerrainHeightRange00 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighNW":
                            settings.TerrainHeightRange01 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighSE":
                            settings.TerrainHeightRange10 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighNE":
                            settings.TerrainHeightRange11 = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                    }
                }

                xtr.ReadEndElement();
                xtr.ReadStartElement("Terrain");

                while (xtr.Read() && xtr.NodeType != XmlNodeType.EndElement)
                {
                    switch (xtr.Name)
                    {
                        case "WaterHeight":
                            settings.WaterHeight = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "TerrainRaiseLimit":
                            settings.TerrainRaiseLimit = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "TerrainLowerLimit":
                            settings.TerrainLowerLimit = float.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "UseEstateSun":
                            settings.UseEstateSun = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "FixedSun":
                            settings.FixedSun = bool.Parse(xtr.ReadElementContentAsString());
                            break;
                    }
                }
            }

            return settings;
        }

        public void ToXML(string filename)
        {
            var sw = new StringWriter();
            var writer = new XmlTextWriter(sw) { Formatting = Formatting.Indented };
            writer.WriteStartDocument();

            writer.WriteStartElement(string.Empty, "RegionSettings", string.Empty);
            writer.WriteStartElement(string.Empty, "General", string.Empty);

            WriteBoolean(writer, "AllowDamage", AllowDamage);
            WriteBoolean(writer, "AllowLandResell", AllowLandResell);
            WriteBoolean(writer, "AllowLandJoinDivide", AllowLandJoinDivide);
            WriteBoolean(writer, "BlockFly", BlockFly);
            WriteBoolean(writer, "BlockLandShowInSearch", BlockLandShowInSearch);
            WriteBoolean(writer, "BlockTerraform", BlockTerraform);
            WriteBoolean(writer, "DisableCollisions", DisableCollisions);
            WriteBoolean(writer, "DisablePhysics", DisablePhysics);
            WriteBoolean(writer, "DisableScripts", DisableScripts);
            writer.WriteElementString("MaturityRating", MaturityRating.ToString());
            WriteBoolean(writer, "RestrictPushing", RestrictPushing);
            writer.WriteElementString("AgentLimit", AgentLimit.ToString());
            writer.WriteElementString("ObjectBonus", ObjectBonus.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement(string.Empty, "GroundTextures", string.Empty);
                
            writer.WriteElementString("Texture1", TerrainDetail0.ToString());
            writer.WriteElementString("Texture2", TerrainDetail1.ToString());
            writer.WriteElementString("Texture3", TerrainDetail2.ToString());
            writer.WriteElementString("Texture4", TerrainDetail3.ToString());
            writer.WriteElementString("ElevationLowSW", TerrainStartHeight00.ToString());
            writer.WriteElementString("ElevationLowNW", TerrainStartHeight01.ToString());
            writer.WriteElementString("ElevationLowSE", TerrainStartHeight10.ToString());
            writer.WriteElementString("ElevationLowNE", TerrainStartHeight11.ToString());
            writer.WriteElementString("ElevationHighSW", TerrainHeightRange00.ToString());
            writer.WriteElementString("ElevationHighNW", TerrainHeightRange01.ToString());
            writer.WriteElementString("ElevationHighSE", TerrainHeightRange10.ToString());
            writer.WriteElementString("ElevationHighNE", TerrainHeightRange11.ToString());
            writer.WriteEndElement();
                
            writer.WriteStartElement(string.Empty, "Terrain", string.Empty);

            writer.WriteElementString("WaterHeight", WaterHeight.ToString());
            writer.WriteElementString("TerrainRaiseLimit", TerrainRaiseLimit.ToString());
            writer.WriteElementString("TerrainLowerLimit", TerrainLowerLimit.ToString());
            WriteBoolean(writer, "UseEstateSun", UseEstateSun);
            WriteBoolean(writer, "FixedSun", FixedSun);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
            sw.Close();
            File.WriteAllText(filename, sw.ToString());
        }

        void WriteBoolean(XmlTextWriter writer, string name, bool value)
        {
            writer.WriteElementString(name, value ? "True" : "False");
        }
    }
}
