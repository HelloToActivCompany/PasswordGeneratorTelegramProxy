using System;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class DefaultTimeProvider : TimeProvider
	{
		public override DateTime Now => DateTime.Now;
	}
}