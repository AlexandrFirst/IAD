using CsvHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1.Model
{
    public abstract class BaseModel
    {
        public virtual string Data { get; set; }
    }
}
