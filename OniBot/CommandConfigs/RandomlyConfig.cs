using System;
using System.Collections.Generic;
using System.Text;

namespace OniBot.CommandConfigs
{
    public class RandomlyConfig
    {
        public List<ImageMessage> RandomMessages { get; set; } = new List<ImageMessage>();
        public int MinMessages { get; set; }
        public int MaxMessages { get; set; }
    }
    public class ImageMessage
    {
        public string Message { get; set; }
        public string Image { get; set; }
    }
}
