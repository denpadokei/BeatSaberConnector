using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberConnector.Models
{
    public class SendedEvetArgs : EventArgs
    {
        public string Message { get; }
        public SendedEvetArgs(string message)
        {
            Message = message;
        }
    }
}
