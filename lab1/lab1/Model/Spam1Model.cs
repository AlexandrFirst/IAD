using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1.Model
{
    internal class Spam1Model: BaseModel
    {
        [Name("email")]
        public override string Data { get; set; }
    }
}
