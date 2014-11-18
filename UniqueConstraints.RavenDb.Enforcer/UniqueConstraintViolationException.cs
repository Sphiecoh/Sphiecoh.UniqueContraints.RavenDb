using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueConstraints.RavenDb.Enforcer
{
    internal class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException(string message)
            : base(message)
        {
        }
    }
}