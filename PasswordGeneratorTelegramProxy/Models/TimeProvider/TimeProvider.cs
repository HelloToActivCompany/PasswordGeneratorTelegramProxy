using System;

namespace PasswordGeneratorTelegramProxy.Models
{
	public abstract class TimeProvider
	{
		private static TimeProvider _current;

		static TimeProvider()
		{
			_current = new DefaultTimeProvider();
		}

		public static TimeProvider Current
		{
			get { return _current; }

			set
			{
				if (_current == null)
				{
					throw new ArgumentNullException("value");
				}

				_current = value; 
			}
		}

		public abstract DateTime Now { get; }

		public static void ResetToDefault()
		{
			_current = new DefaultTimeProvider();
		}
	}
}