using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoArcSysModdingTool.Models;
using GHLib.Common.Enums;
using GHLib.Models;
using GHLib.Utils;

namespace GeoArcSysModdingTool.Utils
{
    public static class GHBinaryTools
    {
        private static readonly byte[] MagicBytes = {0x47, 0x48, 0x4D, 0x00};

        private static Hack[] HackDuplicates;

        private static Hack[][] ChildHackDuplicates;

        private static AoBScript[] AoBScriptDuplicates;

        private static AoBScript[][] AoBScriptsDuplicates;

        private static AoBReplacement[] AoBReplacementDuplicates;

        private static AoBReplacement[][] AoBReplacementsDuplicates;

        private static AoBPointer[] AoBPointerDuplicates;

        private static HackOffset[] HackOffsetDuplicates;

        private static HackOffset[][] HackOffsetsDuplicates;

        private static DropdownOptions[] DropDownDuplicates;

        #region Read Methods

        public static int VersionNumber = -1;

        public static HackCatagory[] ReadBinaryHackModule(string path)
        {
            return ReadBinaryHackModule(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        public static HackCatagory[] ReadBinaryHackModule(Stream stream)
        {
            using (var reader =
                new BinaryReader(stream))
            {
                if (!reader.ReadBytes(4).SequenceEqual(MagicBytes))
                {
                    reader.Close();
                    return null;
                }

                VersionNumber = reader.ReadInt32();
                ReadDuplicates(reader);
                var hackCatagoryList = new List<HackCatagory>();
                while (reader.ReadByte() == 0 && reader.BaseStream.Position < reader.BaseStream.Length)
                    hackCatagoryList.Add(ReadHackCatagoryBytes(reader));

                reader.Close();
                var hackCatagoryArray = hackCatagoryList.ToArray();
                foreach (var hc in hackCatagoryArray)
                foreach (var hg in hc.HackGroups)
                foreach (var h in hg.Hacks)
                    HackTools.ReadjustHackParents(h);

                HackDuplicates = null;
                ChildHackDuplicates = null;
                AoBScriptDuplicates = null;
                AoBScriptsDuplicates = null;
                AoBReplacementDuplicates = null;
                AoBReplacementsDuplicates = null;
                AoBPointerDuplicates = null;
                HackOffsetDuplicates = null;
                HackOffsetsDuplicates = null;
                DropDownDuplicates = null;

                return hackCatagoryArray;
            }
        }

        #region Read Duplicates

        private static void ReadDuplicates(BinaryReader reader)
        {
            HackDuplicates = null;
            ChildHackDuplicates = null;
            AoBScriptDuplicates = null;
            AoBScriptsDuplicates = null;
            AoBReplacementDuplicates = null;
            AoBReplacementsDuplicates = null;
            AoBPointerDuplicates = null;
            HackOffsetDuplicates = null;
            HackOffsetsDuplicates = null;
            DropDownDuplicates = null;

            DropDownDuplicates = ReadDropDownDuplicates(reader);

            HackOffsetDuplicates = ReadHackOffsetDuplicates(reader);

            var count = reader.ReadInt32();

            if (count != -1)
            {
                var hackOffsetsDuplicateArray = new HackOffset[count][];
                for (var i = 0; i < hackOffsetsDuplicateArray.Length; i++)
                    hackOffsetsDuplicateArray[i] = ReadHackOffsetDuplicates(reader);

                HackOffsetsDuplicates = hackOffsetsDuplicateArray;
            }

            AoBPointerDuplicates = ReadAoBPointerDuplicates(reader);

            AoBReplacementDuplicates = ReadAoBReplacementDuplicates(reader);

            count = reader.ReadInt32();

            if (count != -1)
            {
                var aobReplacementsDuplicateArray = new AoBReplacement[count][];
                for (var i = 0; i < aobReplacementsDuplicateArray.Length; i++)
                    aobReplacementsDuplicateArray[i] = ReadAoBReplacementDuplicates(reader);

                AoBReplacementsDuplicates = aobReplacementsDuplicateArray;
            }

            count = reader.ReadInt32();

            if (count != -1)
            {
                AoBScriptsDuplicates = new AoBScript[count][];
                for (var i = 0; i < AoBScriptsDuplicates.Length; i++)
                    AoBScriptsDuplicates[i] = ReadAoBScriptDuplicates(reader);
            }

            AoBScriptDuplicates = ReadAoBScriptDuplicates(reader);

            count = reader.ReadInt32();

            if (count != -1)
            {
                ChildHackDuplicates = new Hack[count][];
                for (var i = 0; i < ChildHackDuplicates.Length; i++)
                    ChildHackDuplicates[i] = ReadHackDuplicates(reader);
            }

            HackDuplicates = ReadHackDuplicates(reader);
        }

        private static Hack[] ReadHackDuplicates(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == -1)
                return null;
            var hackDuplicateArray = new Hack[count];
            for (var i = 0; i < hackDuplicateArray.Length; i++)
            {
                var val = reader.ReadByte();
                if (val == 0xFE)
                {
                    var hack = HackDuplicates[reader.ReadByte()];
                    var hackInput = hack as HackInput;
                    hackDuplicateArray[i] = hackInput != null ? hackInput.Clone() : hack.Clone();
                }
                else
                {
                    hackDuplicateArray[i] = ReadHackBytes(reader, val == 3);
                }
            }

            return hackDuplicateArray;
        }

        private static AoBScript[] ReadAoBScriptDuplicates(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == -1)
                return null;
            var aobScriptDuplicateArray = new AoBScript[count];
            for (var i = 0; i < aobScriptDuplicateArray.Length; i++)
            {
                var val = reader.ReadByte();
                aobScriptDuplicateArray[i] = val == 0xFC
                    ? AoBScriptDuplicates[reader.ReadByte()].Clone()
                    : ReadAoBScriptBytes(reader).Clone();
            }

            return aobScriptDuplicateArray;
        }

