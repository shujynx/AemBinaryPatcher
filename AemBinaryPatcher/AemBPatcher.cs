using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace AemBinaryPatcher
{
    public class AemBPatcher
    {
        public static string path;
        public static string ogFile;
        public static string modFile;
        public static long ogLenght;

        public static string patchString;
        public static int offset = 0;

        static void Main(string[] args)
        {
            if (args.Length < 3)
                Console.WriteLine("How to use:\n" +
                "AemBinaryPatcher.exe (path to file inside of the games archives)" +
                " (path to the original file) " +
                "(path to the modified file)");
            else
            {
                #region VariableDeclares
                path = args[0];
                ogFile = args[1];
                modFile = args[2];

                FileStream ogStream = File.OpenRead(ogFile);
                ogLenght = ogStream.Length;
                ogStream.Close();
                #endregion
                #region PreliminaryActions
                if (File.Exists(Path.GetFileName(path) + ".bp")) File.Delete(Path.GetFileName(path) + ".bp");
                FileStream patchFileStream = File.OpenWrite(Path.GetFileName(path) + ".bp");
                StreamWriter patchFile = new StreamWriter(patchFileStream);
                patchFile.WriteLine("{\n  \"Version\": 1,\n  \"Patches\": [\n");
                patchFile.Close();
                patchFileStream.Close();
                #endregion
                #region WritingPatch
                while (offset < ogLenght)
                {
                    FileStream originalFileStream = File.OpenRead(ogFile);
                    FileStream modifiedFileStream = File.OpenRead(modFile);
                    BinaryReader originalFile = new BinaryReader(originalFileStream);
                    BinaryReader modifiedFile = new BinaryReader(modifiedFileStream);

                    originalFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                    modifiedFile.BaseStream.Seek(offset, SeekOrigin.Begin);

                    if (originalFile.ReadByte() != modifiedFile.ReadByte())
                    {
                        modifiedFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                        patchString += $"    {{\n      \"file\": \"{path}\",\n      \"offset\": {offset},\n      \"data\": \"{modifiedFile.ReadByte()}\"\n    }},\n";
                    }
                    originalFile.Close();
                    originalFileStream.Close();
                    modifiedFile.Close();
                    modifiedFileStream.Close();
                    offset++;
                }
                #endregion
                string comma = ",";
                var ix = patchString.LastIndexOf(comma);
                patchString = patchString.Substring(0, ix) + patchString.Substring(ix + comma.Length);
                FileStream pfs = new FileStream(Path.GetFileName(path) + ".bp", FileMode.Append);
                StreamWriter pfr = new StreamWriter(pfs);
                pfr.Write(patchString);
                pfr.Write("\n  ]\n}");
                pfr.Close();
                pfs.Close();
            }
        }
    }
}
