using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Security.Cryptography;
using System.IO.Compression;
var input=Path.GetFullPath(args[0]); var output=Path.GetFullPath(args[1]); Directory.CreateDirectory(output);
using var stream=File.OpenRead(input); using var provider=MetadataReaderProvider.FromPortablePdbStream(stream); var reader=provider.GetMetadataReader();
var documents=new List<object>(); var custom=new List<object>(); int embedded=0,sourceLink=0;
foreach(var h in reader.Documents) {var d=reader.GetDocument(h);documents.Add(new{id=MetadataTokens.GetRowNumber(h),name=reader.GetString(d.Name),hashAlgorithm=reader.GetGuid(d.HashAlgorithm),hash=Convert.ToHexString(reader.GetBlobBytes(d.Hash)),language=reader.GetGuid(d.Language)});}
foreach(var h in reader.CustomDebugInformation) {
 var d=reader.GetCustomDebugInformation(h);var kind=reader.GetGuid(d.Kind);var bytes=reader.GetBlobBytes(d.Value);string? saved=null;string? originalPath=null;
 if(kind==Guid.Parse("0e8a571b-6926-466e-b4ad-8ab04611f5fe")) {
 embedded++; if(d.Parent.Kind==HandleKind.Document)originalPath=reader.GetString(reader.GetDocument((DocumentHandle)d.Parent).Name);
 if(bytes.Length<4)throw new InvalidDataException();int size=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes);byte[] data;
 if(size==0)data=bytes[4..];else{if(size<0||size>67108864)throw new InvalidDataException();using var ms=new MemoryStream(bytes,4,bytes.Length-4);using var ds=new DeflateStream(ms,CompressionMode.Decompress);data=new byte[size];ds.ReadExactly(data);if(ds.ReadByte()!=-1)throw new InvalidDataException();}
 Directory.CreateDirectory(Path.Combine(output,"embedded"));saved=$"embedded/document-{MetadataTokens.GetRowNumber(h):D6}.cs";File.WriteAllBytes(Path.Combine(output,saved),data);
 }
 if(kind==Guid.Parse("cc110556-a091-4d38-9fec-25ab9a351a6a")){sourceLink++;saved=$"sourcelink-{MetadataTokens.GetRowNumber(h):D6}.json";File.WriteAllBytes(Path.Combine(output,saved),bytes);}
 custom.Add(new{id=MetadataTokens.GetRowNumber(h),parentToken=MetadataTokens.GetToken(d.Parent),parentKind=d.Parent.Kind.ToString(),kind,length=bytes.Length,sha256=Convert.ToHexString(SHA256.HashData(bytes)),originalPath,saved});
}
File.WriteAllText(Path.Combine(output,"pdb-documents.json"),JsonSerializer.Serialize(new{input,sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input))),documentCount=documents.Count,customCount=custom.Count,embedded,sourceLink,documents,customDebugInformation=custom},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"Documents={documents.Count}; Custom={custom.Count}; Embedded={embedded}; SourceLink={sourceLink}");
