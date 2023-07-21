using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yugioh_card_scraper.Utils
{
    public static class LinearBackoff
    {
        public static async Task<T?> DoRequest<T>(Func<Task<T>> taskRequest, int maxDelay)
        {
            var requestDelay = 0;

            while (true)
            {
                try
                {
                    var result = await taskRequest();
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e}\n");
                }

                requestDelay += 1000;
                if (requestDelay > maxDelay)
                    break;

                await Task.Delay((int)requestDelay);
            }

            return default;
        }
    }
}
