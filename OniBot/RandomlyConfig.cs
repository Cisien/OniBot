using System;
using System.Collections.Generic;
using System.Text;

namespace OniBot
{
    public class RandomlyConfig
    {
        public List<ImageMessage> RandomMessages { get; set; } = new List<ImageMessage>();
        public int MinMessages { get; set; } = 50;
        public int MaxMessages { get; set; } = 100;
    }
    public class ImageMessage
    {
        public string Message { get; set; }
        public string Image { get; set; }
    }
}
