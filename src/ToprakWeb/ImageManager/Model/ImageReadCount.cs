namespace ToprakWeb.ImageManager
{
    using System;

    using ToprakWeb.ImageManager.Model;

    public class ImageReadCount : IDbResult
    {
        public string Image { get; set; }

        public int Count { get; set; }
    }
}