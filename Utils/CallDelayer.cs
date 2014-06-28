using System;
using System.Threading;

namespace WF.Player
{
	/// <summary>
	/// Call delayer for function calls.
	/// </summary>
	public class CallDelayer
	{
		System.Threading.Timer _timer;
		DateTime _lastCall;
		int _minMillisecondsToWait = 25;
		int _maxMillisecondsToWait = 500;
		Action<object> _func;
		object _object;

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.CallDelayer"/> class.
		/// </summary>
		/// <param name="func">Func to be called.</param>
		public CallDelayer(Action<object> func) : this(25, 500, func)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.CallDelayer"/> class.
		/// </summary>
		/// <param name="minMillisecondsToWait">Minimum milliseconds to wait before the function is called.</param>
		/// <param name="maxMillisecondsToWait">Max milliseconds to wait before the function is called.</param>
		/// <param name="func">Func to be called.</param>
		public CallDelayer(int minMillisecondsToWait, int maxMillisecondsToWait, Action<object> func)
		{
			_timer = new System.Threading.Timer(TimerTick, null, Timeout.Infinite, Timeout.Infinite);
			_lastCall = DateTime.MaxValue;
			_minMillisecondsToWait = minMillisecondsToWait;
			_maxMillisecondsToWait = maxMillisecondsToWait;
			_func = func;
		}

		/// <summary>
		/// Start the function with the given parameter after minMillisecondsToWaitExecute 
		/// or now, if _maxMillisecondsToWait is exceeded.
		/// </summary>
		/// <param name="param">Parameter for the function.</param>
		public void Call(object param = null)
		{
			_object = param;

			if(_lastCall != null && (DateTime.Now-_lastCall).TotalMilliseconds > _maxMillisecondsToWait) {
				TimerTick(null);
				return;
			}

			_timer.Change(_minMillisecondsToWait, Timeout.Infinite);
		}

		/// <summary>
		/// Abort any calls of the function.
		/// </summary>
		public void Abort()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			_lastCall = DateTime.MaxValue;
		}

		/// <summary>
		/// Tick of timer.
		/// </summary>
		/// <param name="state">State of the timer given at creation.</param>
		void TimerTick(object state)
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			_lastCall = DateTime.Now;

			_func(_object);
		}
	}
}

