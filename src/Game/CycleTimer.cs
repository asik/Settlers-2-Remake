using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Game {

    static class CycleTimer {

        static Stopwatch stopWatch = new Stopwatch();
        static double tickPeriod = 1.0 / (double)Stopwatch.Frequency;
        static double updatePeriod; // in seconds
        static double nextUpdateMaxDuration; // in seconds
        static TimerEvent timerEvent;

        public static void Start(int updateFrequency) {
            stopWatch.Reset();
            Debug.WriteLine("CycleTimer is using a " + 
                (Stopwatch.IsHighResolution ? "high resolution " : "low resolution ") +
                "timer, at " + Stopwatch.Frequency + " ticks per second.");
            updatePeriod = 1.0 / (double)updateFrequency;
            nextUpdateMaxDuration = updatePeriod;
            timerEvent = new TimerEvent { MillisecondsElapsed = 0 };
            stopWatch = Stopwatch.StartNew();
        }

        public static void Update() {
            double thisUpdateElapsedTime = (double)stopWatch.ElapsedTicks / (double)Stopwatch.Frequency;
            double timeLeft = nextUpdateMaxDuration - thisUpdateElapsedTime;
            if (timeLeft <= 0) {
                nextUpdateMaxDuration = updatePeriod + timeLeft;
                stopWatch.Reset();
                stopWatch.Start();
                timerEvent.MillisecondsElapsed = (float)thisUpdateElapsedTime * 1000.0f;
                if (Trigger != null) {
                    Trigger(null, timerEvent);
                }
            }
        }

        public static bool IsCatchingUp() {
            return nextUpdateMaxDuration < 0.9f * updatePeriod;
        }

        public static event EventHandler<TimerEvent> Trigger;
    }

    public class TimerEvent : EventArgs {
        public float MillisecondsElapsed;
    }
}
