using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{
    public class ResultModel
    {
        public int Code { get; set; } = 200;

        public string Message { get; set; }

        public static ResultModel Success()
        {
            return new ResultModel();
        }


        public static ResultModel Error(string error)
        {
            return new ResultModel
            {
                Code = 500,
                Message = error
            };
        }
    }

    public class ResultModel<T> : ResultModel
    {
        public T Data { get; set; }

        public static ResultModel<T> Success(T t)
        {
            var data = new ResultModel<T>();

            data.Data = t;
            return data;
        }


        public new static ResultModel<T> Error(string error)
        {
            return new ResultModel<T>
            {
                Code = 500,
                Message = error
            };
        }
    }
}
