namespace watchdog_timer_console_demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Test WDT";
            var wdt = new WatchDogTimer { Interval = TimeSpan.FromMilliseconds(1000) };

            Console.WriteLine(DateTime.Now.ToLongTimeString());

            // "Update view 1 second after the last restart.
            for (int i = 0; i < 10; i++)
            {
                wdt.Restart(onRanToCompletion: ()=>MockUpdateView());
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }
            Console.ReadKey();
        }
        static void MockUpdateView()
        {
            Console.WriteLine($"Update now! WDT expired at {DateTime.Now.ToLongTimeString()}");
        }
    }
    class WatchDogTimer
    {
        int _wdtCount = 0;
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);
        public void Restart(Action onRanToCompletion)
        {
            _wdtCount++;
            var capturedCount = _wdtCount;
            Task
                .Delay(Interval)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    // If the 'captured' localCount has not changed after awaiting the Interval, 
                    // it indicates that no new 'bones' have been thrown during that interval.        
                    if (capturedCount.Equals(_wdtCount))
                    {
                        onRanToCompletion();
                    }
                });
        }
    }
}