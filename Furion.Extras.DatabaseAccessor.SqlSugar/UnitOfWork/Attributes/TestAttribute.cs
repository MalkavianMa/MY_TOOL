using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TestAttribute: Attribute
    {
        public string Name { get; set; }
    }
}
