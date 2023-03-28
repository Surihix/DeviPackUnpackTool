using System.IO.Compression;
using System.Text;

namespace DeviPackUnpackTool
{
    internal class DEunpack
    {
        public static void UnpackFile(string InFile)
        {
            var InFilePath = Path.GetFullPath(InFile);
            var InFileDir = Path.GetDirectoryName(InFilePath);
            var ExtractDir = InFileDir + "\\" + Path.GetFileNameWithoutExtension(InFile) + "\\";

            bool CheckAndDelOldExtractDir = Directory.Exists(ExtractDir);
            switch (CheckAndDelOldExtractDir)
            {
                case true:
                    Directory.Delete(ExtractDir, true);
                    break;

                case false:
                    break;
            }
            Directory.CreateDirectory(ExtractDir);


            Console.WriteLine("Unpacking files....");
            Console.WriteLine("");

            using (FileStream DeviFile = new(InFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader DeviFileReader = new(DeviFile))
                {
                    CheckArchiveHeader(DeviFileReader);

                    ReadByteValue(DeviFileReader, 16, out uint FileCount);
                    ReadByteValue(DeviFileReader, 20, out uint OffsetTablePos);
                    ReadByteValue(DeviFileReader, 24, out uint DataStartPos);
                    ReadByteValue(DeviFileReader, 32, out uint PathsCmpSize);

                    using (MemoryStream PathStream = new())
                    {
                        DeviFile.CopyTo(PathStream, 36, PathsCmpSize);

                        using (MemoryStream DcmpPathStream = new())
                        {
                            PathStream.Seek(0, SeekOrigin.Begin);
                            ZlibDecompress(PathStream, DcmpPathStream);

                            using (BinaryReader DcmpPathReader = new(DcmpPathStream))
                            {

                                uint PathReaderPos = 0;
                                uint OffsetTblReaderPos = 0;
                                for (int f = 0; f < FileCount; f++)
                                {
                                    var MainFilePath = "";

                                    FileNamesBuilder(DcmpPathReader, PathReaderPos, ref MainFilePath);

                                    ReadByteValue(DeviFileReader, OffsetTablePos + OffsetTblReaderPos, out var FileStartPos);
                                    ReadByteValue(DeviFileReader, OffsetTablePos + OffsetTblReaderPos + 8, out var FileCmpSize);

                                    var DirectoryOfFile = Path.GetDirectoryName(MainFilePath);
                                    var FileName = Path.GetFileName(MainFilePath);
                                    var FinalOutFilePath = ExtractDir + DirectoryOfFile + "\\" + FileName;

                                    bool CheckAndCreateDirectory = Directory.Exists(DirectoryOfFile);
                                    switch (CheckAndCreateDirectory)
                                    {
                                        case true:
                                            break;

                                        case false:
                                            bool CheckIfNeedDir = DirectoryOfFile.Equals("");
                                            switch (CheckIfNeedDir)
                                            {
                                                case true:
                                                    break;

                                                case false:
                                                    Directory.CreateDirectory(ExtractDir + DirectoryOfFile);
                                                    break;
                                            }
                                            break;
                                    }

                                    bool CheckAndDelFile = File.Exists(FinalOutFilePath);
                                    switch (CheckAndDelFile)
                                    {
                                        case true:
                                            File.Delete(FinalOutFilePath);
                                            break;

                                        case false:
                                            break;
                                    }

                                    using (FileStream OutFile = new(FinalOutFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        using (MemoryStream CmpFileData = new())
                                        {
                                            DeviFile.CopyTo(CmpFileData, DataStartPos + FileStartPos, FileCmpSize);

                                            CmpFileData.Seek(0, SeekOrigin.Begin);
                                            ZlibDecompress(CmpFileData, OutFile);
                                        }
                                    }

                                    Console.WriteLine("Unpacked " + MainFilePath);

                                    PathReaderPos = (uint)DcmpPathReader.BaseStream.Position;
                                    OffsetTblReaderPos += 12;
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void UnpackFilePaths(string InFile)
        {
            var InFilePath = Path.GetFullPath(InFile);
            var ExtractDir = Path.GetDirectoryName(InFilePath) + "\\";
            var TxtFileName = Path.GetFileNameWithoutExtension(InFile) + ".txt";
            var OutTxtFile = ExtractDir + TxtFileName;

            DEcmn.CheckAndDelFile(OutTxtFile);


            Console.WriteLine("Unpacking file paths....");
            Console.WriteLine("");

            using (FileStream DeviFile = new(InFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader DeviFileReader = new(DeviFile))
                {
                    CheckArchiveHeader(DeviFileReader);

                    ReadByteValue(DeviFileReader, 16, out uint FileCount);
                    ReadByteValue(DeviFileReader, 32, out uint PathsCmpSize);

                    using (MemoryStream PathStream = new())
                    {
                        DeviFile.CopyTo(PathStream, 36, PathsCmpSize);

                        using (MemoryStream DcmpPathStream = new())
                        {
                            PathStream.Seek(0, SeekOrigin.Begin);
                            ZlibDecompress(PathStream, DcmpPathStream);

                            using (BinaryReader DcmpPathReader = new(DcmpPathStream))
                            {
                                using (StreamWriter PathsWriter = new(OutTxtFile, append: true))
                                {

                                    uint PathReaderPos = 0;
                                    for (int f = 0; f < FileCount; f++)
                                    {
                                        var MainFilePath = "";

                                        FileNamesBuilder(DcmpPathReader, PathReaderPos, ref MainFilePath);

                                        PathsWriter.WriteLine(MainFilePath);

                                        PathReaderPos = (uint)DcmpPathReader.BaseStream.Position;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckArchiveHeader(BinaryReader ReaderName)
        {
            ReaderName.BaseStream.Position = 0;
            var GetArchiveHeader = ReaderName.ReadChars(16);
            var ArchiveHeader = string.Join("", GetArchiveHeader).Replace("\0", "");

            bool CheckArchiveHeader = ArchiveHeader.StartsWith("DeviPack.v1.5");
            switch (CheckArchiveHeader)
            {
                case true:
                    break;

                case false:
                    DEcmn.ErrorExit("Error: This is not a valid DeviPack archive file");
                    break;
            }
        }

        static void ReadByteValue(BinaryReader ReaderName, uint ReaderPos, out uint OutVariable)
        {
            ReaderName.BaseStream.Position = ReaderPos;
            OutVariable = ReaderName.ReadUInt32();
        }

        static void ZlibDecompress(Stream StreamToDecompress, Stream StreamToHoldDcmpData)
        {
            using (ZLibStream ZlibDataDcmp = new(StreamToDecompress, CompressionMode.Decompress))
            {
                ZlibDataDcmp.CopyTo(StreamToHoldDcmpData);
            }
        }

        static void FileNamesBuilder(BinaryReader ReaderName, uint ReaderPos, ref string VarName)
        {
            ReaderName.BaseStream.Position = ReaderPos;
            var FileNameBuilder = new StringBuilder();
            char StringChars;
            while ((StringChars = ReaderName.ReadChar()) != default)
            {
                FileNameBuilder.Append(StringChars);
            }
            VarName = FileNameBuilder.ToString();
        }
    }
}