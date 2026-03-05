using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Common.APIResponse
{
    public class JsonResponse<T>
    {
        public T Data { get; set; }
        public Int32 Status { get; set; }
        public string Message { get; set; }
    }
    public class Response
    {
        public Int32 StatusCode { get; set; }
        public string Message { get; set; }
    }
}
