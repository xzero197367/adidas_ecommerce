using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Common_DTOs
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        private Result(bool isSuccess, string error) => (IsSuccess, Error) = (isSuccess, error);

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
    }

}
