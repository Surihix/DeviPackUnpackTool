# DeviPackUnpackTool

This C# app allows you to make a simple archive file for a folder that contains files and sub folders. the files are compressed when it is packed into the 
archive file with zlib compression and you have three levels of compression to choose from when making the archive file. 

The generated archive file will have the same name of the folder that was packed with ``.devi`` as the file extension.

This app comes with option to decompress the ``.devi`` archive file as well as decompress and get all the file path strings of files stored in the archive
in a text file.


# For Developers
Refer to this [Format Structure](https://github.com/Surihix/DeviPackUnpackTool/blob/master/FormatStruct.md) page to learn more about the structure of the archive file made with this tool.
