using System;
using System.Threading.Tasks;

namespace Common
{
    public class RetryUtil
    {
        public RetryUtil()
        {
        }

        public async Task Retry(Func<Task> func, int retryTime = 3, int waitTime = 500)
        {
            for (var i = 0; i < retryTime - 1; i++)
            {
                try
                {
                    await func();
                    return;
                }
                catch (System.Exception ex)
                {
                    await Task.Delay(waitTime);
                }
            }
            await func();
        }

        public async Task<T> Retry<T>(Func<Task<T>> func, int retryTime = 3, int waitTime = 500)
        {
            for (var i = 0; i < retryTime - 1; i++)
            {
                try
                {
                    return await func();
                }
                catch (System.Exception ex)
                {
                    await Task.Delay(waitTime);
                }
            }
            return await func();
        }
    }
}