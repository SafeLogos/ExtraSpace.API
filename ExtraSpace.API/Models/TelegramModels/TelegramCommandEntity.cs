using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.TelegramModels
{
    public class TelegramCommandEntity
    {
        public string Command { get; set; }
        public string CommandTextToView { get; set; }
        public string Description { get; set; }
        public Func<UpdateModel, ApiResponse<bool>> Action { get; set; }

        public TelegramCommandEntity(string command, string commandTextToView, string description, Func<UpdateModel, ApiResponse<bool>> action)
        {
            this.Command = command;
            this.CommandTextToView = commandTextToView;
            this.Description = description;
            this.Action = action;
        }
    }
}
