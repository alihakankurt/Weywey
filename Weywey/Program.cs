using System;
using Weywey.Core;

namespace Weywey
{
    class Program
    {
        static void Main(string[] args)
            => new Bot().RunAsync().GetAwaiter().GetResult();
    }
}
