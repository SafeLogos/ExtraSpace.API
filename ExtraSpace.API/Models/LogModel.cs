using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models
{
    [AutoDataManager.Table("Logs")]
    public class LogModel
    {
        [AutoDataManager.PrimaryKey]
        [AutoDataManager.NotInsert]
        [AutoDataManager.NotUpdate]
        public int Id { get; set; }
        public string LogType { get; set; }
        public string LogMessage { get; set; }
        public DateTime InsertDate { get; set; }

        public LogModel()
        {

        }
        public LogModel(LogTypes type, string message)
        {
            switch (type)
            {
                case LogTypes.INFO: LogType = "INFO";
                    break;
                case LogTypes.SUCCESS: LogType = "SUCCESS";
                    break;
                case LogTypes.ERROR: LogType = "ERROR";
                    break;
                default: LogType = "UNKNOWN";
                    break;
            }

            LogMessage = message;
            InsertDate = DateTime.Now;
        }

        public enum LogTypes
        {
            INFO,
            SUCCESS,
            ERROR
        }
    }
}
