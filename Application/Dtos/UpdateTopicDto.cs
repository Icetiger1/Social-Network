using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos;

public record UpdateTopicDto(
    string Title,
    string Summary,
    string TopicType,
    LocationDto Location,
    DateTime? EventStart
) : ITopicRequestDto;
