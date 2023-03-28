## Format Structure

**Note:** The path strings and the files which are packed inside the archive, uses the Zlib compression provided by the default System.IO.compression namespace. 
the compressed size value given in the table below is the size of the respective data when it is packed inside the archive.

#### Header Section
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x0 | 0x10 | String | Archive Header, with three null bytes |
| 0x10 | 0x4 | UInt32 | Total number of files |
| 0x14 | 0x4 | UInt32 | Offset table section start offset |
| 0x18 | 0x4 | UInt32 | Compressed files data section start offset |
| 0x1C | 0x4 | UInt32 | File Paths Uncompressed size |
| 0x20 | 0x4 | UInt32 | File Paths Compressed size |

#### Offset table section
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x0 | 0x4 | UInt32 | File start offset, relative from the data section |
| 0x4 | 0x4 | UInt32 | Uncompressed file size |
| 0x8 | 0x4 | UInt32 | Compressed file size |
