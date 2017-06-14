using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeepBoopBot.Services
{
    public class ConsoleCountdown
    {
        public int RunTime { get; private set; }
        public int ElapsedTime { get; private set; } = 0;
        public int Interval { get; private set; }
        public bool Cancelable { get; private set; }
        public string Prompt { get; private set; }

        public event EventHandler CountdownCompleted;
        public event EventHandler CountdownCanceled;

        private string CancelablePrompt = "Press any key to continue. . .";

        private static readonly string Second = " second";
        private static readonly int maxTimeDisplayLength = (int.MaxValue / 1000).ToString().Length;

        private bool countdownStopped = false;
        private Timer timer;

        public ConsoleCountdown(int runTime, string prompt, int interval = 1000, bool cancelable = true)
        {
            RunTime = runTime;
            Interval = interval;
            Cancelable = cancelable;
            Prompt = prompt;

            timer = new Timer(HandleTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        public ConsoleCountdown(int runTime, string prompt, string cancelablePrompt, int interval = 1000, bool cancelable = true)
        {
            RunTime = runTime;
            Interval = interval;
            Cancelable = cancelable;
            Prompt = prompt;
            CancelablePrompt = cancelablePrompt;

            timer = new Timer(HandleTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected virtual void OnCountdownCompleted(EventArgs e)
        {
            CountdownCompleted?.Invoke(this, e);
        }

        protected virtual void OnCountdownCanceled(EventArgs e)
        {
            CountdownCanceled?.Invoke(this, e);
        }

        public void StartCountdown(int delay = 0)
        {
            if (Cancelable)
            {
                // Start the async task to cancel if input is detected
                CancelOnInput();
            }
            // Start the countdown.
            timer.Change(delay, Interval);
        }

        public void StopCountdown()
        {
            countdownStopped = true;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine();
        }

        private async void CancelOnInput()
        {
            while (!countdownStopped && !Console.KeyAvailable)
            {
                await Task.Delay(100); // Block this task to prevent overwork. // Questionable?
            }

            if (!countdownStopped) // A key was pressed.
            {
                StopCountdown();
                OnCountdownCanceled(EventArgs.Empty);
            }
        }

        private void HandleTimer(object stateInfo)
        {
            if (ElapsedTime < RunTime)
            {
                int remainingTime = (RunTime - ElapsedTime) / 1000;
                string plural = remainingTime != 1 ? "s. " : ". ";

                string prompt = $"\r{Prompt}{remainingTime}{Second}{plural}";
                int promptRealLength = 0;
                if (Cancelable)
                {
                    prompt += CancelablePrompt;
                    promptRealLength = prompt.Length;
                    prompt = prompt.PadRight(Prompt.Length + maxTimeDisplayLength + Second.Length + plural.Length + CancelablePrompt.Length);
                }
                else
                {
                    promptRealLength = prompt.Length;
                    prompt = prompt.PadRight(Prompt.Length + maxTimeDisplayLength + Second.Length + plural.Length);
                }
                Console.Write(prompt);
                // -1 for zero based indexing.
                Console.CursorLeft = promptRealLength - 1;
                ElapsedTime += Interval;
            }
            else
            {
                // Successful countdown, not canceled.
                StopCountdown();
                OnCountdownCompleted(EventArgs.Empty);
            }
        }
    }
}
