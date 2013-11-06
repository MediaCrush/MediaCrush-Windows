using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCrush
{
    public class UploadingFile
    {
        public UploadingFile(string fileName)
        {
            File = fileName;
        }

        public string File { get; set; }
    }
}
