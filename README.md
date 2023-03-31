# DeviPackUnpackTool

This C# app allows you to make a simple archive file for a folder that contains files and sub folders. the files are compressed when it is packed into the 
archive file with zlib compression and you have three levels of compression to choose from when making the archive file. 

The generated archive file will have the same name of the folder that was packed with ``.devi`` as the file extension.

This app comes with option to decompress the ``.devi`` archive file, decompress a specific file from the archive as well as get all the file path strings of files stored in the archive in a text file.

# Functions
The app has to be run from a command prompt with a valid argument switch. the list of valid switches that you can use with this app are given below.
<br>``-p`` Pack a folder with files to a devi archive file
<br>``-u`` Unpack a devi archive file
<br>``-uf`` Unpack a specific file from the devi archive file
<br>``-up`` Unpack all file paths from the archive to a text file

<br>When using the pack function ``-p``, you will have to specify a compression level argument switch. the valid compression level switches that you can use with this app are given below:
<br>``-c0`` No compression. not recommended and will make the generated archive slightly bigger.
<br>``-c1`` Fastest compression
<br>``-c2`` Optimal compression
<br>``-c3`` Smallest size

Usage examples:
<br>To Pack a folder: ``DeviPackUnpackTool.exe -p "Folder To pack" -c3``
<br>To Unpack a file: ``DeviPackUnpackTool.exe -u "archiveFile.devi"``
<br>To Unpack a single file: ``DeviPackUnpackTool.exe -uf "archiveFile.devi" "MyStuff\TestFiles\Readme.pdf"``
<br>To Unpack file paths: ``DeviPackUnpackTool.exe -up "archiveFile.devi"``

Optionally, you can also run the app from a command prompt with the ``-?`` or ``-h`` argument switches to display the valid arguments that you can use 
with this app.

There is also a framework version of this app which you can get from this link below:


# For Developers
Refer to the [Format Structure](https://github.com/Surihix/DeviPackUnpackTool/blob/master/FormatStruct.md) page to learn more about the structure of the 
archive file made with this app.
