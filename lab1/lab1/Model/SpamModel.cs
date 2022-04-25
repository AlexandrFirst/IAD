using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1.Model
{
    internal class SpamModel: BaseModel
    {
        [Name("Category")]
        public string wordsType { get; set; }
        [Name("Message")]
        public override string Data { get; set; }
    }
}
