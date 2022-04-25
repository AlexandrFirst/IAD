using CsvHelper;
using CsvHelper.Configuration.Attributes;
using lab1.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class AutoModel: BaseModel
    {
        [Name("Violation Description")]
        public override string Data { get; set; }
    }
}
