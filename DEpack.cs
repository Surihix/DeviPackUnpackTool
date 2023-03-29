using System.Buffers.Binary;
using System.IO.Compression;

namespace DeviPackUnpackTool;

internal class DEpack
{
    public static void PackFolder(string inFolder, string cmpLvlVar)
    {
        cmpLvlVar = cmpLvlVar.Replace("-", "");
        var definedCmpLvl = CompressionLevel.NoCompression;

        bool parsedCmpLvl = Enum.TryParse(cmpLvlVar, false, out CustomCmpLvls specificLvl);
        switch (parsedCmpLvl)
        {
            case true:
                definedCmpLvl = (CompressionLevel)specificLvl;
                break;

            case false:
                break;
        }

        var deviPackFile = inFolder + ".devi";
        var tmpPathsFile = inFolder + "\\_paths";
        var tmpOffsetTableFile = inFolder + "\\_offsets";
        var tmpDataFile = inFolder + "\\_datas";
        var tmpCmpDataFile = inFolder + "\\_CurrentCmpData";

        DEcmn.CheckAndDelFile(deviPackFile);
        DEcmn.CheckAndDelFile(tmpPathsFile);
        DEcmn.CheckAndDelFile(tmpDataFile);
        DEcmn.CheckAndDelFile(tmpCmpDataFile);


        // Check if all the file sizes and the number of
        // files in the folder exceed a uint32 range 
        var inFolderNameLength = inFolder.Length;
        string[] directoryToPack = Directory.GetFiles(inFolder, "*", SearchOption.AllDirectories);
        long fileCount = directoryToPack.Length;
        long totalSizeOfFiles = 0;

        DirectoryInfo dir = new(inFolder);
        foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
        {
            totalSizeOfFiles += fi.Length;
        }

        bool checkTotalSizeOfFiles = totalSizeOfFiles > 4294967296;
        bool checkTotalFileCount = fileCount > 4294967296;

        CheckUInt32Range(checkTotalSizeOfFiles);
        CheckUInt32Range(checkTotalFileCount);

        if (fileCount.Equals(0))
        {
            DEcmn.ErrorExit("Error: There are no files in the specified folder to make a devi archive");
        }


        Console.WriteLine("Packing files....");
        Console.WriteLine("");

        using (FileStream pathsFile = new(tmpPathsFile, FileMode.Append, FileAccess.Write))
        {
            using (FileStream dataFile = new(tmpDataFile, FileMode.Append, FileAccess.Write))
            {
                using (FileStream offsetTableFile = new(tmpOffsetTableFile, FileMode.Append, FileAccess.Write))
                {
                    offsetTableFile.Seek(0, SeekOrigin.Begin);
                    AddNullBytes(offsetTableFile, 0, (uint)fileCount * 12);

                    using (StreamWriter pathsWriter = new(pathsFile))
                    {
                        using (BinaryWriter offsetsWriter = new(offsetTableFile))
                        {

                            uint offsetWritingPos = 0;
                            foreach (var file in directoryToPack)
                            {
                                var filePath = Path.GetDirectoryName(file);
                                var fileName = Path.GetFileName(file);
                                filePath = filePath?.Remove(0, inFolderNameLength);

                                var virtualPath = (filePath + "\\" + fileName + "\0").TrimStart('\\');
                                var dataStartPos = (uint)dataFile.Length;
                                var fileSize = (uint)new FileInfo(file).Length;

                                using (FileStream subFile = new(file, FileMode.Open, FileAccess.Read))
                                {
                                    ZlibCompress(subFile, tmpCmpDataFile, definedCmpLvl);
                                    var cmpFileSize = (uint)new FileInfo(tmpCmpDataFile).Length;

                                    using (FileStream cmpStream = new(tmpCmpDataFile, FileMode.Open, FileAccess.Read))
                                    {
                                        cmpStream.Seek(0, SeekOrigin.Begin);
                                        cmpStream.CopyTo(dataFile);

                                        pathsWriter.Write(virtualPath);

                                        WriteByteValues(offsetsWriter, offsetWritingPos, dataStartPos);
                                        WriteByteValues(offsetsWriter, offsetWritingPos + 4, fileSize);
                                        WriteByteValues(offsetsWriter, offsetWritingPos + 8, cmpFileSize);

                                        offsetWritingPos += 12;
                                        Console.WriteLine("Packed " + virtualPath);
                                    }
                                }

                                DEcmn.CheckAndDelFile(tmpCmpDataFile);
                            }
                        }
                    }
                }
            }
        }

        using (FileStream deviPack = new(deviPackFile, FileMode.Append, FileAccess.Write))
        {
            using (BinaryWriter deviPackWriter = new(deviPack))
            {
                deviPackWriter.BaseStream.Position = 0;
                byte[] archiveHeader = new byte[] { 68, 101, 118, 105, 80, 97, 99, 107, 46, 118, 49, 46, 53, 00, 00, 00 };
                deviPackWriter.Write(archiveHeader);

                AddNullBytes(deviPack, 16, 20);

                WriteByteValues(deviPackWriter, 16, (uint)fileCount);
                WriteByteValues(deviPackWriter, 20, 40);

                deviPack.Seek(deviPack.Length, SeekOrigin.Begin);

                using (FileStream packedPathsFile = new(tmpPathsFile, FileMode.Open, FileAccess.Read))
                {
                    ZlibCompress(packedPathsFile, tmpCmpDataFile, definedCmpLvl);

                    using (FileStream cmpPathsData = new(tmpCmpDataFile, FileMode.Open, FileAccess.Read))
                    {
                        cmpPathsData.CopyTo(deviPack);
                    }
                    using (FileStream packedOffsetsFile = new(tmpOffsetTableFile, FileMode.Open, FileAccess.Read))
                    {
                        packedOffsetsFile.CopyTo(deviPack);
                    }
                    using (FileStream packedDataFile = new(tmpDataFile, FileMode.Open, FileAccess.Read))
                    {
                        packedDataFile.CopyTo(deviPack);
                    }
                }

                var filePathsSize = (uint)new FileInfo(tmpPathsFile).Length;
                WriteByteValues(deviPackWriter, 28, filePathsSize);

                var cmpFilePathsSize = (uint)new FileInfo(tmpCmpDataFile).Length;
                WriteByteValues(deviPackWriter, 32, cmpFilePathsSize);

                var offsetTableStartPos = cmpFilePathsSize + 36;
                WriteByteValues(deviPackWriter, 20, offsetTableStartPos);

                var offsetTableFileSize = (uint)new FileInfo(tmpOffsetTableFile).Length;
                var dataStartPos = offsetTableStartPos + offsetTableFileSize;
                WriteByteValues(deviPackWriter, 24, dataStartPos);

                File.Delete(tmpPathsFile);
                File.Delete(tmpOffsetTableFile);
                File.Delete(tmpDataFile);
                File.Delete(tmpCmpDataFile);
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Finished packing files into the archive");
    }

    static void CheckUInt32Range(bool varToCheck)
    {
        switch (varToCheck)
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

    static void AddNullBytes(FileStream streamName, uint streamPos, uint byteCount)
    {
        streamName.Seek(streamPos, SeekOrigin.Begin);
        for (int b = 0; b < byteCount; b++)
        {
            streamName.WriteByte(0);
        }
    }

    enum CustomCmpLvls
    {
        c0 = CompressionLevel.NoCompression,
        c1 = CompressionLevel.Fastest,
        c2 = CompressionLevel.Optimal,
        c3 = CompressionLevel.SmallestSize
    }

    static void ZlibCompress(FileStream streamToCompress, string fileNameForDataOutFile, CompressionLevel setCmpLvl)
    {
        using FileStream zlibDataOut = new(fileNameForDataOutFile, FileMode.OpenOrCreate, FileAccess.Write);
        using ZLibStream compressor = new(zlibDataOut, setCmpLvl);
        streamToCompress.CopyTo(compressor);
    }

    static void WriteByteValues(BinaryWriter writerName, uint writerPos, uint varToAdjustWith)
    {
        writerName.BaseStream.Position = writerPos;
        var adjustValue = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(adjustValue, varToAdjustWith);
        writerName.Write(adjustValue);
    }
}