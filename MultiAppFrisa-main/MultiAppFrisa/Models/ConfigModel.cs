using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO. Verify if Cancel_Executed forces to start from id=0
namespace PredictorV2.Models
{
    public class ConfigModel
    {
        static int id = 0;
        public ConfigModel()
        {
            Date = DateTime.Now;
            Id = id++;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        // [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        // [MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        // [MaxLength(64)]
        public string Mail { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        // [MaxLength(64)]
        public string IdMessenger { get; set; }

        /// <summary>
        /// Gets or sets the day of birth.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <remarks>Is a blob in the database.</remarks>
        public byte[] Picture { get; set; }
    }
}
