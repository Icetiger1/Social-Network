using Application.Topics.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Topics.Dtos;

public record UpdateTopicRequestDto(
    string Title,
    DateTime? EventStart,
    string Summary,
    string TopicType,
    LocationDto Location
);
