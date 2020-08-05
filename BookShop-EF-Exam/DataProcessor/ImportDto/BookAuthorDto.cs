
using Newtonsoft.Json;

namespace BookShop.DataProcessor.ImportDto
{
    public class BookAuthorDto
    { 
        [JsonProperty("Id")]
        public int? BookId { get; set; }
    }
}
