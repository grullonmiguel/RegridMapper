using RegridMapper.Core.Configuration;

namespace RegridMapper.Models
{
    public class US_State
    {
        public State StateID { get; set; }

        public string? Name { get; set; }

        public List<US_County>? Counties { get; set; }
    }
}
