using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbedAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            // Usage: EmbedAssembly <target assembly> <assemblies...>
            // Simple tool that zips up deps and embeds them into the installer so we can have a nice one-file installer
            AssemblyDefinition target;
            using (var stream = File.OpenRead(args[0]))
                target = AssemblyDefinition.ReadAssembly(stream);
            for (int i = 1; i < args.Length; i++)
            {
                var memStream = new MemoryStream();
                using (var stream = File.OpenRead(args[i]))
                {
                    using (var gStream = new GZipStream(memStream, CompressionMode.Compress))
                        stream.CopyTo(gStream);
                }
                var data = memStream.ToArray();
                target.MainModule.Resources.Add(new EmbeddedResource(Path.GetFileName(args[i]), ManifestResourceAttributes.Public, data));
            }
            using (var stream = File.Create(args[0]))
                target.Write(stream);
        }
    }
}