        private static AoBReplacement[] ReadAoBReplacementDuplicates(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == -1)
                return null;
            var aobReplacementDuplicateArray = new AoBReplacement[count];
            for (var i = 0; i < aobReplacementDuplicateArray.Length; i++)
            {
                var val = reader.ReadByte();
                if (val == 0xFA)
                    aobReplacementDuplicateArray[i] = AoBReplacementDuplicates[reader.ReadByte()];
                else
                    aobReplacementDuplicateArray[i] = ReadAoBReplacementBytes(reader);
            }

            return aobReplacementDuplicateArray;
        }

        private static AoBPointer[] ReadAoBPointerDuplicates(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == -1)
                return null;
            var aobPointerDuplicateArray = new AoBPointer[count];
            for (var i = 0; i < aobPointerDuplicateArray.Length; i++)
            {
                var val = reader.ReadByte();
                if (val == 0xF8)
                    aobPointerDuplicateArray[i] = AoBPointerDuplicates[reader.ReadByte()];
                else
                    aobPointerDuplicateArray[i] = ReadAoBPointerBytes(reader);
            }

            return aobPointerDuplicateArray;
        }

        private static HackOffset[] ReadHackOffsetDuplicates(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == -1)
                return null;
            var HackOffsetDuplicateArray = new HackOffset[count];
            for (var i = 0; i < HackOffsetDuplicateArray.Length; i++)
            {
                var val = reader.ReadByte();
                if (val == 0xF7)
                    HackOffsetDuplicateArray[i] = HackOffsetDuplicates[reader.ReadByte()];
                else
                    HackOffsetDuplicateArray[i] = ReadHackOffsetBytes(reader);
            }

            return HackOffsetDuplicateArray;
        }

        private static DropdownOptions[] ReadDropDownDuplicates(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == -1)
                return null;
            var dropDownDuplicateArray = new DropdownOptions[count];
            for (var i = 0; i < dropDownDuplicateArray.Length; i++)
            {
                var val = reader.ReadByte();
                if (val == 0xF5)
                    dropDownDuplicateArray[i] = DropDownDuplicates[reader.ReadByte()];
                else
                    dropDownDuplicateArray[i] = ReadDropDownBytes(reader);
            }

            return dropDownDuplicateArray;
        }

        #endregion

        private static HackCatagory ReadHackCatagoryBytes(BinaryReader reader)
        {
            var hackCatagory = new HackCatagory();
            hackCatagory.Name = reader.ReadString();
            var hackGroupList = new List<HackGroup>();
            var nothing = true;
            while (reader.ReadByte() == 1 && reader.BaseStream.Position < reader.BaseStream.Length)
            {
                nothing = false;
                hackGroupList.Add(ReadHackGroupBytes(reader));
            }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);

            hackCatagory.HackGroups = hackGroupList.ToArray();

            return hackCatagory;
        }

        private static HackGroup ReadHackGroupBytes(BinaryReader reader)
        {
            var hackGroup = new HackGroup();
            hackGroup.Name = reader.ReadString();
            var hackList = new List<Hack>();
            var nothing = true;
            var b = reader.ReadByte();
            while ((b == 2 || b == 3 || b == 0xFE) && reader.BaseStream.Position < reader.BaseStream.Length)
            {
                nothing = false;
                if (b == 0xFE)
                {
                    var hack = HackDuplicates[reader.ReadByte()];
                    var hackinput = hack as HackInput;
                    hackList.Add(hackinput != null ? hackinput.Clone() : hack);
                }
                else
                {
                    hackList.Add(ReadHackBytes(reader, b == 3));
                }

                b = reader.ReadByte();
            }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);

            hackGroup.Hacks = hackList.ToArray();

            return hackGroup;
        }

        private static Hack ReadHackBytes(BinaryReader reader, bool isInput = false)
        {
            var hack = isInput ? new HackInput() : new Hack();
            hack.Name = reader.ReadString();
            hack.Address = (IntPtr) reader.ReadInt64();
            hack.RelativeAddress = reader.ReadBoolean();
            if (isInput)
            {
                ((HackInput) hack).IsReadOnly = reader.ReadBoolean();
                var memType = reader.ReadByte();
                ((HackInput) hack).MemType = memType < 0xFF ? (MemValueType?) memType : null;
                var b = reader.ReadByte();
                if (b < 0xFF)
                {
                    var intArray = new int[b];
                    for (var i = 0; i < b; i++) intArray[i] = reader.ReadInt32();
                }

                var memValMod = reader.ReadByte();
                ((HackInput) hack).MemValMod = memValMod < 0xFF ? (MemValueModifier?) memValMod : null;

                var ddb = reader.ReadByte();
                if (ddb != 0xFF)
                {
                    if (ddb == 0xF5)
                        ((HackInput) hack).Dropdown = DropDownDuplicates[reader.ReadByte()];
                    else if (ddb == 8)
                        ((HackInput) hack).Dropdown = ReadDropDownBytes(reader);
                }
            }

            var nothing = true;

            var aobScriptList = new List<AoBScript>();
            var aobsb = reader.ReadByte();
            if (aobsb == 0xFB)
            {
                var aobScriptsDuplicate = AoBScriptsDuplicates[reader.ReadByte()];
                for (var i = 0; i < aobScriptsDuplicate.Length; i++)
                    aobScriptList.Add(aobScriptsDuplicate[i].Clone());
            }
            else if (aobsb != 0xFF)
            {
                while ((aobsb == 4 || aobsb == 0xFC) && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    nothing = false;
                    if (aobsb == 0xFC)
                        aobScriptList.Add(AoBScriptDuplicates[reader.ReadByte()].Clone());
                    else
                        aobScriptList.Add(ReadAoBScriptBytes(reader));
                    aobsb = reader.ReadByte();
                }
            }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);

            nothing = true;

            var hackOffsettList = new List<HackOffset>();
            var hob = reader.ReadByte();
            if (hob == 0xF6)
                hackOffsettList.AddRange(HackOffsetsDuplicates[reader.ReadByte()]);
            else if (hob != 0xFF)
                while ((hob == 6 || hob == 0xF7) && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    nothing = false;
                    if (hob == 0xF7)
                        hackOffsettList.Add(HackOffsetDuplicates[reader.ReadByte()]);
                    else
                        hackOffsettList.Add(ReadHackOffsetBytes(reader));
                    hob = reader.ReadByte();
                }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);

            nothing = true;

            var childHackList = new List<Hack>();
            var chcount = reader.ReadByte();
            if (chcount == 0xFD)
            {
                var childHacksDuplicate = ChildHackDuplicates[reader.ReadByte()];
                for (var i = 0; i < childHacksDuplicate.Length; i++)
                {
                    var chack = childHacksDuplicate[i];
                    var chackinput = chack as HackInput;
                    childHackList.Add(chackinput != null ? chackinput.Clone() : hack.Clone());
                }
            }
            else if (chcount != 0xFF)
            {
                var hb = reader.ReadByte();
                for (var i = 0;
                    i < chcount && (hb == 2 || hb == 3 || hb == 0xFE) &&
                    reader.BaseStream.Position < reader.BaseStream.Length;
                    i++)
                {
                    nothing = false;
                    if (hb == 0xFE)
                    {
                        var chack = HackDuplicates[reader.ReadByte()];
                        var chackinput = chack as HackInput;
                        childHackList.Add(chackinput != null ? chackinput.Clone() : chack.Clone());
                    }
                    else
                    {
                        childHackList.Add(ReadHackBytes(reader, hb == 3));
                    }

                    hb = reader.ReadByte();
                }
            }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);

            hack.AoBScripts = aobScriptList.ToArray();
            hack.Offsets = hackOffsettList.ToArray();
            hack.ChildHacks = childHackList.ToArray();

            return hack;
        }

        private static AoBScript ReadAoBScriptBytes(BinaryReader reader)
        {
            var aobScript = new AoBScript();
            aobScript.Address = (IntPtr) reader.ReadInt64();
            aobScript.AoBString = reader.ReadString();
            aobScript.IsRelative = reader.ReadBoolean();

            var nothing = true;

            var aobReplacementList = new List<AoBReplacement>();

            var aobrb = reader.ReadByte();
            if (aobrb == 0xF9)
                aobReplacementList.AddRange(AoBReplacementsDuplicates[reader.ReadByte()]);
            else if (aobrb != 0xFF)
                while ((aobrb == 6 || aobrb == 0xFA) && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    nothing = false;
                    if (aobrb == 0xFA)
                        aobReplacementList.Add(AoBReplacementDuplicates[reader.ReadByte()]);
                    else
                        aobReplacementList.Add(ReadAoBReplacementBytes(reader));
                    aobrb = reader.ReadByte();
                }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);

            nothing = true;

            var aobScriptList = new List<AoBScript>();
            var ascount = reader.ReadByte();
            if (ascount == 0xFB)
            {
                var aobScriptsDuplicate = AoBScriptsDuplicates[reader.ReadByte()];
                for (var i = 0; i < aobScriptsDuplicate.Length; i++)
                    aobScriptList.Add(aobScriptsDuplicate[i].Clone());
            }
            else if (ascount != 0xFF)
            {
                var aobsb = reader.ReadByte();
                for (var i = 0;
                    i < ascount && (aobsb == 4 || aobsb == 0xFC) &&
                    reader.BaseStream.Position < reader.BaseStream.Length;
                    i++)
                {
                    nothing = false;
                    if (aobsb == 0xFE)
                        aobScriptList.Add(AoBScriptDuplicates[reader.ReadByte()].Clone());
                    else
                        aobScriptList.Add(ReadAoBScriptBytes(reader));

                    aobsb = reader.ReadByte();
                }
            }

            if (!nothing) reader.BaseStream.Seek(-1, SeekOrigin.Current);


            var aobpb = reader.ReadByte();
            if (aobpb != 0xFF)
            {
                if (aobpb == 0xF8)
                    aobScript.AoBPointer = AoBPointerDuplicates[reader.ReadByte()];
                else if (aobpb == 7)
                    aobScript.AoBPointer = ReadAoBPointerBytes(reader);
            }

            aobScript.AoBReplacements = aobReplacementList.ToArray();
            aobScript.AoBScripts = aobScriptList.ToArray();

            return aobScript;
        }

        private static AoBReplacement ReadAoBReplacementBytes(BinaryReader reader)
        {
            var aobReplacement = new AoBReplacement();
            aobReplacement.ReplaceAoB = reader.ReadBytes(reader.ReadInt32());
            aobReplacement.Offset = reader.ReadInt32();

            return aobReplacement;
        }

        private static AoBPointer ReadAoBPointerBytes(BinaryReader reader)
        {
            var aobPointer = new AoBPointer();
            aobPointer.Offset = reader.ReadInt32();
            var b = reader.ReadByte();
            aobPointer.PointerType = b < 0xFF ? (AoBPointerType?) b : null;

            return aobPointer;
        }

        private static HackOffset ReadHackOffsetBytes(BinaryReader reader)
        {
            var hackOffset = new HackOffset();
            hackOffset.Offset = reader.ReadInt32();
            hackOffset.IsPointer = reader.ReadBoolean();

            return hackOffset;
        }

        private static DropdownOptions ReadDropDownBytes(BinaryReader reader)
        {
            var dropDown = new DropdownOptions();
            var optionCount = reader.ReadInt32();
            for (var i = 0; i < optionCount; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                dropDown.Options.Add(key, value);
            }

            dropDown.DisallowManualInput = reader.ReadBoolean();

            return dropDown;
        }

        #endregion
    }
}