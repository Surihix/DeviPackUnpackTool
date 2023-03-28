using System.Buffers.Binary;
using System.IO.Compression;

namespace DeviPackUnpackTool
{
    internal class DEpack
    {
        public static void PackFolder(string InFolder, string CompLvlVar)
        {
            CompLvlVar = CompLvlVar.Replace("-", "");
            var DefinedCompLvl = CompressionLevel.NoCompression;

            bool ParsedCompLvl = Enum.TryParse(CompLvlVar, false, out CustomCompLvls SpecificLvl);
            switch (ParsedCompLvl)
            {
                case true:
                    DefinedCompLvl = (CompressionLevel)SpecificLvl;
                    break;

                case false:
                    break;
            }

            var DeviPackFile = InFolder + ".devi";
            var TmpPathsFile = InFolder + "\\_paths";
            var TmpOffsetTableFile = InFolder + "\\_offsets";
            var TmpDataFile = InFolder + "\\_datas";
            var TmpCmpDataFile = InFolder + "\\_CurrentCmpData";

            DEcmn.CheckAndDelFile(DeviPackFile);
            DEcmn.CheckAndDelFile(TmpPathsFile);
            DEcmn.CheckAndDelFile(TmpDataFile);
            DEcmn.CheckAndDelFile(TmpCmpDataFile);


            // Check if all the file sizes and the number of
            // files in the folder exceed a uint32 range 
            var InFolderNameLength = InFolder.Length;
            string[] DirectoryToPack = Directory.GetFiles(InFolder, "*", SearchOption.AllDirectories);
            long FileCount = DirectoryToPack.Length;
            long TotalSizeOfFiles = 0;

            DirectoryInfo dir = new(InFolder);
            foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                TotalSizeOfFiles += fi.Length;
            }

            bool CheckTotalSizeOfFiles = TotalSizeOfFiles > 4294967296;
            bool CheckTotalFileCount = FileCount > 4294967296;

            CheckUInt32Range(CheckTotalSizeOfFiles);
            CheckUInt32Range(CheckTotalFileCount);

            bool CheckThereIsNoFileInDir = FileCount.Equals(0);
            switch (CheckThereIsNoFileInDir)
            {
                case true:
                    DEcmn.ErrorExit("Error: There are no files in the specified folder to make a devi archive");
                    break;

                case false:
                    break;
            }


            Console.WriteLine("Packing files....");
            Console.WriteLine("");

            using (FileStream PathsFile = new(TmpPathsFile, FileMode.Append, FileAccess.Write))
            {
                using (FileStream DatasFile = new(TmpDataFile, FileMode.Append, FileAccess.Write))
                {
                    using (FileStream OffsetTableFile = new(TmpOffsetTableFile, FileMode.Append, FileAccess.Write))
                    {
                        OffsetTableFile.Seek(0, SeekOrigin.Begin);
                        AddNullBytes(OffsetTableFile, 0, (uint)FileCount * 12);

                        using (StreamWriter PathsWriter = new(PathsFile))
                        {
                            using (BinaryWriter OffsetsWriter = new(OffsetTableFile))
                            {

                                uint OffsetWritingPos = 0;
                                foreach (var file in DirectoryToPack)
                                {
                                    var FilePath = Path.GetDirectoryName(file);
                                    var FileName = Path.GetFileName(file);
                                    FilePath = FilePath?.Remove(0, InFolderNameLength);

                                    var VirtualPath = (FilePath + "\\" + FileName + "\0").TrimStart('\\');
                                    var DataStartPos = (uint)DatasFile.Length;
                                    var FileSize = (uint)new FileInfo(file).Length;

                                    using (FileStream SubFile = new(file, FileMode.Open, FileAccess.Read))
                                    {
                                        ZlibCompress(SubFile, TmpCmpDataFile, DefinedCompLvl);
                                        var CmpFileSize = (uint)new FileInfo(TmpCmpDataFile).Length;

                                        using (FileStream CmpStream = new(TmpCmpDataFile, FileMode.Open, FileAccess.Read))
                                        {
                                            CmpStream.Seek(0, SeekOrigin.Begin);
                                            CmpStream.CopyTo(DatasFile);

                                            PathsWriter.Write(VirtualPath);

                                            WriteByteValues(OffsetsWriter, OffsetWritingPos, DataStartPos);
                                            WriteByteValues(OffsetsWriter, OffsetWritingPos + 4, FileSize);
                                            WriteByteValues(OffsetsWriter, OffsetWritingPos + 8, CmpFileSize);

                                            OffsetWritingPos += 12;
                                            Console.WriteLine("Packed " + VirtualPath);
                                        }
                                    }

                                    DEcmn.CheckAndDelFile(TmpCmpDataFile);
                                }
                            }
                        }
                    }
                }
            }

