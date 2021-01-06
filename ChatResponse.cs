using Microsoft.AspNetCore.Mvc;
using System;

namespace chatserver
{

    public class ChatResponse : JsonResult
    {
        public ChatResponse(object value) : base(value)
        {
        }

        public static ChatResponse Error(string message)
        {
            ChatResponse response = new ChatResponse(new
            {
                message = message
            })
            {
                StatusCode = 400
            };
            return response;
        }

        public static ChatResponse Error(Exception error)
        {
            string message;
            string stacktrace;
            if (error.InnerException != null)
            {
                message = error.InnerException.Message;
                stacktrace = error.InnerException.StackTrace;
            }
            else
            {
                message = error.Message;
                stacktrace = error.StackTrace;
            }

            ChatResponse response = new ChatResponse(new
            {
                message = message,
                statcktrace = stacktrace
            })
            {
                StatusCode = 400
            };
            return response;
        }

        public static ChatResponse Ok(dynamic data)
        {
            ChatResponse response = new ChatResponse(data)
            {
                StatusCode = 200
            };
            return response;
        }
    }
}