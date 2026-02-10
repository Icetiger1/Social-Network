using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public interface ITopicRequestDto
    {
        string Title { get; }
        string Summary { get; }
        string TopicType { get; }
        LocationDto Location { get; }
    }
}
