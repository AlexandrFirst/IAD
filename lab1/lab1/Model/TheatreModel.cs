using CsvHelper.Configuration.Attributes;
using lab1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class TheatreModel: BaseModel
    {
        [Name("DESCRIPTION")]
        public override string Data { get; set; }
    }
}
