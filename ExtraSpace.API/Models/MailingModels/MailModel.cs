using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.MailingModels
{
    public class MailModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public MailModel()
        {

        }

        public MailModel(string from, string to, string title, string body)
        {
            this.From = from;
            this.To = to;
            this.Title = title;
            this.Body = body;
        }
    }
}
