﻿namespace DBA
{
    public class FileModel
    {
        public int id { get; set; }
        public string? filename { get; set; }
        public string? action { get; set; }
        public DateOnly date { get; set; }
    }
}
