using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GTASaveData;

namespace SaveConverter
{
    public static class GxtLoader
    {
        public static Dictionary<string, string> LoadGTA3(string path)
        {
            return LoadGTA3(File.ReadAllBytes(path));
        }

        public static Dictionary<string, string> LoadGTA3(byte[] data)
        {
            const int SectionHeaderSize = 8;
            const int SectionIdLength = 4;
            const int KeyRecordSize = 12;
            const int KeyLength = 8;

            using DataBuffer buf = new DataBuffer(data);
            Dictionary<string, string> gxtTable = new Dictionary<string, string>();

            string tkey = buf.ReadString(SectionIdLength);
            int tkeySize = buf.ReadInt32();
            int tkeyStart = buf.Mark();
            if (tkey != "TKEY") throw InvalidGTA3GxtFile();

            Debug.Assert(tkeyStart < data.Length);
            Debug.Assert(tkeySize < data.Length);

            buf.Skip(tkeySize);

            string tdat = buf.ReadString(SectionIdLength);
            int tdatSize = buf.ReadInt32();
            int tdatStart = buf.Mark();
            if (tdat != "TDAT") throw InvalidGTA3GxtFile();

            Debug.Assert(tdatStart < data.Length);
            Debug.Assert(tdatSize < data.Length);

            buf.Seek(tkeyStart);

            int tkeyPos = 0;
            while (tkeyPos < tkeySize)
            {
                buf.Seek(SectionHeaderSize + tkeyPos);
                int valueOffset = buf.ReadInt32();
                string key = buf.ReadString(KeyLength);
                tkeyPos += KeyRecordSize;

                Debug.Assert(valueOffset < tdatSize);

                buf.Seek(tdatStart + valueOffset);
                string value = buf.ReadString(unicode: true);

                gxtTable.Add(key, value);
            }

            return gxtTable;
        }

        public static Dictionary<string, Dictionary<string, string>> LoadVC(string path)
        {
            return LoadVC(File.ReadAllBytes(path));
        }

        public static Dictionary<string, Dictionary<string, string>> LoadVC(byte[] data)
        {
            int gxtSize = data.Length;
            int numEntries = 0;

            using DataBuffer buf = new DataBuffer(data);
            var gxtTable = new Dictionary<string, Dictionary<string, string>>();

            // Read TABL header
            string tabl = buf.ReadString(4);
            int tablSectionSize = buf.ReadInt32();
            int tablSectionPos = buf.Position;
            if (tabl != "TABL") throw InvalidGTAVCGxtFile();
            Debug.Assert(buf.Position + tablSectionSize <= gxtSize);

            // Read TABL
            while (tablSectionPos < tablSectionSize)
            {
                var table = new Dictionary<string, string>();

                // Read TABL entry
                string tableName = buf.ReadString(8);
                int tablePos = buf.ReadInt32();
                tablSectionPos += 12;
                Debug.Assert(tablePos < gxtSize);

                // Jump to table and read table name
                buf.Seek(tablePos);
                string tableName2 = tableName;
                if (tableName != "MAIN") tableName2 = buf.ReadString(8);
                Debug.Assert(tableName == tableName2);

                // Read TKEY header
                string tkey = buf.ReadString(4);
                int tkeySectionSize = buf.ReadInt32();
                int tkeySectionStart = buf.Position;
                int tkeySectionOffset = 0;
                if (tkey != "TKEY") throw InvalidGTAVCGxtFile();
                Debug.Assert(buf.Position + tkeySectionSize <= gxtSize);

                // Verify TDAT presence
                buf.Skip(tkeySectionSize);
                string tdat = buf.ReadString(4);
                int tdatSectionSize = buf.ReadInt32();
                int tdatSectionStart = buf.Position;
                if (tdat != "TDAT") throw InvalidGTAVCGxtFile();
                Debug.Assert(buf.Position + tdatSectionSize <= gxtSize);

                // Read TKEY
                buf.Seek(tkeySectionStart);
                while (tkeySectionOffset < tkeySectionSize)
                {
                    // Read TKEY entry
                    int valueOffset = buf.ReadInt32();
                    string key = buf.ReadString(8);
                    tkeySectionOffset += 12;
                    Debug.Assert(valueOffset < tdatSectionSize);
                    Debug.Assert(!table.ContainsKey(key));

                    // Read value
                    buf.Seek(tdatSectionStart + valueOffset);
                    table[key] = buf.ReadString(unicode: true);
                    numEntries++;

                    buf.Seek(tkeySectionStart + tkeySectionOffset);
                }

                gxtTable.Add(tableName, table);
                buf.Seek(tablSectionPos);
            }

            return gxtTable;
        }

        private static InvalidDataException InvalidGTA3GxtFile() => new InvalidDataException("Not a valid GTA3 GXT file!");
        private static InvalidDataException InvalidGTAVCGxtFile() => new InvalidDataException("Not a valid GTA:VC GXT file!");
    }
}
