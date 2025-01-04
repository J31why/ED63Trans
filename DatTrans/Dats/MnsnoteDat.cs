using System.Text;
namespace DatTrans.Dats;

public class MnsnoteDatItem
{
    public uint Id;
    public uint Length;
    public byte[] Verify= new byte[8];
}

public static class MnsnoteDat
{
    public static void Generate(string noteFile,string translatedMsDir)
    {
        var fs = new FileStream(noteFile, FileMode.Open, FileAccess.Read);
        var items = Parse(fs);
        fs.Dispose();
        fs = new FileStream(Path.GetFileName(noteFile), FileMode.Create, FileAccess.Write);
        
        var files = Directory.EnumerateFiles(translatedMsDir).ToArray();
        for (var i = 0; i < items.Count-1; i++)
        {
            var item = items[i];
            var fileData = File.ReadAllBytes(files[i]);
            if (!Verify(fileData,item.Verify)) //确实没毛病
            {
                Console.WriteLine($"{i}| {files}");
            }
            item.Length = (uint)fileData.Length;
            fs.WriteUint(item.Id);
            fs.WriteUint(item.Length);
            fs.Write(fileData);
        }
        fs.Write([0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff]);
        fs.Flush();
        fs.Dispose();
    }

    private static bool Verify(byte[] data1, byte[] data2) =>
        data1[0] == data2[0] && data1[1] == data2[1] &&
        data1[2] == data2[2] && data1[3] == data2[3] &&
        data1[4] == data2[4] && data1[5] == data2[5] &&
        data1[6] == data2[6] && data1[7] == data2[7];


    private static List<MnsnoteDatItem> Parse(FileStream fs)
    {
        var items = new List<MnsnoteDatItem>();
        while (fs.Position < fs.Length)
        {
            var item = new MnsnoteDatItem
            {
                Id = fs.ReadUint(),
                Length = fs.ReadUint(),
            };
     
            items.Add(item);
            for (int i = 0; i < 8; i++)
            {
                item.Verify[i] = (byte)fs.ReadByte();
            }
            fs.Seek(item.Length - 8, SeekOrigin.Current);
        }

        return items;
    }
}