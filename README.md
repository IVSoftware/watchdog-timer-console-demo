In my own experience I've dealt with lots of scenarios just like the one you describe, e.g. _update something one second after the mouse stops moving_ etc. 

For a long time I would do timer restarts just the way you describe, by cancelling an old task and starting a new one. But I never really liked how messy that was, so I came up with an alternative that I use in production code. Long-term it has proven quite reliable. It takes advantage of the captured context associated with a task. Multiple instances of `TaskCanceledException` no longer occur.

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

Another nice perk is that it doesn't rely on platform timers and works just as well in iOS/Android as it does in WinForms/WPF.
***
For purposes of demonstration, this can be exercised in a quick console demo where the `MockUpdateView()` action is sent to the WDT 10 times at 500 ms intervals. It will only execute one time, 1 second after the last restart is received.

    static void Main(string[] args)
    {
        Console.Title = "Test WDT";
        var wdt = new WatchDogTimer { Interval = TimeSpan.FromSeconds(1) };

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
***
So, with 500 ms times 10 restarts this verifies one event at ~5 seconds from the start.

[![console output][1]][1]


  [1]: https://i.stack.imgur.com/41KWl.png