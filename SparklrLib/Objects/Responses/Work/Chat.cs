﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparklrLib.Objects.Responses.Work
{
    public class Chat
    {
        public int to { get; set; }
        public int from { get; set; }
        public int time { get; set; }
        public string message { get; set; }
    }
}
