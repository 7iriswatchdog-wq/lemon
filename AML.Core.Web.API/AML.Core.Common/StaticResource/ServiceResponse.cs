using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Common.StaticResource
{
    public class ServiceResponse<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
        public string Information { get; set; }
    }
}