            using (FileStream DeviPack = new(DeviPackFile, FileMode.Append, FileAccess.Write))
            {
                using (BinaryWriter DeviPackWriter = new(DeviPack))
                {
                    DeviPackWriter.BaseStream.Position = 0;
                    byte[] Header = new byte[] { 68, 101, 118, 105, 80, 97, 99, 107, 46, 118, 49, 46, 53, 00, 00, 00 };
                    DeviPackWriter.Write(Header);

                    AddNullBytes(DeviPack, 16, 20);

                    WriteByteValues(DeviPackWriter, 16, (uint)FileCount);
                    WriteByteValues(DeviPackWriter, 20, 40);

                    DeviPack.Seek(DeviPack.Length, SeekOrigin.Begin);

                    using (FileStream PackedPathsFile = new(TmpPathsFile, FileMode.Open, FileAccess.Read))
                    {
                        ZlibCompress(PackedPathsFile, TmpCmpDataFile, DefinedCompLvl);

                        using (FileStream CmpPathsData = new(TmpCmpDataFile, FileMode.Open, FileAccess.Read))
                        {
                            CmpPathsData.CopyTo(DeviPack);
                        }
                        using (FileStream PackedOffsets = new(TmpOffsetTableFile, FileMode.Open, FileAccess.Read))
                        {
                            PackedOffsets.CopyTo(DeviPack);
                        }
                        using (FileStream PackedDatasFile = new(TmpDataFile, FileMode.Open, FileAccess.Read))
                        {
                            PackedDatasFile.CopyTo(DeviPack);
                        }
                    }

                    var FilePathsSize = (uint)new FileInfo(TmpPathsFile).Length;
                    WriteByteValues(DeviPackWriter, 28, FilePathsSize);

                    var CmpFilePathsSize = (uint)new FileInfo(TmpCmpDataFile).Length;
                    WriteByteValues(DeviPackWriter, 32, CmpFilePathsSize);

                    var OffsetTableStartPos = CmpFilePathsSize + 36;
                    WriteByteValues(DeviPackWriter, 20, OffsetTableStartPos);

                    var OffsetTableFileSize = (uint)new FileInfo(TmpOffsetTableFile).Length;
                    var DataStartPos = OffsetTableStartPos + OffsetTableFileSize;
                    WriteByteValues(DeviPackWriter, 24, DataStartPos);

                    File.Delete(TmpPathsFile);
                    File.Delete(TmpOffsetTableFile);
                    File.Delete(TmpDataFile);
                    File.Delete(TmpCmpDataFile);
                }
            }
        }

        static void CheckUInt32Range(bool VarToCheck)
        {
            switch (VarToCheck)
            {
                case true:
                    Console.WriteLine("Error: Total file size or file count in the folder is greater than 4294967296");
                    Console.WriteLine("Check if there is a file in the folder that is more than 4gb or check if the");
                    Console.WriteLine("total number of files in the folder you are trying to pack is less than 4294967296");
                    DEcmn.ErrorExit("");
                    break;

                case false:
                    break;
            }
        }

        static void AddNullBytes(FileStream StreamName, uint StreamPos, uint ByteCount)
        {
            StreamName.Seek(StreamPos, SeekOrigin.Begin);
            for (int b = 0; b < ByteCount; b++)
            {
                StreamName.WriteByte(0);
            }
        }

        enum CustomCompLvls
        {
            c0 = CompressionLevel.NoCompression,
            c1 = CompressionLevel.Fastest,
            c2 = CompressionLevel.Optimal,
            c3 = CompressionLevel.SmallestSize
        }

        static void ZlibCompress(FileStream StreamToCompress, string FileNameForDataOutFile, CompressionLevel Clvel)
        {
            using FileStream ZlibDataOut = new(FileNameForDataOutFile, FileMode.OpenOrCreate, FileAccess.Write);
            using ZLibStream Compressor = new(ZlibDataOut, Clvel);
            StreamToCompress.CopyTo(Compressor);
        }

        static void WriteByteValues(BinaryWriter WriterName, uint WriterPos, uint VarToAdjustWith)
        {
            WriterName.BaseStream.Position = WriterPos;
            var AdjustValue = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(AdjustValue, VarToAdjustWith);
            WriterName.Write(AdjustValue);
        }
    }
}