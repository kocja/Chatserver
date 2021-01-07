using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace chatserver.Models
{
    public class Message
    {
        [Key]
        public string id { get; set; }

        [ReadOnly(true)]
        public DateTime timestamp { get; set; }

        [Required]
        public string user_id { get; set; }

        [Required]
        public string message { get; set; }
    }
}