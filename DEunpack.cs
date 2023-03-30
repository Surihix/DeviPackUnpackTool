using System.IO.Compression;
using System.Text;

namespace DeviPackUnpackTool;

internal class DEunpack
{
    // Unpack all files
    public static void UnpackFiles(string inFile)
    {
        CreateExtractDir(inFile, out string extractDir);


        Console.WriteLine("Unpacking files....");
        Console.WriteLine("");

        using (FileStream deviFile = new(inFile, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader deviFileReader = new(deviFile))
            {
                CheckArchiveHeader(deviFileReader);

                ReadByteValue(deviFileReader, 16, out uint fileCount);
                ReadByteValue(deviFileReader, 20, out uint offsetTablePos);
                ReadByteValue(deviFileReader, 24, out uint dataStartPos);
                ReadByteValue(deviFileReader, 32, out uint pathsCmpSize);

                using (MemoryStream pathStream = new())
                {
                    deviFile.CopyTo(pathStream, 36, pathsCmpSize);

                    using (MemoryStream dcmpPathStream = new())
                    {
                        pathStream.Seek(0, SeekOrigin.Begin);
                        ZlibDecompress(pathStream, dcmpPathStream);

                        using (BinaryReader dcmpPathReader = new(dcmpPathStream))
                        {

                            uint pathReaderPos = 0;
                            uint offsetTblReaderPos = 0;
                            for (int f = 0; f < fileCount; f++)
                            {
                                var mainFilePath = "";

                                FileNamesBuilder(dcmpPathReader, pathReaderPos, ref mainFilePath);

                                ReadByteValue(deviFileReader, offsetTablePos + offsetTblReaderPos, out var fileStartPos);
                                ReadByteValue(deviFileReader, offsetTablePos + offsetTblReaderPos + 8, out var fileCmpSize);

                                var directoryOfFile = Path.GetDirectoryName(mainFilePath);
                                var fileName = Path.GetFileName(mainFilePath);
                                var finalOutFilePath = extractDir + directoryOfFile + "\\" + fileName;

                                // Check if the directory of the file does not exist
                                // If it does not exist, then check if the directory
                                // name string is not null and if the directory 
                                // string equals "", then create a directory
                                if (!Directory.Exists(directoryOfFile))
                                {
                                    if (directoryOfFile is not null)
                                    {
                                        if (!directoryOfFile.Equals(""))
                                        {
                                            Directory.CreateDirectory(extractDir + directoryOfFile);
                                        }
                                    }
                                }

                                if (File.Exists(finalOutFilePath))
                                {
                                    File.Delete(finalOutFilePath);
                                }

                                using (FileStream outFile = new(finalOutFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    using (MemoryStream cmpFileData = new())
                                    {
                                        deviFile.CopyTo(cmpFileData, dataStartPos + fileStartPos, fileCmpSize);

                                        cmpFileData.Seek(0, SeekOrigin.Begin);
                                        ZlibDecompress(cmpFileData, outFile);
                                    }
                                }

                                Console.WriteLine("Unpacked " + mainFilePath);

                                pathReaderPos = (uint)dcmpPathReader.BaseStream.Position;
                                offsetTblReaderPos += 12;
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Finished unpacking files");
    }

    // Unpack a single file
    public static void UnpackSingleFile(string inFile, string specificFilePath)
    {
        CreateExtractDir(inFile, out string extractDir);


        Console.WriteLine("Unpacking a single file....");
        Console.WriteLine("");

        using (FileStream deviFile = new(inFile, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader deviFileReader = new(deviFile))
            {
                CheckArchiveHeader(deviFileReader);

                ReadByteValue(deviFileReader, 16, out uint fileCount);
                ReadByteValue(deviFileReader, 20, out uint offsetTablePos);
                ReadByteValue(deviFileReader, 24, out uint dataStartPos);
                ReadByteValue(deviFileReader, 32, out uint pathsCmpSize);

                using (MemoryStream pathStream = new())
                {
                    deviFile.CopyTo(pathStream, 36, pathsCmpSize);

                    using (MemoryStream dcmpPathStream = new())
                    {
                        pathStream.Seek(0, SeekOrigin.Begin);
                        ZlibDecompress(pathStream, dcmpPathStream);

                        using (BinaryReader dcmpPathReader = new(dcmpPathStream))
                        {

                            uint pathReaderPos = 0;
                            uint offsetTblReaderPos = 0;
                            for (int f = 0; f < fileCount; f++)
                            {
                                var mainFilePath = "";

                                FileNamesBuilder(dcmpPathReader, pathReaderPos, ref mainFilePath);

                                ReadByteValue(deviFileReader, offsetTablePos + offsetTblReaderPos, out var fileStartPos);
                                ReadByteValue(deviFileReader, offsetTablePos + offsetTblReaderPos + 8, out var fileCmpSize);

                                // If the main file path equals the specific file
                                // path, then do the following:
                                // Check if the directory of the file does not exist
                                // If it does not exist, then check if the directory
                                // name string is not null and if the directory 
                                // string equals "", then create a directory
                                if (mainFilePath.Equals(specificFilePath))
                                {
                                    var directoryOfFile = Path.GetDirectoryName(mainFilePath);
                                    var fileName = Path.GetFileName(mainFilePath);
                                    var finalOutFilePath = extractDir + directoryOfFile + "\\" + fileName;

                                    if (!Directory.Exists(directoryOfFile))
                                    {
                                        if (directoryOfFile is not null)
                                        {
                                            if (!directoryOfFile.Equals(""))
                                            {
                                                Directory.CreateDirectory(extractDir + directoryOfFile);
                                            }
                                        }
                                    }

                                    if (File.Exists(finalOutFilePath))
                                    {
                                        File.Delete(finalOutFilePath);
                                    }

                                    using (FileStream outFile = new(finalOutFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        using (MemoryStream cmpFileData = new())
                                        {
                                            deviFile.CopyTo(cmpFileData, dataStartPos + fileStartPos, fileCmpSize);

                                            cmpFileData.Seek(0, SeekOrigin.Begin);
                                            ZlibDecompress(cmpFileData, outFile);
                                        }
                                    }

                                    Console.WriteLine("Unpacked " + mainFilePath);
                                }

                                pathReaderPos = (uint)dcmpPathReader.BaseStream.Position;
                                offsetTblReaderPos += 12;
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Finished unpacking file");
    }


    // Unpack file paths
    public static void UnpackFilePaths(string inFile)
    {
        var inFilePath = Path.GetFullPath(inFile);
        var extractDir = Path.GetDirectoryName(inFilePath) + "\\";
        var txtFileName = Path.GetFileNameWithoutExtension(inFile) + ".txt";
        var outTxtFile = extractDir + txtFileName;

        DEcmn.CheckAndDelFile(outTxtFile);


        Console.WriteLine("Unpacking file paths....");
        Console.WriteLine("");

        using (FileStream deviFile = new(inFile, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader deviFileReader = new(deviFile))
            {
                CheckArchiveHeader(deviFileReader);

                ReadByteValue(deviFileReader, 16, out uint fileCount);
                ReadByteValue(deviFileReader, 32, out uint pathsCmpSize);

                using (MemoryStream pathStream = new())
                {
                    deviFile.CopyTo(pathStream, 36, pathsCmpSize);

                    using (MemoryStream dcmpPathStream = new())
                    {
                        pathStream.Seek(0, SeekOrigin.Begin);
                        ZlibDecompress(pathStream, dcmpPathStream);

                        using (BinaryReader dcmpPathReader = new(dcmpPathStream))
                        {
                            using (StreamWriter pathsWriter = new(outTxtFile, append: true))
                            {

                                uint pathReaderPos = 0;
                                for (int f = 0; f < fileCount; f++)
                                {
                                    var MainFilePath = "";

                                    FileNamesBuilder(dcmpPathReader, pathReaderPos, ref MainFilePath);

                                    pathsWriter.WriteLine(MainFilePath);

                                    pathReaderPos = (uint)dcmpPathReader.BaseStream.Position;
                                }
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Unpacked file paths to " + @"""" + txtFileName + @"""" + " file");
    }


    static void CreateExtractDir(string inFileVar, out string extractDirVar)
    {
        var inFilePath = Path.GetFullPath(inFileVar);
        var inFileDir = Path.GetDirectoryName(inFilePath);
        extractDirVar = inFileDir + "\\" + Path.GetFileNameWithoutExtension(inFileVar) + "\\";

        if (Directory.Exists(extractDirVar))
        {
            Directory.Delete(extractDirVar, true);
        }
        Directory.CreateDirectory(extractDirVar);
    }


    static void CheckArchiveHeader(BinaryReader readerName)
    {
        readerName.BaseStream.Position = 0;
        var getArchiveHeader = readerName.ReadChars(16);
        var archiveHeader = string.Join("", getArchiveHeader).Replace("\0", "");

        bool checkArchiveHeader = archiveHeader.StartsWith("DeviPack.v1.5");
        switch (checkArchiveHeader)
        {
            case true:
                break;

            case false:
                DEcmn.ErrorExit("Error: This is not a valid DeviPack archive file");
                break;
        }
    }

    static void ReadByteValue(BinaryReader readerName, uint readerPos, out uint outVariable)
    {
        readerName.BaseStream.Position = readerPos;
        outVariable = readerName.ReadUInt32();
    }

    static void ZlibDecompress(Stream streamToDecompress, Stream streamToHoldDcmpData)
    {
        using (ZLibStream zlibDataDcmp = new(streamToDecompress, CompressionMode.Decompress))
        {
            zlibDataDcmp.CopyTo(streamToHoldDcmpData);
        }
    }

    static void FileNamesBuilder(BinaryReader readerName, uint readerPos, ref string varName)
    {
        readerName.BaseStream.Position = readerPos;
        var fileNameBuilder = new StringBuilder();
        char stringChars;
        while ((stringChars = readerName.ReadChar()) != default)
        {
            fileNameBuilder.Append(stringChars);
        }
        varName = fileNameBuilder.ToString();
    }
}