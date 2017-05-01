using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordGeneratorTelegramProxy.Models
{
    public interface IElapsed
    {
        event EventHandler Elapsed;  
    }
}
