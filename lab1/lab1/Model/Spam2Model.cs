using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1.Model
{
    internal class Spam2Model : BaseModel
    {
        [Name("v2")]
        public override string Data { get; set; }
    }
}
