using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueConstraints.RavenDb.Enforcer
{
    public class UniqueConstraint
    {
        public string UniqueProperty { get; set; }

        public string UniquePropertyValue { get; set; }
    }
}