﻿namespace Shrooms.Presentation.WebViewModels.Models.Map
{
    public class MapRoomTypeViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string IconId { get; set; }

        public string Color { get; set; }

        public MapRoomTypeViewModel()
        {
            this.Color = "#FFFFFF";
        }
    }
}