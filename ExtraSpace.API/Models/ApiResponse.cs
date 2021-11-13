using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> DoMethod(Action<ApiResponse<T>> action)
        {
            ApiResponse<T> resp = new ApiResponse<T>();
            try
            {
                action(resp);
            }
            catch (ApiResponseException ex)
            {
                resp.Code = ex.Code;
                resp.Message = ex.Message;
            }
            catch (Exception e)
            {
                resp.Code = -1;
                resp.Message = "ERROR | " + e.Message;
            }

            return resp;
        }

        public void Throw(int code, string message) =>
            throw new ApiResponseException(code, message);

        public T GetResultIfNotError()
        {
            if (this.Code != 0)
                throw new ApiResponseException(this.Code, this.Message);
            return this.Data;
        }


        private class ApiResponseException : Exception
        {
            public int Code { get; set; }
            public ApiResponseException(int code, string message)
                :base(message)
            {
                Code = code;
            }
        }
    }
}
